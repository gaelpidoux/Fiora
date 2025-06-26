using Xunit;
using FluentAssertions;
using Moq;
using fiora;


namespace FioraUnitTest
{
    public class ScoreCalculatorTests
    {
        private readonly ScoreCalculator _calculator;
        public ScoreCalculatorTests()
        {
            _calculator = new ScoreCalculator();
        }

        /// <summary>
        /// Jeux de données de test : séparés par catégorie pour plus de lisibilité.
        /// </summary>
        #region MemberData
        public static class ScoreTestData
        {
            #region Raccourcis pour les résultats
            private static MatchResult W => new(MatchResult.Result.Win);
            private static MatchResult D => new(MatchResult.Result.Draw);
            private static MatchResult L => new(MatchResult.Result.Loss);
            #endregion

            // --------------- 1-4 : Fonctionnement « de base » ----------------
            public static IEnumerable<object[]> Basic =>
                new List<object[]>
                {
                // 1. Calcul simple : Win, Draw, Loss → 4 points
                new object[] { new[] { W, D, L }, 4 },

                // 2. Que des victoires : Win, Win → 6 points
                new object[] { new[] { W, W }, 6 },

                // 3. Que des nuls : Draw ×3 → 3 points
                new object[] { new[] { D, D, D }, 3 },

                // 4. Que des défaites : Loss ×2 → 0 point
                new object[] { new[] { L, L }, 0 },
                };

            // --------------- 5-9 : Bonus de série ---------------------------
            public static IEnumerable<object[]> StreakBonus =>
                new List<object[]>
                {
                // 5. Bonus minimum : Win ×3 → 14 points (9 + 5)
                new object[] { new[] { W, W, W }, 14 },

                // 6. 4 victoires : Win ×4 → 17 points (12 + 5)
                new object[] { new[] { W, W, W, W }, 17 },

                // 7. Streak interrompu : Win, Win, Loss, Win → 6 points
                new object[] { new[] { W, W, L, W }, 9 },

                // 8. Deux séries : Win ×3, Loss, Win ×4 → 31 points (21 + 10)
                new object[] { new[] { W, W, W, L, W, W, W, W }, 31 },

                // 9. Bonus non accordé (moins de 3 victoires d’affilée)
                //    Win, Draw, Win, Win → 10 points
                new object[] { new[] { W, D, W, W }, 10 },
                };
            public static IEnumerable<object[]> Penalities =>
              new List<object[]>
              {
                        
                        new object[] { new[] { W, W, D, W }, 7 , 3 },
                        new object[] { new[] { W, D, D}, 0, 8 },
                        new object[] { new[] { W, W, D}, 0, 7 }
              };

            public static IEnumerable<object[]> Out =>
              new List<object[]>
              {
                                    // 5. Bonus minimum : Win ×3 → 14 points (9 + 5) MAIS isDisqualified = true
                                    new object[] { new[] { W, W, W }, 0, true},
                                     // pas de combat MAIS isDisqualified = true
                                    new object[] { new[] { new MatchResult[] { }, }, 0, true},
              };
        
        }
        #endregion


        // ---------- Tests de base -------------------------------------------------
        #region Testsdebase
        [Theory]
        [MemberData(nameof(ScoreTestData.Basic), MemberType = typeof(ScoreTestData))]
        public void Should_Calculate_Basic_Scenarios(MatchResult[] matches, int expected)
        {
            // Act
            var score = _calculator.CalculateScore(matches.ToList());

            // Assert
            score.Should().Be(expected);
        }
        #endregion

        // ---------- Tests du bonus de série --------------------------------------
        #region Testsdubonusdesérie
        [Theory]
        [MemberData(nameof(ScoreTestData.StreakBonus), MemberType = typeof(ScoreTestData))]
        public void Should_Calculate_Streak_Bonus_Scenarios(MatchResult[] matches, int expected)
        {
            // Act
            var score = _calculator.CalculateScore(matches.ToList());

            // Assert
            score.Should().Be(expected);
        }
        #endregion

        // ---------- Cas limite : score jamais négatif ----------------------------
        #region Caslimite
        [Fact]
        public void Should_Not_Allow_Negative_Final_Score()
        {
            // Arrange : 3 + 1 = 4 points, mais 10 points de pénalité
            var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw)
        };

            // Act
            var score = _calculator.CalculateScore(matches, penaltyPoints: 10);

            // Assert
            score.Should().Be(0, "le score final ne peut jamais être négatif");
        }

        // Tests des cas limites
        [Fact]
        public void Should_Return_Zero_When_Disqualified() { }
    

        [Fact]
        public void Should_Calculate_Basic_Score_Without_Bonus()
        {
            // Arrange
            var matches = new List<MatchResult>
            {
            new(MatchResult.Result.Win), // 3 points
            new(MatchResult.Result.Draw), // 1 point
            new(MatchResult.Result.Loss) // 0 point
            };
            // Act
            var score = _calculator.CalculateScore(matches);
            // Assert
            score.Should().Be(4, "because 3+1+0 = 4 points without bonus");
        }

        [Fact]
        public void Should_Add_Bonus_For_Three_Consecutive_Wins()
        {
            // Arrange
            var matches = new List<MatchResult>
            {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw)
            };
            // Act
            var score = _calculator.CalculateScore(matches);
            // Assert
            score.Should().Be(15, "because 3*3 + 1 + 5 bonus = 15 points");
        }

        [Theory]
        [InlineData(3, 0, 0, 14)] // 3 wins → 9 + 5 bonus
        [InlineData(2, 1, 0, 7)] // 2 wins, 1 draw → 7, no bonus
        [InlineData(0, 0, 3, 0)] // 3 losses → 0 points
        public void Should_Calculate_Score_With_Different_Results(int wins, int draws, int losses, int expected)
        {
            // Arrange
            var matches = new List<MatchResult>();
            for (int i = 0; i < wins; i++)
                matches.Add(new(MatchResult.Result.Win));
            for (int i = 0; i < draws; i++)
                matches.Add(new(MatchResult.Result.Draw));
            for (int i = 0; i < losses; i++)
                matches.Add(new(MatchResult.Result.Loss));
            // Act
            var score = _calculator.CalculateScore(matches);
            // Assert
            score.Should().Be(expected);
        }

        [Fact]
        public void Should_Throw_Exception_When_Matches_Is_Null()
        {
            // Act & Assert
            Action act = () => _calculator.CalculateScore(null);
            act.Should().Throw<ArgumentNullException>()
            .WithParameterName("matches")
            .WithMessage("*cannot be null*");
        }
        #endregion

        // ---------- Tests de disqualification -------------------------------------------------
        #region Tests de disqualification
        [Theory]
        [MemberData(nameof(ScoreTestData.Out), MemberType = typeof(ScoreTestData))]
        public void Should_Calculate_PlayerOut(MatchResult[] matches, int expected, bool isDisqualified)
        {
            // Act
            var score = _calculator.CalculateScore(matches.ToList(), isDisqualified);

            // Assert
            score.Should().Be(expected);
        }
        #endregion

        // ---------- Tests des pénalités -------------------------------------------------
        #region Tests des pénalités
        [Theory]
        [MemberData(nameof(ScoreTestData.Penalities), MemberType = typeof(ScoreTestData))]
        public void Should_Calculate_Basic_Scenarios_Penalities(MatchResult[] matches, int expected,int penaltyPoints)
        {
            // Act
            var score = _calculator.CalculateScore(matches.ToList(), false, penaltyPoints);

            // Assert
            score.Should().Be(expected);
        }
        #endregion
    }
}