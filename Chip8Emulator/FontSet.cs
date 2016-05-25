using System;

namespace Chip8Emulator
{
    public static class FontSet
    {
        public static byte[] GetFontSet()
        {
            var result = new byte[80];

            Buffer.BlockCopy(Zero(), 0, result, 0, 5);
            Buffer.BlockCopy(One(), 0, result, 5, 5);
            Buffer.BlockCopy(Two(), 0, result, 10, 5);
            Buffer.BlockCopy(Three(), 0, result, 15, 5);
            Buffer.BlockCopy(Four(), 0, result, 20, 5);
            Buffer.BlockCopy(Five(), 0, result, 25, 5);
            Buffer.BlockCopy(Six(), 0, result, 30, 5);
            Buffer.BlockCopy(Seven(), 0, result, 35, 5);
            Buffer.BlockCopy(Eight(), 0, result, 40, 5);
            Buffer.BlockCopy(Nine(), 0, result, 45, 5);
            Buffer.BlockCopy(A(), 0, result, 50, 5);
            Buffer.BlockCopy(B(), 0, result, 55, 5);
            Buffer.BlockCopy(C(), 0, result, 60, 5);
            Buffer.BlockCopy(D(), 0, result, 65, 5);
            Buffer.BlockCopy(E(), 0, result, 70, 5);
            Buffer.BlockCopy(F(), 0, result, 75, 5);

            return result;
        }

        public static byte[] Zero()
        {
            return new byte[] {0xF0, 0x90, 0x90, 0x90, 0xF0};
        }

        public static byte[] One()
        {
            return new byte[] {0x20, 0x60, 0x20, 0x20, 0x70};
        }

        public static byte[] Two()
        {
            return new byte[] {0xF0, 0x10, 0xF0, 0x80, 0xF0};
        }

        public static byte[] Three()
        {
            return new byte[] {0xF0, 0x10, 0xF0,0x10, 0xF0};
        }

        public static byte[] Four()
        {
            return new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 };
        }

        public static byte[] Five()
        {
            return new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 };
        }

        public static byte[] Six()
        {
            return new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
        }

        public static byte[] Seven()
        {
            return new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 };
        }

        public static byte[] Eight()
        {
            return new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 };
        }

        public static byte[] Nine()
        {
            return new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 };
        }

        public static byte[] A()
        {
            return new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 };
        }

        public static byte[] B()
        {
            return new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 };
        }

        public static byte[] C()
        {
            return new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 };
        }

        public static byte[] D()
        {
            return new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 };
        }

        public static byte[] E()
        {
            return new byte[] { 0xF0, 0x80, 0xF0, 0x70, 0xF0 };
        }

        public static byte[] F()
        {
            return new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };
        }
    }
}
