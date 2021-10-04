using System;
using System.Collections.Generic;
using System.Text;

namespace ShameBoy.Exceptions
{
    public class UnknownInstructionException : Exception
    {
        public UnknownInstructionException(byte instruction) 
            : base($"Attempted to call non-existent instruction! 0x{instruction.ToString("X2")}")
        {

        }
    }
}
