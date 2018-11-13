using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class GameSquare
    {
        public GameSquare(int x, int y, GameBoard board)
        {
            this.X = x;
            this.Y = y;
            this.Board = board;

            CurrentStatus = new SquareStatus();
        }

        public readonly int X, Y;

        private readonly GameBoard Board;

        public SquareStatus CurrentStatus { get; private set; }
    }
}
