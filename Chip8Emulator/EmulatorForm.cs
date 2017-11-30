using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Threading;

namespace Chip8Emulator
{
    public partial class EmulatorForm : Form
    {
        private RenderEngine _renderEngine;
        private readonly byte[] _keyMap = new byte[16];
        private Chip8 _emulator;
        private Task _emulatorTask;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public EmulatorForm()
        {
            InitializeComponent();
            chipRenderPanel.Load += renderPanel_Load;
        }

        private void renderPanel_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.SkyBlue);
        }

        private void EmulatorForm_Load(object sender, EventArgs e)
        {
            _renderEngine = new RenderEngine(chipRenderPanel);         
        }

        private void openRomButton_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            if (open.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (_emulator != null)
            {
                _cancellationToken?.Cancel();
                _emulator.Dispose();
            }

            var filePath = open.FileName;
            _emulator = new Chip8(new FileStream(filePath, FileMode.Open, FileAccess.Read), _renderEngine)
            {
                GetKeyMap = () => _keyMap
            };

            _cancellationToken = new CancellationTokenSource();
            _emulatorTask = Task.Run(() => _emulator.Start(_cancellationToken.Token));
        }

        private void EmulatorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1)
            {
                _keyMap[0] = 1;
            }
            else if (e.KeyCode == Keys.D2)
            {
                _keyMap[1] = 1;
            }
            else if (e.KeyCode == Keys.D3)
            {
                _keyMap[2] = 1;
            }
            else if (e.KeyCode == Keys.D4)
            {
                _keyMap[3] = 1;
            }
            else if (e.KeyCode == Keys.Q)
            {
                _keyMap[4] = 1;
            }
            else if (e.KeyCode == Keys.W)
            {
                _keyMap[5] = 1;
            }
            else if (e.KeyCode == Keys.E)
            {
                _keyMap[6] = 1;
            }
            else if (e.KeyCode == Keys.R)
            {
                _keyMap[7] = 1;
            }
            else if (e.KeyCode == Keys.A)
            {
                _keyMap[8] = 1;
            }
            else if (e.KeyCode == Keys.S)
            {
                _keyMap[9] = 1;
            }
            else if (e.KeyCode == Keys.D)
            {
                _keyMap[10] = 1;
            }
            else if (e.KeyCode == Keys.F)
            {
                _keyMap[11] = 1;
            }
            else if (e.KeyCode == Keys.Y)
            {
                _keyMap[12] = 1;
            }
            else if (e.KeyCode == Keys.X)
            {
                _keyMap[13] = 1;
            }
            else if (e.KeyCode == Keys.C)
            {
                _keyMap[14] = 1;
            }
            else if (e.KeyCode == Keys.V)
            {
                _keyMap[15] = 1;
            }
        }

        private void EmulatorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D1)
            {
                _keyMap[0] = 0;
            }
            else if (e.KeyCode == Keys.D2)
            {
                _keyMap[1] = 0;
            }
            else if (e.KeyCode == Keys.D3)
            {
                _keyMap[2] = 0;
            }
            else if (e.KeyCode == Keys.D4)
            {
                _keyMap[3] = 0;
            }
            else if (e.KeyCode == Keys.Q)
            {
                _keyMap[4] = 0;
            }
            else if (e.KeyCode == Keys.W)
            {
                _keyMap[5] = 0;
            }
            else if (e.KeyCode == Keys.E)
            {
                _keyMap[6] = 0;
            }
            else if (e.KeyCode == Keys.R)
            {
                _keyMap[7] = 0;
            }
            else if (e.KeyCode == Keys.A)
            {
                _keyMap[8] = 0;
            }
            else if (e.KeyCode == Keys.S)
            {
                _keyMap[9] = 0;
            }
            else if (e.KeyCode == Keys.D)
            {
                _keyMap[10] = 0;
            }
            else if (e.KeyCode == Keys.F)
            {
                _keyMap[11] = 0;
            }
            else if (e.KeyCode == Keys.Y)
            {
                _keyMap[12] = 0;
            }
            else if (e.KeyCode == Keys.X)
            {
                _keyMap[13] = 0;
            }
            else if (e.KeyCode == Keys.C)
            {
                _keyMap[14] = 0;
            }
            else if (e.KeyCode == Keys.V)
            {
                _keyMap[15] = 0;
            }
        }

        private void EmulatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _emulator.Dispose();
        }
    }
}