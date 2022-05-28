using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ShameBoy
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        // TODO: AF, BC, DE, HL can also be accessed as a 16 bit register (combined)

        /// <summary>
        /// Register A
        /// </summary>
        [FieldOffset(0)]
        public byte A;

        /// <summary>
        /// Register F
        /// </summary>
        [FieldOffset(1)]
        public FlagRegister F;

        /// <summary>
        /// Combined register A/F
        /// </summary>
        [FieldOffset(0)]
        public ushort AF;

        /// <summary>
        /// Register B
        /// </summary>
        [FieldOffset(2)]
        public byte B;

        /// <summary>
        /// Register C
        /// </summary>
        [FieldOffset(3)]
        public byte C;

        /// <summary>
        /// Combined register B/C
        /// </summary>
        [FieldOffset(2)]
        public ushort BC;

        /// <summary>
        /// Register D
        /// </summary>
        [FieldOffset(4)]
        public byte D;

        /// <summary>
        /// Register E
        /// </summary>
        [FieldOffset(5)]
        public byte E;

        /// <summary>
        /// Combined register D/E
        /// </summary>
        [FieldOffset(4)]
        public ushort DE;

        /// <summary>
        /// Register H
        /// </summary>
        [FieldOffset(6)]
        public byte H;

        /// <summary>
        /// Register L
        /// </summary>
        [FieldOffset(7)]
        public byte L;

        /// <summary>
        /// Combined register H/L
        /// </summary>
        [FieldOffset(6)]
        public ushort HL;

        /// <summary>
        /// Stack pointer, points to a position on the stack
        /// </summary>
        [FieldOffset(8)]
        public ushort StackPointer;

        /// <summary>
        /// Program counter, points to a position in the program.
        /// </summary>
        [FieldOffset(10)]
        public ushort ProgramCounter;

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
