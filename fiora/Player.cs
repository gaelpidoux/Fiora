using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiora
{
    public class Player
    {
        public string Name { get; set; }

        public List<MatchResult> Matches { get; set; } = new();

        public bool IsDisqualified { get; set; }

        public int PenaltyPoints { get; set; }

        public int FinalScore { get; set; }
    }

    public class TournamentRanking
    {
        private readonly ScoreCalculator _scoreCalculator;

        public TournamentRanking(ScoreCalculator scoreCalculator)
        {
            _scoreCalculator = scoreCalculator;
        }

        /// <summary>
        /// Classe les joueurs par score décroissant.
        /// </summary>
        public List<Player> GetRanking(List<Player> players)
        {
            foreach (var player in players)
            {
                player.FinalScore = _scoreCalculator.CalculateScore(
                    player.Matches,
                    player.IsDisqualified,
                    player.PenaltyPoints
                );
            }

            return players
                .OrderByDescending(p => p.FinalScore)
                .ThenBy(p => p.Name) // En cas d'égalité : ordre alphabétique
                .ToList();
        }

        /// <summary>
        /// Trouve le champion avec le meilleur score.
        /// </summary>
        public Player GetChampion(List<Player> players)
        {
            return GetRanking(players).FirstOrDefault();
        }
    }

}
