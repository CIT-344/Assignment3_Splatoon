using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SplatoonGameLibrary
{
    public class GameBoard
    {
        // Event System for the GUI project
        public delegate void ColorChanged(Color color, int X, int Y);
        public event ColorChanged OnColorChanged;
        public delegate void GameEnded(GameBoard sender, IEnumerable<KeyValuePair<Team, int>> Results, Team Winner, int WinnerCount);
        public event GameEnded OnGameEnd;


        private static Random MovementRandomGenHelper = new Random();
        private readonly List<GameSquare> Squares;
        public bool GameIsRunning;
        private Task SimulatedTimerThread;
        public readonly CancellationTokenSource StopTimerRequest = new CancellationTokenSource();

        private Timer SimpleGameLengthTimer;
        private List<Team> Teams;

        private TimeSpan GameLength;
        
        public IEnumerable<IGrouping<Team, GameSquare>> GetFinalBoard()
        {
            var group = Squares
                        .GroupBy(x => x.CurrentStatus.Team);
            return group;
        }


        public GameBoard(int sizeSquare, int numTeams, int playersPerTeam, TimeSpan GameLength)
        {
            this.GameLength = GameLength;
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
            
        }


        private void NotifyGameEnded()
        {
            var simpleGrouping = GetFinalBoard()
                .Where(x=>x.Key != null)
                .Select(x=> new KeyValuePair<Team, int>(x.Key, x.Count()))
                .OrderByDescending(x=>x.Value);


            OnGameEnd?.Invoke(this, simpleGrouping, simpleGrouping.First().Key,simpleGrouping.First().Value);
        }

        private void TimerCallback(object state)
        {
            // Once the game is setup and running stop it after the GameLength
            StopTimerRequest.Cancel();
            SimpleGameLengthTimer.Dispose();
        }

        public void StartGame()
        {
            GameIsRunning = true;

            // Spin up a thread for the simulated locking timer
            SimulatedTimerThread = Task.Run(() =>
            {
                BeginSimulatedTimer(TimeSpan.FromMilliseconds(100));
            }, StopTimerRequest.Token);


            SimpleGameLengthTimer = new Timer(TimerCallback, null, (int)GameLength.TotalMilliseconds, 0);

            foreach (var player in Teams.SelectMany(x=>x.Players))
            {
                Debug.WriteLine($"Player on Team {player.PlayerTeam.TeamColor.Identifier} has started moving");
                player.StartMoving();
            }
        }
        

        public void NotifyColorChanged(Color c, int x, int y)
        {
            OnColorChanged?.Invoke(c,x, y);
        }

        public GameSquare FindNextAvailableSquare(Player p)
        {
            var DistanceQuery = Squares.Select(sqr => new { sqr.X, sqr.Y, sqr }).Select(d => new
            {
                distance = Math.Sqrt(Math.Pow(d.X - p.X, 2) + Math.Pow(d.Y - p.Y, 2)),
                Square = d.sqr
            });

            // Given the current player
            // Find the first Square that doesn't already have a player waiting
            // Order the squares to find all the clear ones first
            var OrderedSquares = DistanceQuery
                .Where(x=>x.distance >= Math.Sqrt(Squares.LongCount())/(MovementRandomGenHelper.Next(2, 4)))
                .Where(x => x.Square.PlayerWaiting == false)
                .OrderByDescending(x => x.Square.CurrentStatus.IsClear)
                .ThenBy(x=>x.distance);

            

            foreach (var sqr in OrderedSquares.Select(x=>x.Square))
            {
                // Find a square that isn't already owned by this player's team
                if (sqr.CurrentStatus.Team != p.PlayerTeam)
                {
                    return sqr;
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
                                //Debug.WriteLine($"Timer thread released lock on ({sqr.X}, {sqr.Y})");
                            }
                        }
                        else
                        {
                            // Don't sit here and wait for a square if it's locked by a player
                            // Just skip it
                            if (Monitor.TryEnter(sqr.CurrentStatus))
                            {
                                sqr.CurrentStatus.IsLocked = true;
                                //Debug.WriteLine($"Timer thread acquired lock on ({sqr.X}, {sqr.Y})");
                            }
                            else
                            {
                                //Debug.WriteLine($"Timer thread couldn't acquire lock on ({sqr.X}, {sqr.Y})");
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
                if (Monitor.IsEntered(sqr.CurrentStatus))
                {
                    //Debug.WriteLine($"Timer thread released lock on ({sqr.X}, {sqr.Y})");
                    Monitor.Exit(sqr.CurrentStatus);
                }
            }


            // End the Game State
            GameIsRunning = false;

            // Compile list of player threads and wait for them to finish
            var playerThreads = Teams.SelectMany(x => x.Players).Select(x=>x.MovementThread).ToArray();
            Task.WaitAll(playerThreads);

            // Trigger score tally and fire event to GUI
            NotifyGameEnded();
            
        }

        public readonly Color Yellow = new Color(0, 255, 255, 0, 'Y');
        public readonly Color Pink = new Color(0, 255, 192, 203, 'P');
    }
}
