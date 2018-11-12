using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        private List<GameSquare> Squares;

        private List<Team> Teams;
        
        /// <summary>
        /// Returns a grouping of players per team
        /// </summary>
        /// <returns>A grouping of players per team</returns>
        public IGrouping<Team, Player> GetAllPlayers()
        {
            var group = Teams
                        .SelectMany(x => x.Players)
                        .GroupBy(x => x.PlayerTeam)
                        .Single();

            return group;
        }


    }
}
