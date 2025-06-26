using Xunit;
using FluentAssertions;
using fiora;
using System.Collections.Generic;
using System.Linq;

namespace FioraUnitTest
{
    public class ScoreCalculatorTests
    {
        private readonly ScoreCalculator _calculator;

        public ScoreCalculatorTests()
        {
            _calculator = new ScoreCalculator();
        }

        #region Player-based Test Data
        public static class PlayerTestData
        {
            private static MatchResult W => new(MatchResult.Result.Win);
            private static MatchResult D => new(MatchResult.Result.Draw);
            private static MatchResult L => new(MatchResult.Result.Loss);

            public static IEnumerable<object[]> Basic => new List<object[]>
            {
                new object[] { new Player { Matches = new List<MatchResult> { W, D, L } }, 4 },
                new object[] { new Player { Matches = new List<MatchResult> { W, W } }, 6 },
                new object[] { new Player { Matches = new List<MatchResult> { D, D, D } }, 3 },
                new object[] { new Player { Matches = new List<MatchResult> { L, L } }, 0 }
            };

            public static IEnumerable<object[]> StreakBonus => new List<object[]>
            {
                new object[] { new Player { Matches = new List<MatchResult> { W, W, W } }, 14 },
                new object[] { new Player { Matches = new List<MatchResult> { W, W, W, W } }, 17 },
                new object[] { new Player { Matches = new List<MatchResult> { W, W, L, W } }, 9 },
                new object[] { new Player { Matches = new List<MatchResult> { W, W, W, L, W, W, W, W } }, 31 },
                new object[] { new Player { Matches = new List<MatchResult> { W, D, W, W } }, 10 }
            };

            public static IEnumerable<object[]> Penalities => new List<object[]>
            {
                new object[] { new Player { Matches = new List<MatchResult> { W, W, D, W }, PenaltyPoints = 3 }, 7 },
                new object[] { new Player { Matches = new List<MatchResult> { W, D, D }, PenaltyPoints = 8 }, 0 },
                new object[] { new Player { Matches = new List<MatchResult> { W, W, D }, PenaltyPoints = 7 }, 0 }
            };

            public static IEnumerable<object[]> Disqualified => new List<object[]>
            {
                new object[] { new Player { Matches = new List<MatchResult> { W, W, W }, IsDisqualified = true }, 0 },
                new object[] { new Player { Matches = new List<MatchResult>(), IsDisqualified = true }, 0 }
            }; 
        }
        #endregion

        [Theory]
        [MemberData(nameof(PlayerTestData.Basic), MemberType = typeof(PlayerTestData))]
        public void Should_Calculate_Basic_Score(Player player, int expected)
        {
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);
            score.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(PlayerTestData.StreakBonus), MemberType = typeof(PlayerTestData))]
        public void Should_Calculate_Streak_Bonus(Player player, int expected)
        {
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);
            score.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(PlayerTestData.Penalities), MemberType = typeof(PlayerTestData))]
        public void Should_Calculate_Score_With_Penalities(Player player, int expected)
        {
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);
            score.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(PlayerTestData.Disqualified), MemberType = typeof(PlayerTestData))]
        public void Should_Return_Zero_When_Disqualified(Player player, int expected)
        {
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);
            score.Should().Be(expected);
        }


        [Fact]

        public void Should_Limited_Case()
        {
            var player = new Player { Matches = new List<MatchResult>(), PenaltyPoints = 0 };
            var score = _calculator.CalculateScore(player.Matches, false, player.PenaltyPoints);
            score.Should().Be(0);
        }

        [Fact]
        public void Should_Limited_Case_Penalty_Negatif()
        {
            var matches1 = new MatchResult(MatchResult.Result.Win);
            var matches2 = new MatchResult(MatchResult.Result.Win);

            var player = new Player { Matches = new List<MatchResult> { matches1 , matches2 }, PenaltyPoints = -1 };
            Action act = () => _calculator.CalculateScore(player.Matches, false, player.PenaltyPoints);

            act.Should()
               .Throw<ArgumentException>()
               .WithParameterName("penaltyPoints")
               .WithMessage("*Penalty points cannot be negative*");
        }

        [Fact]
        public void Should_Limited_Case_Match_Null()
        {
            var player = new Player { Matches = null, IsDisqualified = false, PenaltyPoints = 0 };

            Action act = () => _calculator.CalculateScore(player.Matches, false, player.PenaltyPoints);

            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("matches")
               .WithMessage("*cannot be null*");
        }


        [Fact]
        public void Should_Limited_Case_100Match()
        {
            // Arrange : pattern de 100 matchs : Win, Draw, Loss, Win, Draw, Loss, ...
            var matches = new List<MatchResult>();
            int expectedScore = 0;
            int consecutiveWins = 0;
            int bonusCount = 0;

            for (int i = 0; i < 100; i++)
            {
                MatchResult match;
                switch (i % 3)
                {
                    case 0:
                        match = new MatchResult(MatchResult.Result.Win);
                        matches.Add(match);
                        expectedScore += 3;
                        consecutiveWins++;

                        // gestion bonus
                        if (consecutiveWins == 3)
                        {
                            expectedScore += 5;
                            bonusCount++;
                        }
                        else if (consecutiveWins > 3 && bonusCount < 1)
                        {
                            // éviter bonus multiples pour une même série >3
                        }
                        break;

                    case 1:
                        match = new MatchResult(MatchResult.Result.Draw);
                        matches.Add(match);
                        expectedScore += 1;
                        consecutiveWins = 0;
                        bonusCount = 0;
                        break;

                    case 2:
                        match = new MatchResult(MatchResult.Result.Loss);
                        matches.Add(match);
                        consecutiveWins = 0;
                        bonusCount = 0;
                        break;
                }
            }

            var player = new Player
            {
                Matches = matches
            };

            // Act
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);

            // Assert
            score.Should().Be(expectedScore, "le score doit correspondre à la simulation de 100 matchs avec bonus");
        }


        [Fact]
        public void Should_Throw_Exception_When_Matches_Is_Null()
        {
            var player = new Player { Matches = null };
            Action act = () => _calculator.CalculateScore(player.Matches);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("matches")
                .WithMessage("*cannot be null*");
        }

        [Fact]
        public void Should_Not_Allow_Negative_Final_Score()
        {
            var player = new Player
            {
                Matches = new List<MatchResult> { new(MatchResult.Result.Win), new(MatchResult.Result.Draw) },
                PenaltyPoints = 10
            };
            var score = _calculator.CalculateScore(player.Matches, player.IsDisqualified, player.PenaltyPoints);
            score.Should().Be(0);
        }
    }
}
