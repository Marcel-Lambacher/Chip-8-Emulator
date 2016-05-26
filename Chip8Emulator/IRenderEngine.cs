namespace Chip8Emulator
{
    public interface IRenderEngine
    {
        void Clear();
        void DrawPixelSet(byte[] pixelSet);
    }
}
