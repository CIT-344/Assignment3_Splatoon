using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SplatoonGameLibrary
{
    public class Player
    {
        public int X = 0, Y = 0;
        public readonly Team PlayerTeam;

        private Task MovementThread;

        public static readonly Random PlayerSleepGenerator = new Random();

        public Player(Team myTeam)
        {
            PlayerTeam = myTeam;
        }
        
        public void StartMoving()
        {
            MovementThread = Task.Run(()=> 
            {
                while (PlayerTeam.Board.GameIsRunning)
                {
                    var nextAvailableSpot = PlayerTeam.Board.FindNextAvailableSquare(this);

                    if (nextAvailableSpot != null)
                    {
                        nextAvailableSpot.ApplyTeamColor(this);
                    }

                    // Last check before entering sleep
                    if (!PlayerTeam.Board.GameIsRunning)
                    {
                        break;
                    }

                    // Sleep this player for 500ms to 1sec to simulate a real player.
                    Thread.Sleep(PlayerSleepGenerator.Next(500, 10001));
                }
            }, PlayerTeam.Board.StopTimerRequest.Token);
        }

    }
}
