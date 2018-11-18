using SplatoonGameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Number of teams (Defaults to 2): ");
            bool numTeams = int.TryParse(Console.ReadLine(), out int _numTeams);

            Console.WriteLine("Number of players per team (Defaults to 2): ");
            bool numPlayersPerTeam = int.TryParse(Console.ReadLine(), out int _numPlayersPerTeam);

            Console.WriteLine("Length of game in minutes (Defaults to 5): ");
            bool minutesOfPlayTime = int.TryParse(Console.ReadLine(), out int _minutesOfPlayTime);

            Console.WriteLine("Size of game board squared (Defaults to 5): ");
            bool gameSize = int.TryParse(Console.ReadLine(), out int _gameSize);

            Console.WriteLine("Setting up game environment");

            GameBoard board = new GameBoard(gameSize ? _gameSize : 5, numTeams ? _numTeams : 2, numPlayersPerTeam ? _numPlayersPerTeam : 2, TimeSpan.FromMinutes(minutesOfPlayTime ? _minutesOfPlayTime : 5));

            Console.Clear();
            Console.Title = "Game is running";

            board.StartGame();

            while (board.GameIsRunning)
            {
                // Cheap way of making the console chill out while the game is running
            }

            var gameResult = board.GetFinalBoard();
            KeyValuePair<char, int> Winner = new KeyValuePair<char, int>('C', -1);
            foreach (var team in gameResult)
            {
                if (team.Key != null)
                {
                    Console.WriteLine($"'{team.Key.TeamColor.Identifier}' Team has {team.Count()} squares!");
                    
                    if (Winner.Value < team.Count())
                    {
                        Winner = new KeyValuePair<char, int>(team.Key.TeamColor.Identifier, team.Count());
                    }

                }
            }

            Console.WriteLine($"'{Winner.Key}' has won the game!");

            for (int x = 0; x < (gameSize ? _gameSize : 5); x++)
            {
                for (int y = 0; y < (gameSize ? _gameSize : 5); y++)
                {
                    // Get Square at this location
                    var sqr = gameResult.SelectMany(i => i).Where(s => s.X == x && s.Y == y).Single();
                    if (sqr.CurrentStatus.Team != null)
                    {
                        Console.Write(String.Concat(" ", sqr.CurrentStatus.Team.TeamColor.Identifier, " "));
                    }
                    else
                    {
                        Console.Write('C');
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("Game is ending");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }
    }
}
