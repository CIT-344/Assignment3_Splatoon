using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        private readonly List<GameSquare> Squares;
        public bool GameIsRunning;
        private Task SimulatedTimerThread;
        public readonly CancellationTokenSource StopTimerRequest = new CancellationTokenSource();

        private List<Team> Teams;
        
        public IEnumerable<IGrouping<Team, GameSquare>> GetFinalBoard()
        {
            var group = Squares
                        .GroupBy(x => x.CurrentStatus.Team);
            return group;
        }

        public GameBoard(int sizeSquare, int numTeams, int playersPerTeam, TimeSpan GameLength)
        {
            Squares = new List<GameSquare>((int)Math.Pow(sizeSquare, 2)); // Just a hint to the list of the total size
            // Something like 5 means 5^2 = 25 squares total
            for (int x = 0; x < sizeSquare; x++)
            {
                for (int y = 0; y < sizeSquare; y++)
                {
                    var sqr = new GameSquare(x, y, this);
                    Squares.Add(sqr);
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
                BeginSimulatedTimer(TimeSpan.FromMilliseconds(100));

                // End the Game State
                GameIsRunning = false;

            }, StopTimerRequest.Token);
        
            // Once the game is setup and running stop it after the GameLength
            StopTimerRequest.CancelAfter(GameLength);
        }

        public void StartGame()
        {
            GameIsRunning = true;
            foreach (var player in Teams.SelectMany(x=>x.Players))
            {
                Debug.WriteLine($"Player on Team {player.PlayerTeam.TeamColor.Identifier} has started moving");
                player.StartMoving();
            }
        }
        
        public GameSquare FindNextAvailableSquare(Player p)
        {
            // Given the current player
            // Find the first Square that doesn't already have a player waiting
            // Order the squares to find all the clear ones first
            var OrderedSquares = Squares
                .Where(x => x.PlayerWaiting == false)
                .OrderByDescending(x=>x.CurrentStatus.IsClear);

            foreach (var sqr in OrderedSquares)
            {
                // Find a square that isn't already owned by this player's team
                if (sqr.CurrentStatus.Team != p.PlayerTeam)
                {
                    if (sqr.X != p.X && sqr.Y != p.Y)
                    {
                        return sqr;
                    }
                }
            }

            return null;
        }

        private void BeginSimulatedTimer(TimeSpan Interval)
        {
            while (!StopTimerRequest.Token.IsCancellationRequested)
            {
                // ForEach square in Squares
                foreach (var sqr in Squares)
                {
                    // Before doing anything just check if the game is ending and needs to break this loop
                    if (StopTimerRequest.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    var dtRef = DateTime.Now;
                    var lockRef = sqr.CurrentStatus.LockingTime;

                    if (lockRef.Ticks < dtRef.Ticks)
                    {
                        if (sqr.CurrentStatus.IsLocked)
                        {
                            if (Monitor.IsEntered(sqr.CurrentStatus))
                            {
                                sqr.CurrentStatus.IsLocked = false;
                                Monitor.Exit(sqr.CurrentStatus);
                                Debug.WriteLine($"Timer thread released lock on ({sqr.X}, {sqr.Y})");
                            }
                        }
                        else
                        {
                            // Don't sit here and wait for a square if it's locked by a player
                            // Just skip it
                            if (Monitor.TryEnter(sqr.CurrentStatus))
                            {
                                sqr.CurrentStatus.IsLocked = true;
                                Debug.WriteLine($"Timer thread acquired lock on ({sqr.X}, {sqr.Y})");
                            }
                            else
                            {
                                Debug.WriteLine($"Timer thread couldn't acquire lock on ({sqr.X}, {sqr.Y})");
                            }
                            
                        }

                        // Generate a new period of wait before the locking changes
                        sqr.CurrentStatus.LockingTime = dtRef.Add(TimeSpan.FromMilliseconds(SquareStatus.GenerateNextDelay()));
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
