using System;
using System.Collections.Generic;
using System.Text;

namespace ShameBoy
{
    public class MemoryBank
    {
        /// <summary>
        /// GameBoy BIOS.
        /// </summary>
        private byte[] bios = new byte[256];

        /// <summary>
        /// Loaded ROM.
        /// </summary>
        private byte[] rom = new byte[32768];

        /// <summary>
        /// Static RAM
        /// </summary>
        private byte[] sram = new byte[8192];

        /// <summary>
        /// Input Output (link cable / GB printer / etc?)
        /// </summary>
        private byte[] io = new byte[256];

        /// <summary>
        /// Video RAM
        /// </summary>
        private byte[] vram = new byte[8192];

        /// <summary>
        /// Sprite Attribute Table
        /// </summary>
        private byte[] oam = new byte[256];

        /// <summary>
        /// Work RAM
        /// </summary>
        private byte[] wram = new byte[8192];

        /// <summary>
        /// High RAM.
        /// </summary>
        private byte[] hram = new byte[128];

        public MemoryBank()
        {
            //TODO implement :3
        }

        private ref byte getByteRef(ushort i)
        {
            return ref this.bios[0];
        }

        public byte this[ushort i]
        {
            get { return 0; }
            set { }
        }
    }
}
