using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        private readonly List<GameSquare> Squares;

        private Task SimulatedTimerThread;
        private CancellationTokenSource StopTimerRequest = new CancellationTokenSource();

        private List<Team> Teams;
        
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

            // Spin up a thread for the simulated locking timer
            SimulatedTimerThread = Task.Run(()=> 
            {
                BeginSimulatedTimer(TimeSpan.FromMilliseconds(150));
            }, StopTimerRequest.Token);
        }
        
        public GameSquare FindNextAvailableSquare(Player p)
        {
            // Given the current player
            foreach (var sqr in Squares)
            {
                if (Monitor.TryEnter(sqr.PlayerWaiting))
                {
                    // Find the first Square that doesn't already have a player waiting
                    // Within this condition I am locking
                    Monitor.Exit(sqr.PlayerWaiting);
                    return sqr;
                }
            }

            return null;
        }

        private void BeginSimulatedTimer(TimeSpan Interval)
        {
            while (true && !StopTimerRequest.Token.IsCancellationRequested)
            {
                // ForEach square in Squares
                foreach (var sqr in Squares)
                {
                    if (sqr.CurrentStatus.LockingTime < DateTime.Now)
                    {
                        if (sqr.CurrentStatus.IsLocked)
                        {
                            if (Monitor.IsEntered(sqr.CurrentStatus.IsLocked))
                            {
                                Monitor.Exit(sqr.CurrentStatus.IsLocked);

                                // Generate a new period of wait before the locking changes
                                sqr.CurrentStatus.LockingTime = sqr.CurrentStatus.LockingTime.Add(TimeSpan.FromMilliseconds(SquareStatus.GenerateNextDelay()));
                            }
                        }
                        else
                        {
                            Monitor.Enter(sqr.CurrentStatus.IsLocked);
                            sqr.CurrentStatus.IsLocked = !sqr.CurrentStatus.IsLocked;
                        }
                    }
                }

                Thread.Sleep(Interval.Milliseconds);
            }


            foreach (var sqr in Squares)
            {
                // Run through and if a lock is being held let it go
                if (Monitor.IsEntered(sqr.CurrentStatus.IsLocked))
                {
                    Monitor.Exit(sqr.CurrentStatus.IsLocked);
                }
            }
        }

        public readonly Color Yellow = new Color(0, 255, 255, 0, 'Y');
        public readonly Color Pink = new Color(0, 255, 192, 203, 'P');
    }
}
