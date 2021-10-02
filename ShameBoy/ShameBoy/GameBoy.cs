using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

/*
 * This is the main emulator class for ShameBoy.
 * listed in this comment are a bunch of sources
 * I've used to make this a reality! :)
 * 
 * https://gbdev.io/gb-opcodes/optables/
 * http://imrannazar.com/Gameboy-Z80-Opcode-Map
 * https://gbdev.io/list.html#documentation
 * http://bgb.bircd.org/pandocs.htm
 * https://cturt.github.io/cinoop.html
 * http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-The-CPU
 * https://rgbds.gbdev.io/docs/v0.5.1/gbz80.7#INC_r8
 */

namespace ShameBoy
{
    public class GameBoy
    {
        private Registers registers;
        private byte machineClock = 0;
        private byte timerClock = 0;
        private MemoryBank memory;

        private Dictionary<byte, (InstructionAttribute, MethodInfo)> instructions;

        public GameBoy()
        {
            this.registers = new Registers();
            this.instructions = new Dictionary<byte, (InstructionAttribute, MethodInfo)>();

            // Load CPU instructions into the dictionary
            this.GetType().GetMethods()
                .Where(x => x.GetCustomAttribute<InstructionAttribute>() != null) // find methods that have the instruction attr
                .ToDictionary(x => 
                {
                    // map to dictionary on byte and (attr+method)
                    var attr = x.GetCustomAttribute<InstructionAttribute>();
                    return (attr.Instruction, (attr, x));
                });
        }

        // args: opcode, length in bytes, description
        [Instruction(0x00, 1, "NOP - No instruction")]
        private byte NOP()
        {
            // Return amount of T-states
            return 4;
        }

        [Instruction(0x01, 3, "LD BC - Load 16-bit into BC")]
        private byte LD_BC(params byte[] args)
        {
            this.registers.BC = memory[BitConverter.ToUInt16(args, 0)];
            return 12;
        }

        [Instruction(0x02, 1, "LD (BC), A - Load 8-bit value at A into address at pointer (BC)")]
        private byte LD_A_BC()
        {
            memory[registers.BC] = registers.A;
            return 8;
        }

        [Instruction(0x03, 1, "INC BC - Increase BC by 1")]
        private byte INC_BC()
        {
            // increase BC. no flags to be set.
            this.registers.BC++;
            return 8;
        }

        [Instruction(0x04, 1, "INC B - Increase B by 1")]
        private byte INC_B()
        {

            // increase B
            this.registers.B++;

            // result is zero? set zero flag.
            this.registers.SetFlagConditional(FlagRegister.Zero, this.registers.B == 0);
            this.registers.SetFlagConditional(FlagRegister.HalfCarry, this.registers.B == 0); // carry would only happen on 0
            this.registers.DisableFlag(FlagRegister.AddSub); // addition, set flag 0

            return 4;
        }

        [Instruction(0x05, 1, "DEC B - Decrease B by 1")]
        private byte DEC_B()
        {
            this.registers.B--;

            this.registers.SetFlagConditional(FlagRegister.Zero, this.registers.B == 0); // result 0?
            this.registers.SetFlagConditional(FlagRegister.HalfCarry, this.registers.B == 0); // carry would only happen on 0
            this.registers.EnableFlag(FlagRegister.AddSub); // subtraction so addsub = 1

            return 4;
        }

        [Instruction(0x06, 2, "LD B - Load 8-bit into B")]
        private byte LD_B(params byte[] args)
        {
            this.registers.B = memory[args[0]];
            return 8;
        }
    }

    /// <summary>
    /// Attribute defining a CPU instruction method.
    /// </summary>
    public class InstructionAttribute : Attribute
    {
        public byte Instruction;
        public byte Length;
        public string Description;

        public InstructionAttribute(byte instruction, byte length, string description = "")
        {
            this.Instruction = instruction;
            this.Length = length;
            this.Description = description;
        }
    }
}
