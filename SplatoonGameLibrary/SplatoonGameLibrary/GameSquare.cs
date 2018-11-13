using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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

        public void ApplyTeamColor(Player p)
        {
            // Given a player

            Monitor.Wait(CurrentStatus);
            try
            {
                if (Monitor.IsEntered(CurrentStatus))
                {
                    // Lock has been acquired I can do stuff now
                    // Call a method to change the team holding this
                    CurrentStatus.ChangeSquareOwnership(p.PlayerTeam);
                }
            }
            catch (Exception e)
            { }
            finally
            {
                Monitor.Exit(CurrentStatus);
                Monitor.PulseAll(CurrentStatus);
            }
        }
    }
}
