using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        private readonly List<GameSquare> Squares;

        private List<Team> Teams;
        
        private readonly Timer LockingTimer;

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
                Teams.Add(new Team(playersPerTeam, (i == 0 ? Yellow: Pink) ));
            }


            // Setup the universal locking/unlocking timer
            LockingTimer = new Timer
            {
                AutoReset = true,
                Interval = 150, // Something extremely low, this timer needs to run fast enough to simulate all the blocks having timers
                Enabled = true
            };
            LockingTimer.Elapsed += LockingTimerHasTicked;
        }

        private void LockingTimerHasTicked(object sender, ElapsedEventArgs e)
        {
            // ForEach square in Squares
            foreach (var sqr in Squares)
            {
                // Wrap this in a lock because by doing this I can stop players from reading it while it's being updated
                lock (sqr)
                {
                    if (sqr.CurrentStatus.LockingTime < DateTime.Now)
                    {
                        // Their time has expired and needs changed
                        sqr.CurrentStatus.IsLocked = !sqr.CurrentStatus.IsLocked;

                        // Generate a new period of wait before the locking changes
                        sqr.CurrentStatus.LockingTime = sqr.CurrentStatus.LockingTime.Add(TimeSpan.FromMilliseconds(SquareStatus.GenerateNextDelay()));
                    }
                }
            }
        }

        public readonly Color Yellow = new Color(0, 255, 255, 0, 'Y');
        public readonly Color Pink = new Color(0, 255, 192, 203, 'P');
    }
}
