using System;
using System.Collections.Generic;
using System.Text;

namespace ShameBoy
{
    public struct Registers
    {
        // TODO: AF, BC, DE, HL can also be accessed as a 16 bit register (combined)

        /// <summary>
        /// Register A
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// Register F
        /// </summary>
        public FlagRegister F { get; set; }

        /// <summary>
        /// Combined register A/F
        /// </summary>
        public ushort AF
        {
            get
            {
                return BitConverter.ToUInt16(new[] { A, (byte)F }, 0);
            }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                A = bytes[0];
                F = (FlagRegister)bytes[1];
            }
        }

        /// <summary>
        /// Register B
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// Register C
        /// </summary>
        public byte C { get; set; }

        /// <summary>
        /// Combined register B/C
        /// </summary>
        public ushort BC
        {
            get
            {
                return BitConverter.ToUInt16(new[] { B, C }, 0);
            }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                B = bytes[0];
                C = bytes[1];
            }
        }

        /// <summary>
        /// Register D
        /// </summary>
        public byte D { get; set; }

        /// <summary>
        /// Register E
        /// </summary>
        public byte E { get; set; }

        /// <summary>
        /// Combined register D/E
        /// </summary>
        public ushort DE
        {
            get
            {
                return BitConverter.ToUInt16(new[] { D, E }, 0);
            }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                D = bytes[0];
                E = bytes[1];
            }
        }

        /// <summary>
        /// Register H
        /// </summary>
        public byte H { get; set; }

        /// <summary>
        /// Register L
        /// </summary>
        public byte L { get; set; }

        /// <summary>
        /// Combined register H/L
        /// </summary>
        public ushort HL
        {
            get
            {
                return BitConverter.ToUInt16(new[] { H, L }, 0);
            }
            set
            {
                var bytes = BitConverter.GetBytes(value);
                H = bytes[0];
                L = bytes[1];
            }
        }

        /// <summary>
        /// Stack pointer, points to a position on the stack
        /// </summary>
        public ushort StackPointer { get; set; }

        /// <summary>
        /// Program counter, points to a position in the program.
        /// </summary>
        public ushort ProgramCounter { get; set; }

        public void SetFlagConditional(FlagRegister flag, bool condition)
        {
            if (condition)
                this.F |= flag;
            else
                this.F &= ~flag;
        }

        public void EnableFlag(FlagRegister flag)
        {
            this.F |= flag;
        }

        public void DisableFlag(FlagRegister flag)
        {
            this.F &= ~flag;
        }
    }
}
