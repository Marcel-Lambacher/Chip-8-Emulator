/*This is my first emulator based on the virtual processor Chip-8
 * With this tutorial I've implemented this emulator:
 * http://www.multigesture.net/articles/how-to-write-an-emulator-chip-8-interpreter/
 * 
 * This emulator is still in development, sry for ugly code. I'll refactor at the end 
 * of this project ;)
 */

using System;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Chip8Emulator
{
    public class Chip8
    {
        /// <summary>
        /// Represents the current executing operation code.
        /// </summary>
        private ushort _currentOpCode;

        /// <summary>
        /// Represents the available memory for the chip-8.
        /// </summary>
        private byte[] _memory = new byte[4096];

        /// <summary>
        /// This registers can be used in any way for any application which is currently executing.
        /// </summary>
        private byte[] _generalPurposeRegistersV = new byte[16];

        /// <summary>
        /// This register store memory addresses (12bit from the right).
        /// </summary>
        private ushort _indexRegister;

        /// <summary>
        /// Stores the currently executing address od the program.
        /// </summary>
        private ushort _programCounterRegister;

        /// <summary>
        /// Points to the topmpost leven of the stack.
        /// </summary>
        private ushort _stackPointerRegister;

        /// <summary>
        /// Used to store the address that the interpreter should return when finished with a subroutine.
        /// </summary>
        private ushort[] _stack = new ushort[16];

        /// <summary>
        /// General application timer at 60hz.
        /// </summary>
        private byte _delayTimerRegister;

        /// <summary>
        /// Timer for output sound if the value is zero.
        /// </summary>
        private byte _soundTimerRegister;

        /// <summary>
        /// Represents the pressed keys.
        /// </summary>
        private byte[] _keyMap = new byte[16];

        private byte[] _pixelMap = new byte[64 * 32];

        /// <summary>
        /// Will be only set from OP:0x00E0 (clear screen) and OP:0xDXYN (draw sprite on the screen)
        /// </summary>
        private bool _readyToDraw;

        private readonly string _gamePath;

        private readonly RenderEngine _renderEngine;

        private readonly System.Windows.Forms.Timer _timer;

        public Chip8(string gamePath, RenderEngine renderEngine, int clockSpeed)
        {
            _renderEngine = renderEngine;
            _gamePath = gamePath;

            _timer = new Timer();
            _timer.Interval = 1000 / clockSpeed;
            _timer.Tick += (sender, args) => GameTick();
        }

        public void Start()
        {
            Initialize();
            LoadGame();
            _timer.Start();
        }

        /// <summary>
        /// The gaming loop of the chip-8 emulator
        /// </summary>
        private void GameTick()
        {
      
            EmulateCycle();

            if (_readyToDraw)
            {
                DrawGraphics();
                _readyToDraw = false;
            }

            SetKeys();
        }

        /// <summary>
        /// Clears the memory, registers and the screen.
        /// </summary>
        private void Initialize()
        {
            //Chip8 programm start mostly always at address: 0x200
            _programCounterRegister = 0x200;
            _currentOpCode = 0;
            _indexRegister = 0;
            _stackPointerRegister = 0;

            //Load fontset into the memory
            //Data like fonts should be stored between 0x000 and 0x1FF (512bit)
            Buffer.BlockCopy(FontSet.GetFontSet(), 0, _memory, 0, 80);
        }

        /// <summary>
        /// Emulate one cycle of the system.
        /// </summary>
        private void EmulateCycle()
        {
            //fetch operation code
            _currentOpCode = (ushort) ((_memory[_programCounterRegister] << 8) | _memory[_programCounterRegister + 1]);
            Console.WriteLine("New op code: 0x" + _currentOpCode.ToString("X"));
            switch (_currentOpCode & 0xF000)
            {
                case 0x0000:
                    switch (_currentOpCode & 0x000F)
                    {
                        //0x00E0: cls
                        case 0x0000:
                            _renderEngine.ClearScreen();
                            _pixelMap = new byte[64 * 32];
                            _readyToDraw = true;
                            _programCounterRegister += 2;
                        break;

                        //0x00EE: return from a subroutine
                        case 0x00E:
                            //Decrease stack pointer to restore the original program address.
                            _stackPointerRegister--;
                            _programCounterRegister = _stack[_stackPointerRegister];
                            _programCounterRegister += 2;
                        break;
                    }
                break;

                //ANNN: Sets I to the address NNN
                case 0xA000:
                    _indexRegister = (ushort) (_currentOpCode & 0x0FFF);
                    _programCounterRegister += 2;
                break;

                //0x2NNN Calls a subroutine
                case 0x2000:
                    //Stores current programm address into the stack
                    _stack[_stackPointerRegister] = _programCounterRegister;
                    //Prevent stored address to override
                    _stackPointerRegister++;
                    //Call the subroutine at address (op & 0x0FFF)
                    _programCounterRegister = (ushort) (_currentOpCode & 0x0FFF);
                break;

                case 0x8000:
                    //0x8XY4: Adds two numbers (x+y)
                    switch (_currentOpCode & 0x000F)
                    {
                        case 0x0004:
                            if (_generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4]  > (0xFF - _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8]))
                            {
                                //carry, result is larger then 255 
                                _generalPurposeRegistersV[0xF] = 1;
                            }
                            else
                            {
                                _generalPurposeRegistersV[0xF] = 0;
                            }

                            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] += _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
                            _programCounterRegister += 2;
                        break;
                    }
                break;

                case 0xF000:
                    switch (_currentOpCode & 0x00FF)
                    {
                        //0xFx33: The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, 
                        //the tens digit at location I+1, and the ones digit at location I+2.
                        case 0x0033:
                            _memory[_indexRegister] = (byte) (_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8]/100);
                            _memory[_indexRegister + 1] = (byte) ((byte)(_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] / 10) % 10);
                            _memory[_indexRegister + 2] = (byte)((byte)(_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] / 100) % 10);

                            _programCounterRegister += 2;
                        break;
                    }
                break;

                    //Draws a sprite onto the screen. Thos operation will also process collision detection.
                case 0xD000:
                    var x = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
                    var y = _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
                    var height = _currentOpCode & 0x000F;

                    _generalPurposeRegistersV[0xF] = 0;
                    for (var yLine = 0; yLine < height; yLine++)
                    {
                        var pixel = _memory[_indexRegister + yLine];
                        for (var xLine = 0; xLine < 8; xLine++)
                        {
                            if ((pixel & (0x80 >> xLine)) != 0)
                            {
                                if (_pixelMap[(x + xLine + ((y + yLine)*64))] == 1)
                                {
                                    _generalPurposeRegistersV[0xF] = 1;
                                }

                                _pixelMap[x + xLine + ((y + yLine)*64)] ^= 1;
                            }
                        }
                    }

                    _readyToDraw = true;
                    _programCounterRegister += 2;

                break;

                default:
                    Console.WriteLine("Unknow opcode: 0x" + _currentOpCode.ToString("X"));
                    _programCounterRegister += 2;
                    break;
            }

            if (_delayTimerRegister > 0)
            {
                _delayTimerRegister--;
            }

            if (_soundTimerRegister > 0)
            {
                if (_soundTimerRegister == 1)
                {
                    SystemSounds.Beep.Play();
                }

                _soundTimerRegister--;
            }
        }

        /// <summary>
        /// Draw available graphics on the screen.
        /// </summary>
        private void DrawGraphics()
        {
            _renderEngine.DrawPixelSet(_pixelMap);
        }

        /// <summary>
        /// Stores pressed keys into the keymap.
        /// </summary>
        private void SetKeys()
        {
            
        }

        /// <summary>
        /// Loads a game from a file.
        /// </summary>
        private void LoadGame()
        {
            var gameBytes = File.ReadAllBytes(_gamePath);
            Buffer.BlockCopy(gameBytes, 0, _memory, 512, gameBytes.Length);
        }
    }
}
