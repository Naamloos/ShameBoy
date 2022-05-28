using System;
using System.Runtime.InteropServices;

namespace ShameBoy
{
    public partial class GameBoy
    {
        /*
         Useful reference for implementing Opcodes:
         https://gbdev.io/gb-opcodes/optables/
        */


        // args: opcode, length in bytes, description
        [Instruction(0x00, 1, "NOP - No instruction")]
        private byte NOP(Span<byte> args) => 4;

        [Instruction(0x01, 3, "LD BC - Load 16-bit into BC")]
        private byte LD_BC(Span<byte> args)
            => LD_16bit(ref registers.BC, args);

        [Instruction(0x02, 1, "LD (BC), A - Load 8-bit value at A into address at pointer (BC)")]
        private byte LD_A_BC(Span<byte> args)
            => LD_8bit_pointer(ref registers.A, registers.BC);

        [Instruction(0x03, 1, "INC BC - Increase BC by 1")]
        private byte INC_BC(Span<byte> args)
            => INC_16bit(ref registers.BC);

        [Instruction(0x04, 1, "INC B - Increase B by 1")]
        private byte INC_B(Span<byte> args)
            => INC_8bit(ref registers.B);

        [Instruction(0x05, 1, "DEC B - Decrease B by 1")]
        private byte DEC_B(Span<byte> args)
            => DEC_8bit(ref registers.B);

        [Instruction(0x06, 2, "LD B - Load 8-bit into B")]
        private byte LD_B(Span<byte> args)
            => LD_8bit(ref registers.B, args);

        [Instruction(0x07, 1, "RLC A - Rotate Left Carry A")]
        private byte RLC_A(Span<byte> args)
            => RLC_8bit(ref registers.A);

        #region Reusable
        /// <summary>
        /// Load 16 bit value into a register
        /// </summary>
        /// <param name="register">Register to load value into</param>
        /// <param name="args">Arguments given</param>
        /// <returns>T states</returns>
        private byte LD_16bit(ref ushort register, Span<byte> args)
        {
            register = MemoryMarshal.Read<ushort>(args);
            return 12;
        }

        private byte LD_8bit(ref byte register, Span<byte> args)
        {
            register = args[0];
            return 8;
        }

        private byte LD_8bit_pointer(ref byte register, ushort pointer)
        {
            MemoryMarshal.Write(memory.FetchMemory(pointer, 1), ref register);
            return 8;
        }

        private byte INC_8bit(ref byte register)
        {
            register++;

            registers.SetFlagConditional(FlagRegister.Zero, register == 0);
            registers.SetFlagConditional(FlagRegister.HalfCarry, register == 0); // carry would only happen on 0
            registers.DisableFlag(FlagRegister.Subtract); // addition, set flag 0

            return 4;
        }

        private byte INC_16bit(ref ushort register)
        {
            register++;
            return 8;
        }

        private byte DEC_8bit(ref byte register)
        {
            register--;

            registers.SetFlagConditional(FlagRegister.Zero, register == 0); // result 0?
            registers.SetFlagConditional(FlagRegister.HalfCarry, register == 0); // carry would only happen on 0
            registers.EnableFlag(FlagRegister.Subtract); // subtraction so addsub = 1

            return 4;
        }

        private byte DEC_16bit(ref ushort register)
        {
            register--;
            return 8;
        }

        private byte RLC_8bit(ref byte register)
        {
            byte old = register;
            register = (byte)(old << (byte)1 | old >> (byte)7);

            registers.DisableFlag(FlagRegister.Zero);
            registers.DisableFlag(FlagRegister.Subtract);
            registers.DisableFlag(FlagRegister.HalfCarry);
            registers.SetFlagConditional(FlagRegister.Carry, (register & 0x80) >> 7 > 0);

            return 4;
        }
        #endregion
    }
}
