using System;
using System.Collections.Generic;
using System.Text;

namespace ShameBoy.Exceptions
{
    public class InvalidBiosException : Exception
    {
        public InvalidBiosException() : base("Invalid BIOS supplied!")
        {

        }
    }
}
