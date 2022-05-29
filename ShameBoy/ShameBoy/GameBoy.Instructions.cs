using System;
using System.Runtime.InteropServices;

namespace ShameBoy
{
    public partial class GameBoy
    {
        /*
         Useful reference for implementing Opcodes:
         https://gbdev.io/gb-opcodes/optables/
         http://imrannazar.com/Gameboy-Z80-Opcode-Map
        */


        // args: opcode, length in bytes, description
        [Instruction(0x00, 1, "NOP - No instruction")]
        private byte NOP(Span<byte> args) => 4;

        [Instruction(0x01, 3, "LD BC - Load 16-bit into BC")]
        private byte LD_BC(Span<byte> args)
            => LD_16bit(ref registers.BC, MemoryMarshal.Read<ushort>(args));

        [Instruction(0x02, 1, "LD (BC), A - Load 8-bit value at A into address at pointer (BC)")]
        private byte LD_BC_A(Span<byte> args)
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
            => LD_8bit(ref registers.B, args[0]);

        [Instruction(0x07, 1, "RLC A - Rotate Left with Carry on A")]
        private byte RLC_A(Span<byte> args)
            => RLC_8bit(ref registers.A);

        [Instruction(0x08, 3, "LD nn SP - Save SP (stack pointer) to address nn")]
        private byte LD_nn_SP(Span<byte> args)
        {
            var mem = memory.FetchMemory(MemoryMarshal.Read<ushort>(args), 2);
            MemoryMarshal.Write(mem, ref registers.StackPointer);
            return 20;
        }

        [Instruction(0x09, 1, "ADD HL, BC - Add HL to BC")]
        private byte ADD_HL_BC(Span<byte> args)
            => ADD_16bit(ref registers.HL, registers.BC);

        [Instruction(0x0A, 1, "LD A, (BC) - Load A from address stored in BC")]
        private byte LD_A_BC(Span<byte> args)
            => LD_8bit_pointer(ref registers.A, registers.BC);

        [Instruction(0x0B, 1, "DEC BC - Decrement register BC")]
        private byte DEC_BC(Span<byte> args)
            => DEC_16bit(ref registers.BC);

        [Instruction(0x0C, 1, "INC C - Increases the value in register C")]
        private byte INC_C(Span<byte> args)
            => INC_8bit(ref registers.C);

        [Instruction(0x0D, 1, "DEC C - Decreases the value in register C")]
        private byte DEC_C(Span<byte> args)
            => DEC_8bit(ref registers.C);

        [Instruction(0x0E, 2, "LD C - Loads 8 bit into C")]
        private byte LD_C(Span<byte> args)
            => LD_8bit(ref registers.C, args[0]);

        [Instruction(0x0F, 1, "RRC A - Rotate Right with Carry on A")]
        private byte RRC_A(Span<byte> args)
            => RRC_8bit(ref registers.A);

        [Instruction(0x10, 2, "STOP - Stops the processor.")]
        private byte STOP(Span<byte> args)
        {
            return 0; // None of the instructions return 0 t-states, so this return value would inform the CPU it should die.
        }

        #region Reusable
        /// <summary>
        /// Load 16 bit value into a register
        /// </summary>
        /// <param name="register">Register to load value into</param>
        /// <param name="args">Arguments given</param>
        /// <returns>T states</returns>
        private byte LD_16bit(ref ushort register, ushort value)
        {
            register = value;
            return 12;
        }

        private byte LD_8bit(ref byte register, byte value)
        {
            register = value;
            return 8;
        }

        private byte LD_8bit_pointer(ref byte register, ushort pointer)
        {
            MemoryMarshal.Write(memory.FetchMemory(pointer, 1), ref register);
            return 8;
        }

        private byte INC_8bit(ref byte register)
        {
            registers.SetFlagConditional(FlagRegister.HalfCarry, (register & 0x0f) != 0x0f);

            register++;

            registers.SetFlagConditional(FlagRegister.Zero, register == 0);
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
            registers.SetFlagConditional(FlagRegister.HalfCarry, (register & 0x0f) == 0x0f);
            register--;

            registers.SetFlagConditional(FlagRegister.Zero, register == 0); // result 0?
            registers.EnableFlag(FlagRegister.Subtract); // subtraction so addsub = 1

            return 4;
        }

        private byte RLC_8bit(ref byte register)
        {
            registers.SetFlagConditional(FlagRegister.Carry, ((register & 0x80) >> 7) != 0);

            byte old = register;
            register = (byte)(old << (byte)1 | old >> (byte)7);

            registers.DisableFlag(FlagRegister.Zero);
            registers.DisableFlag(FlagRegister.Subtract);
            registers.DisableFlag(FlagRegister.HalfCarry);

            return 4;
        }

        private byte RRC_8bit(ref byte register)
        {
            registers.SetFlagConditional(FlagRegister.Carry, (register & 0x01) != 0);

            byte old = register;
            register = (byte)(old >> (byte)1 | old << (byte)7);

            registers.DisableFlag(FlagRegister.Zero);
            registers.DisableFlag(FlagRegister.Subtract);
            registers.DisableFlag(FlagRegister.HalfCarry);

            return 4;
        }

        private byte ADD_16bit(ref ushort destination, ushort value)
        {
            destination += value;

            registers.DisableFlag(FlagRegister.Subtract);
            registers.SetFlagConditional(FlagRegister.HalfCarry, (destination & 0xFFFF0000) != 0);
            registers.SetFlagConditional(FlagRegister.Carry, ((destination & 0x0f) + (value & 0x0f)) > 0x0f);

            return 8;
        }

        private byte DEC_16bit(ref ushort register)
        {
            registers.SetFlagConditional(FlagRegister.HalfCarry, (register & 0x0f) != 0);

            register--;

            registers.SetFlagConditional(FlagRegister.Zero, register == 0);
            registers.EnableFlag(FlagRegister.Subtract);

            return 8;
        }
        #endregion
    }
}
