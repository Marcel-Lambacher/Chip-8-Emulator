using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Chip8Emulator.Test
{
    [TestClass]
    public class Chip8Test
    {
        [TestMethod]
        public void CanClearDisplay()
        {
            var ms = new MemoryStream();
            ms.Write(new byte[0x00E0], 0, 1);
            var display = new TestRender();
            display.Screen.Pixels[5, 5] = true;
            var chip8 = new Chip8(ms, display);
            chip8.Start();
            Assert.IsFalse(display.Screen.Pixels[5, 5]);
        }
    }
}
