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

        // Tests avec données paramétrées
        [Theory]
        [InlineData(new[] { "Win", "Win", "Win" }, 14)] // 9 points + 5 bonus
        [InlineData(new[] { "Win", "Draw", "Win" }, 7)] // 7 points, pas de bonus
        public void Should_Calculate_Score_Correctly(string[] results, int expectedScore)
        { }
        // Tests des cas limites
        [Fact]
        public void Should_Return_Zero_When_Disqualified() { }
        [Fact]
        public void Should_Not_Allow_Negative_Final_Score() { }
    

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

    }
}