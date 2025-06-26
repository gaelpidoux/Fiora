using System;
using System.Collections.Generic;

namespace fiora
{
    public class ScoreCalculator
    {
        public int CalculateScore(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
        {
            if (matches == null)
                throw new ArgumentNullException(nameof(matches), "Match list cannot be null.");

            if (penaltyPoints < 0)
                throw new ArgumentException("Penalty points cannot be negative.", nameof(penaltyPoints));

            if (isDisqualified)
                return 0;

            int score = 0;
            int consecutiveWins = 0;
            bool bonusAlreadyApplied = false;

            foreach (var match in matches)
            {
                switch (match.Outcome)
                {
                    case MatchResult.Result.Win:
                        score += 3;
                        consecutiveWins++;
                        break;
                    case MatchResult.Result.Draw:
                        score += 1;
                        consecutiveWins = 0;
                        break;
                    case MatchResult.Result.Loss:
                        consecutiveWins = 0;
                        break;
                }

                if (consecutiveWins == 3)
                {
                    score += 5;
                    bonusAlreadyApplied = true;
                }
                else if (consecutiveWins > 3 && !bonusAlreadyApplied)
                {
                    score += 0;
                }
                else if (consecutiveWins < 3)
                {
                    bonusAlreadyApplied = false;
                }
            }

            score -= penaltyPoints;

            return score < 0 ? 0 : score;
        }
    }
}
