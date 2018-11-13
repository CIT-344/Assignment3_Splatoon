using SplatoonGameLibrary;
using System;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Number of teams: ");
            int numTeams = 2;
            Console.WriteLine("Number of players per team");
            int numPlayersPerTeam = 2;
            Console.WriteLine("Setting up game environment");

            GameBoard board = new GameBoard(5, numTeams, numPlayersPerTeam);



            Console.WriteLine("Game is ending");

        }
    }
}
