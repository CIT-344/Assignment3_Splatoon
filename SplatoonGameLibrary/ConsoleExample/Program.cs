using SplatoonGameLibrary;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Number of teams: ");
            int numTeams = 2;
            Console.WriteLine("Number of players per team");
            int numPlayersPerTeam = 1;
            Console.WriteLine("Setting up game environment");

            GameBoard board = new GameBoard(5, numTeams, numPlayersPerTeam);


            while (board.GameIsRunning)
            {
                var t1 = Task.Run(() =>
                {
                    var p1 = board.GetAllPlayers().First().First();

                    for (int i = 0; i < 10000; i++)
                    {
                        var avail = board.FindNextAvailableSquare(p1);

                        if (avail != null)
                        {
                            avail.ApplyTeamColor(p1);
                        }
                    }
                });

                var t2 = Task.Run(() =>
                {
                    var p1 = board.GetAllPlayers().Last().First();

                    for (int i = 0; i < 10000; i++)
                    {
                        var avail = board.FindNextAvailableSquare(p1);

                        if (avail != null)
                        {
                            avail.ApplyTeamColor(p1);
                        }
                    }
                });

                Task.WaitAll(t1, t2);

                var allSquares = board.GetFinalBoard();
            };
            Console.WriteLine("Game is ending");

        }
    }
}
