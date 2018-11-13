﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class SquareStatus
    {

        public SquareStatus()
        {
            LockingTime = LockingTime.Add(TimeSpan.FromMilliseconds(GenerateNextDelay()));
        }

        private static Random DelayGenerator = new Random();

        public bool IsClear { get; private set; } = true;

        public Nullable<Guid> TeamID { get; private set; } = null;

        public bool IsLocked = false;

        public DateTime LockingTime = DateTime.Now;

        public static int GenerateNextDelay()
        {
            // The delay can be between 250ms up to 1000 ms
            return SquareStatus.DelayGenerator.Next(250, 1001);
        }
    }
}
