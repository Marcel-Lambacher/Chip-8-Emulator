namespace Chip8Emulator
{
    public class Screen
    {
        public static int PixelDimension { get { return  16;} }

        public int InternalHeight { get; set; }
        public int InternalWidth { get; set; }

        public  bool[,] Pixels { get; private set; }

        public Screen()
        {
            InternalWidth = 64;
            InternalHeight = 32;
            Pixels = new bool[InternalWidth, InternalHeight];
        }

        public void Clear()
        {
            Pixels = new bool[InternalWidth, InternalHeight];
        }
    }
}
