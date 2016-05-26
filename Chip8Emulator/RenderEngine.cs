using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;

namespace Chip8Emulator
{
    public class RenderEngine
    {
        private readonly GLControl _glControl;

        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }

        private readonly Screen _screen;

        public RenderEngine(GLControl glControl)
        {
            _screen = new Screen();
            _glControl = glControl;

            Height = glControl.Height;
            Width = glControl.Width;

            _glControl.Paint += GlControlPaint;
            SetupViewport();
        }

        private void GlControlPaint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Color3(Color.Yellow);
            GL.Begin(PrimitiveType.Quads);

            for (var x = 0; x < _screen.InternalWidth; x++)
            {
                for(var y = 0; y < _screen.InternalHeight; y++)
                {
                    if (_screen.Pixels[x, y])
                    {
                        DrawPixel(x, y);
                    }
                }
            }

            GL.End();
            _glControl.SwapBuffers();
        }

        private void DrawPixel(int x, int y)
        {
            GL.Vertex2(x * Screen.PixelDimension, y * Screen.PixelDimension);
            GL.Vertex2((x * Screen.PixelDimension) + Screen.PixelDimension, y * Screen.PixelDimension);
            GL.Vertex2((x * Screen.PixelDimension) + Screen.PixelDimension, (y * Screen.PixelDimension) + Screen.PixelDimension);
            GL.Vertex2(x * Screen.PixelDimension, (y * Screen.PixelDimension) + Screen.PixelDimension);
        }

        public void SetupViewport()
        {
            GL.ClearColor(Color.SkyBlue);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1, 1);
            GL.Viewport(0, 0, Width, Height);
        }

        public void Draw(int x, int y)
        {
            _screen.Pixels[x, y] = true;
            _glControl.Invalidate();
        }

        public void Clear()
        {
            //GL.ClearColor(Color.SkyBlue);
            _screen.Clear();
            _glControl.Invalidate();
        }

        public void DrawPixelSet(byte[] pixelSet)
        {
            for (var x = 0; x < _screen.InternalWidth; x++)
            {
                for (var y = 0; y < _screen.InternalHeight; y++)
                {
                    if (pixelSet[x + (y * 64)] == 1)
                    {
                        _screen.Pixels[x, y] = true;
                    }
                    else
                    {
                        _screen.Pixels[x, y] = false;
                    }
                }
            }

            _glControl.Invalidate();
        }
    }
}
