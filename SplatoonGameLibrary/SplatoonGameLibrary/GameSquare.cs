using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SplatoonGameLibrary
{
    public class GameSquare
    {
        public delegate void ColorChanged(int X, int Y, Color color);

        public event ColorChanged MyColorChanged;

        public bool PlayerWaiting = false;

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
            lock (this)
            {
                PlayerWaiting = true;
            }
            
            // Apply waiting logic here
            // Player thread will block here until the color my be changed
            // This CurrentStatus can be locked in the SimulatedTimerThread
            lock (CurrentStatus)
            {
                // Lock has been acquired I can do stuff now
                // Call a method to change the team holding this
                CurrentStatus.ChangeSquareOwnership(p.PlayerTeam);
                p.X = X;
                p.Y = Y;


                Board.NotifyColorChanged(CurrentStatus.Team.TeamColor, X, Y);

                PlayerWaiting = false;
            }

            

        }
    }
}
