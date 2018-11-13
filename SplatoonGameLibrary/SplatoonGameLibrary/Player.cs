using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class Player
    {
        // My initial location all players will start at 0,0 for now
        // In the future all players of the same team will spawn near each other and opposite another player
        public int X = 0, Y = 0;
        public readonly Team PlayerTeam;

        public Player(Team myTeam)
        {
            PlayerTeam = myTeam;
        }

    }
}
