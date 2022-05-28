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
    public partial class GameBoy
    {
        private Registers registers;
        private MemoryBank memory;
        private ResolvedInstruction[] instructions;
        private byte tStateClock = 0;

        public GameBoy()
        {
            this.registers = new Registers();
            this.memory = new MemoryBank();
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
                var opcode = memory.FetchMemory(this.registers.ProgramCounter, 1)[0];

                this.registers.ProgramCounter++;

                var instruction = this.instructions[opcode];
                var length = instruction.Attribute.Length;

                var args = memory.FetchMemory(this.registers.ProgramCounter, length);
                this.registers.ProgramCounter += length;

                Console.WriteLine($"${opcode:X2} [{string.Join(",", args.ToArray())}]");
                instruction.Invoke(ref this.tStateClock, args);
            }
        }

        public void Reset()
        {
            this.registers = new Registers();
            this.tStateClock = 0;
        }
    }

    public class ResolvedInstruction
    {
        public delegate byte ExecuteInstruction(Span<byte> args);
        public ExecuteInstruction Instruction;
        public InstructionAttribute Attribute;

        public ResolvedInstruction(ExecuteInstruction instruction, InstructionAttribute attribute)
        {
            this.Instruction = instruction;
            this.Attribute = attribute;
        }

        public void Invoke(ref byte tStates, Span<byte> args)
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
