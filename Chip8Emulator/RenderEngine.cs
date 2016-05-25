using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chip8Emulator
{
    public class RenderEngine
    {
        private readonly Brush _foregroundBrush;
        private readonly Color _backgroundColor;

        private readonly Control _panel;

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        private const int EmulatorHeight = 32;
        private const int EmulatorWidth = 64;

        private bool[,] _pixelsToDraw = new bool[EmulatorWidth, EmulatorHeight];
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        private bool _graphicsChanged;       

        public RenderEngine(Control panel, Brush foreground, Color background)
        {
            _panel = panel;
            _foregroundBrush = foreground;
            _backgroundColor = background;

            PixelHeight = 16;
            PixelWidth = 16;

            panel.Paint += PanelOnPaint;
        }

        private void PanelOnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            if (!_graphicsChanged)
            {
                return;
            }

            var graphics = paintEventArgs.Graphics;            
            graphics.Clear(_backgroundColor);

            _readerWriterLock.EnterReadLock();

            for (var xIndex = 0; xIndex < EmulatorWidth; xIndex++)
            {
                for (var yIndex = 0; yIndex < EmulatorHeight; yIndex++)
                {
                    if(_pixelsToDraw[xIndex, yIndex])
                    {
                        graphics.FillRectangle(_foregroundBrush, xIndex * PixelWidth, yIndex * PixelHeight, PixelWidth, PixelHeight);
                    }
                }
            }

            _graphicsChanged = false;
            _readerWriterLock.ExitReadLock();
        }

        public void DrawPixelSet(bool[,] pixelSet)
        {
            if (pixelSet == null || pixelSet.GetLength(0) != EmulatorWidth || pixelSet.GetLength(1) != EmulatorHeight)
            {
                throw new ArgumentException("Invalid pixel set.");
            }

            _readerWriterLock.EnterWriteLock();
            _pixelsToDraw = pixelSet;
            _readerWriterLock.ExitWriteLock();
        }

        public void DrawPixel(int x, int y)
        {
            _pixelsToDraw[x, y] = true;
            _graphicsChanged = true;
            _panel.Invalidate();
        }

        public void ClearPixel(int x, int y)
        {
            _pixelsToDraw[x, y] = false;
            _graphicsChanged = true;
            _panel.Invalidate();
        }

        public void ClearScreen()
        {
            _readerWriterLock.EnterWriteLock();
            _pixelsToDraw = new bool[EmulatorWidth, EmulatorHeight];
            _readerWriterLock.ExitWriteLock();
        }
    }
}
