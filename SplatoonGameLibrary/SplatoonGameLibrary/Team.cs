using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class Team
    {

        public Team(int PlayerCount, Color TeamColor)
        {
            PopulatePlayers(PlayerCount);
            this.TeamColor = TeamColor;
        }

        public readonly Guid TeamID;

        public List<Player> Players { get; private set; }
        
        private readonly Color TeamColor;

        private void PopulatePlayers(int n)
        {
            Players = new List<Player>(n);

            for (int i = 0; i < n; i++)
            {
                // Do player creation
                Players.Add(new Player(this));
            }
        }

        public Color GetTeamColor()
        {
            return TeamColor;
        }
    }
}
