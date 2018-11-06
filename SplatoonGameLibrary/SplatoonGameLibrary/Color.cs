using System;
using System.Collections.Generic;
using System.Text;

namespace SplatoonGameLibrary
{
    public class Color
    {
        public readonly byte A;
        public readonly byte B;
        public readonly byte R;
        public readonly byte G;

        public readonly char Identifier;

        public Color(byte Alpha, byte Blue, byte Red, byte Green, char Letter)
        {
            this.A = Alpha;
            this.B = Blue;
            this.R = Red;
            this.G = Green;
            this.Identifier = Letter;
        }
    }
}
