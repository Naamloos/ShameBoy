using ShameBoy.Exceptions;
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
        private MemoryBank memory;
        private ResolvedInstruction[] instructions;
        private byte tStateClock = 0;

        public GameBoy(byte[] bios)
        {
            this.registers = new Registers();
            this.memory = new MemoryBank(bios);
            this.instructions = new ResolvedInstruction[256]; // The instructions go from 0x00 to 0xFF, so that'd fit into 256 values.

            // Load CPU instructions into the dictionary
            var methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<InstructionAttribute>() != null)
                .Select(x => (x, x.GetCustomAttribute<InstructionAttribute>())).ToList(); // find methods that have the instruction attr

            for(byte i = 0; i < 0xFF; i++)
            {
                if(methods.Any(x => x.Item2.Instruction == i))
                {
                    var method = methods.FirstOrDefault(x => x.Item2.Instruction == i);
                    var opdelegate = (ResolvedInstruction.ExecuteInstruction)method.x.CreateDelegate(typeof(ResolvedInstruction.ExecuteInstruction), this);
                    var attr = method.Item2;
                    this.instructions[i] = new ResolvedInstruction(opdelegate, attr);
                    Console.WriteLine($"Initialized instruction for 0x{attr.Instruction:X2}: {attr.Description}");
                }
                else
                {
                    Console.WriteLine($"No instruction implemented for 0x{i:X2}. Setting empty.");
                    this.instructions[i] = ResolvedInstruction.CreateEmpty(i);
                }
            }
        }

        public void Start()
        {
            while(true)
            {
                var opcode = memory.ReadByte(this.registers.ProgramCounter);
                this.registers.ProgramCounter++;
                var instruction = this.instructions[opcode];
                var args = new byte[instruction.Attribute.Length - 1];
                for(byte i =0; i < args.Length; i++)
                {
                    args[i] = memory.ReadByte(this.registers.ProgramCounter);
                    this.registers.ProgramCounter++;
                }
                instruction.Invoke(ref this.tStateClock, args);
            }
        }

        public void Reset()
        {
            this.registers = new Registers();
            this.tStateClock = 0;
        }

        // args: opcode, length in bytes, description
        [Instruction(0x00, 1, "NOP - No instruction")]
        private byte NOP(params byte[] args)
        {
            // Return amount of T-states
            return 4;
        }

        [Instruction(0x01, 3, "LD BC - Load 16-bit into BC")]
        private byte LD_BC(params byte[] args)
        {
            this.registers.BC = memory.ReadShort(BitConverter.ToUInt16(args, 0));
            return 12;
        }

        [Instruction(0x02, 1, "LD (BC), A - Load 8-bit value at A into address at pointer (BC)")]
        private byte LD_A_BC(params byte[] args)
        {
            memory.WriteByte(registers.BC, registers.A);
            return 8;
        }

        [Instruction(0x03, 1, "INC BC - Increase BC by 1")]
        private byte INC_BC(params byte[] args)
        {
            // increase BC. no flags to be set.
            this.registers.BC++;
            return 8;
        }

        [Instruction(0x04, 1, "INC B - Increase B by 1")]
        private byte INC_B(params byte[] args)
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
        private byte DEC_B(params byte[] args)
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
            this.registers.B = memory.ReadByte(args[0]);
            return 8;
        }
    }

    public class ResolvedInstruction
    {
        public delegate byte ExecuteInstruction(params byte[] args);
        public ExecuteInstruction Instruction;
        public InstructionAttribute Attribute;

        public ResolvedInstruction(ExecuteInstruction instruction, InstructionAttribute attribute)
        {
            this.Instruction = instruction;
            this.Attribute = attribute;
        }

        public void Invoke(ref byte tStates, params byte[] args)
        {
            tStates = Instruction.Invoke(args);
        }

        public static ResolvedInstruction CreateEmpty(byte value)
        {
            return new ResolvedInstruction(x => 
            {
                throw new UnknownInstructionException(value);
            }, new InstructionAttribute(value, 1, "Unknown Instruction"));
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
