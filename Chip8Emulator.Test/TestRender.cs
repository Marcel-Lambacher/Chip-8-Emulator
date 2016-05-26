namespace Chip8Emulator.Test
{
    public class TestRender : IRenderEngine
    {
        public Screen Screen { get; private set; }

        public TestRender()
        {
            Screen = new Screen();
        }

        public void Clear()
        {
            Screen.Clear();
        }

        public void DrawPixelSet(byte[] pixelSet)
        {
            for (var x = 0; x < Screen.InternalWidth; x++)
            {
                for (var y = 0; y < Screen.InternalHeight; y++)
                {
                    Screen.Pixels[x, y] = pixelSet[x + (y * 64)] == 1;
                }
            }
        }
    }
}
