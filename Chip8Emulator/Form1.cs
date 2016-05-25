using System;
using System.Drawing;
using System.Windows.Forms;

namespace Chip8Emulator
{
    public partial class Form1 : Form
    {
        private RenderEngine _renderEngine;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _renderEngine = new RenderEngine(renderPanel, new SolidBrush(Color.Brown), Color.BlanchedAlmond);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var open = new OpenFileDialog();
            open.Filter = @"Chip-8 | *.c8";
            if (open.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var filePath = open.FileName;
            var chip8 = new Chip8(filePath, _renderEngine);
            chip8.GameLoop();
        }
    }
}
