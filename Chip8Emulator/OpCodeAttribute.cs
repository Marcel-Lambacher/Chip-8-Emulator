using System;

namespace Chip8Emulator
{
    public class OpCodeAttribute: Attribute
    {
        public ushort OpCode { get; private set; }
        public ushort Mask { get; private set; }
        public string Name { get; private set; }
        public string Assembler { get; private set; }

        public OpCodeAttribute(ushort opCode, ushort mask, string name, string assembler)
        {
            OpCode = opCode;
            Mask = mask;
            Name = name;
            Assembler = assembler;
        }
    }
}
