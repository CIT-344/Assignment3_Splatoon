using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class GameSquare
    {
        public readonly int X, Y;

        private readonly GameBoard Board;

        public readonly SquareStatus CurrentStatus;

        public bool IsLocked;
    }
}
