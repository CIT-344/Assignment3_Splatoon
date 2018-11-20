using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class Team
    {

        public Team(int PlayerCount, Color TeamColor, GameBoard board)
        {
            PopulatePlayers(PlayerCount);
            this.TeamColor = TeamColor;
            this.Board = board;
            TeamID = Guid.NewGuid();
        }

        // A reference to the gameBoard that created this team
        // Used to allow players to interact with the squares on the board
        public readonly GameBoard Board;

        public readonly Guid TeamID;

        public List<Player> Players { get; private set; }
        
        public Color TeamColor { get; private set; }

        private void PopulatePlayers(int n)
        {
            Players = new List<Player>(n);

            for (int i = 0; i < n; i++)
            {
                // Do player creation
                Players.Add(new Player(this));
            }
        }
    }
}
