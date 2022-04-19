﻿using ShameBoy.Exceptions;
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
        private byte[] bios = new byte[256]
        {
            0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB, 0x21, 0x26, 0xFF, 0x0E,
            0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0,
            0x47, 0x11, 0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
            0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 0xF9,
            0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99, 0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20,
            0xF9, 0x2E, 0x0F, 0x18, 0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
            0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 0x1D, 0x20, 0xF2,
            0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62, 0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06,
            0x7B, 0xE2, 0x0C, 0x3E, 0x87, 0xF2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
            0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17, 0xC1, 0xCB, 0x11, 0x17,
            0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
            0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
            0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
            0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E, 0x3c, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x4C,
            0x21, 0x04, 0x01, 0x11, 0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x20, 0xFE, 0x23, 0x7D, 0xFE, 0x34, 0x20,
            0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x20, 0xFE, 0x3E, 0x01, 0xE0, 0x50
        };

        /// <summary>
        /// Loaded ROM.
        /// </summary>
        private byte[] rom = new byte[32768];

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

        private byte[] eram = new byte[8192];

        private byte[] zram = new byte[128];

        private bool isBiosMapped = true;

        public MemoryBank()
        {
        }

        public MemoryBank(byte[] rom)
        {
            rom.CopyTo(this.rom, 0);
        }

        public byte ReadByte(ushort address)
        {
            /*
             * Extra thanks to http://imrannazar.com/GameBoy-Emulation-in-JavaScript:-Memory
             * for great explanation of the memory mapping
             */
            switch (address & 0xF000) // mask out lower 3 nibbles
            {
                // BIOS
                case 0x0000:
                    if(this.isBiosMapped)
                    {
                        if (address < 0x1000)
                            return this.bios[address];
                        else if (address == 0x0100) // we've hit the end of the bios. disable bios mapping.
                            this.isBiosMapped = false;
                    }
                    return this.rom[address];

                // ROM
                case 0x1000:
                case 0x2000:
                case 0x3000:
                // unbanked ROM
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return this.rom[address];

                // VRAM
                case 0x8000:
                case 0x9000:
                    return this.vram[address & 0x1FFF];

                case 0xA000:
                case 0xB000:
                    return this.eram[address & 0x1FFF];

                case 0xC000:
                case 0xD000:
                    return this.wram[address & 0x1FFF];

                case 0xE000:
                    return this.wram[address & 0x1FFF];

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Graphics: object attribute memory.
                        case 0xE00:
                            if (address < 0xFEA0)
                                return oam[address & 0xFF];
                            else
                                return 0;

                        // zero-page 
                        case 0xF00:
                            if(address >= 0xFF80)
                            {
                                return zram[address & 0x7F];
                            }
                            else
                            {
                                // unimplemented IO
                                return 0;
                            }

                        default:
                            return wram[address & 0x1FFF];
                    }

                default:
                    throw new Exception("address out of bounds!");
            }
        }

        public ushort ReadShort(ushort address)
        {
            return (ushort)(ReadByte(address) + (ReadByte((ushort)(address+1))<<8));
        }

        public void WriteByte(ushort address, byte value)
        {
            switch (address & 0xF000) // mask out lower 3 nibbles
            {
                // ROM
                case 0x0000:
                case 0x1000:
                case 0x2000:
                case 0x3000:
                // unbanked ROM
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    break;

                // VRAM
                case 0x8000:
                case 0x9000:
                    this.vram[address & 0x1FFF] = value;
                    break;

                case 0xA000:
                case 0xB000:
                    this.eram[address & 0x1FFF] = value;
                    break;

                case 0xC000:
                case 0xD000:
                case 0xE000:
                    this.wram[address & 0x1FFF] = value;
                    return;

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Graphics: object attribute memory.
                        case 0xE00:
                            if (address < 0xFEA0)
                                oam[address & 0xFF] = value;
                            break;

                        // zero-page 
                        case 0xF00:
                            if (address >= 0xFF80)
                            {
                                zram[address & 0x7F] = value;
                            }
                            else
                            {
                                // unimplemented IO
                            }
                            break;

                        default:
                            wram[address & 0x1FFF] = value;
                            break;
                    }
                    break;

                default:
                    throw new Exception("address out of bounds!");
            }
        }

        public void WriteShort(ushort address, ushort value)
        {
            var shrt = BitConverter.GetBytes(value);
            WriteByte(address, shrt[0]);
            WriteByte((ushort)(address + 1), shrt[1]);
        }
    }
}
