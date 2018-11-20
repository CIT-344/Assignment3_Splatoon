using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SplatoonGameLibrary
{
    public class Player
    {
        public readonly Guid PlayerID;
        public int X = 0, Y = 0;
        public readonly Team PlayerTeam;

        public Task MovementThread { get; private set; }

        public static readonly Random PlayerSleepGenerator = new Random();

        public Player(Team myTeam)
        {
            PlayerTeam = myTeam;
            PlayerID = Guid.NewGuid();
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
                    
                    Thread.Sleep(PlayerSleepGenerator.Next(100, 501));
                }
            }, PlayerTeam.Board.StopTimerRequest.Token);
        }

    }
}
