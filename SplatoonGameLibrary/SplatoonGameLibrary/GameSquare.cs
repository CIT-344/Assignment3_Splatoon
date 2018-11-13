﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SplatoonGameLibrary
{
    public class GameSquare
    {

        public object PlayerWaiting = new object();

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

        public void ApplyTeamColor(Player p)
        {
            // Add something about HasPlayerWaiting to prevent multiple players from sitting here
            lock (PlayerWaiting)
            {
                // Apply waiting logic here
                // Player thread will block here until the color my be changed
                Monitor.Enter(CurrentStatus.IsLocked);

                // Lock has been acquired I can do stuff now
                // Call a method to change the team holding this
                CurrentStatus.ChangeSquareOwnership(p.PlayerTeam);

                Monitor.Exit(CurrentStatus.IsLocked);
            }
        }
    }
}
