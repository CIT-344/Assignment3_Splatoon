using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        private readonly List<GameSquare> Squares;

        private List<Team> Teams;
        
        private readonly System.Timers.Timer LockingTimer;

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

        public IGrouping<Team, GameSquare> GetFinalBoard()
        {
            var group = Squares
                        .GroupBy(x => x.CurrentStatus.Team)
                        .Single();
            return group;
        }

        public GameBoard(int sizeSquare, int numTeams, int playersPerTeam)
        {
            Squares = new List<GameSquare>((int)Math.Pow(sizeSquare, 2)); // Just a hint to the list of the total size
            // Something like 5 means 5^2 = 25 squares total
            for (int x = 0; x < sizeSquare; x++)
            {
                for (int y = 0; y < sizeSquare; y++)
                {
                    Squares.Add(new GameSquare(x, y, this));
                }
            }
            // Number of teams to create
            Teams = new List<Team>(numTeams);
            for (int i = 0; i < numTeams; i++)
            {
                // Create all the teams
                // For simple testing reasons if i is 0 their color is Yellow
                Teams.Add(new Team(playersPerTeam, (i == 0 ? Yellow: Pink), this));
            }
            
            // Setup the universal locking/unlocking timer
            LockingTimer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = 150, // Something extremely low, this timer needs to run fast enough to simulate all the blocks having timers
                Enabled = true
            };
            LockingTimer.Elapsed += LockingTimerHasTicked;
        }
        
        public GameSquare FindNextAvailableSquare(Player p)
        {
            
            // Given the current player
            // Find the next available square to travel to
            // Will use the current players pos to attempt to find squares that are close
            // This doesn't check for locked vs. unlocked
            // rather will find a square that isn't already being waited on by another player
            return null;
        }

        private void LockingTimerHasTicked(object sender, ElapsedEventArgs e)
        {
            // ForEach square in Squares
            foreach (var sqr in Squares)
            {
                // Wrap this in a lock because by doing this I can stop players from reading it while it's being updated
                lock (sqr.CurrentStatus)
                {
                    if (sqr.CurrentStatus.LockingTime < DateTime.Now)
                    {
                        // Their time has expired and needs changed
                        sqr.CurrentStatus.IsLocked = !sqr.CurrentStatus.IsLocked;

                        // Generate a new period of wait before the locking changes
                        sqr.CurrentStatus.LockingTime = sqr.CurrentStatus.LockingTime.Add(TimeSpan.FromMilliseconds(SquareStatus.GenerateNextDelay()));
                    }
                }

                // Send out a notify event stating that something has changed
                Monitor.PulseAll(sqr.CurrentStatus);
            }
        }

        public readonly Color Yellow = new Color(0, 255, 255, 0, 'Y');
        public readonly Color Pink = new Color(0, 255, 192, 203, 'P');
    }
}
