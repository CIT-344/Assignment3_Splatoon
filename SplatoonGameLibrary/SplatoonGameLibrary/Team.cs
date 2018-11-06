using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class Team
    {
        public readonly Guid TeamID;

        private List<Player> Players;

        private readonly Color TeamColor;
    }
}
