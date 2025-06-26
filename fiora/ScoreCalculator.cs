using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fiora
{
    public class ScoreCalculator
    {
        /// <summary>
        /// Calcule le score final d'un joueur selon les règles du tournoi
        /// </summary>
        /// <param name="matches">Liste des résultats de combat dans l'ordre  chronologique</param>
        /// <param name="isDisqualified">True si le joueur est disqualifié</param>
        /// <param name="penaltyPoints">Points de pénalité (nombre
        /// positif)</param>
        /// <returns>Score final (jamais négatif)</returns>
        public int CalculateScore(List<MatchResult> matches, bool
        isDisqualified = false, int penaltyPoints = 0)
                {
                    // TODO: À implémenter selon les règles
                    throw new NotImplementedException();
                }
            }

}
