using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;

namespace Chip8Emulator
{
    public class Chip8: IDisposable
    {
        /// <summary>
        /// Represents the current executing operation code.
        /// </summary>
        private ushort _currentOpCode;

        /// <summary>
        /// Represents the available memory for the chip-8.
        /// </summary>
        private readonly byte[] _memory = new byte[4096];

        /// <summary>
        /// This registers can be used in any way for any application which is currently executing.
        /// </summary>
        private readonly byte[] _generalPurposeRegistersV = new byte[16];

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
        private readonly ushort[] _stack = new ushort[16];

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

        private readonly Stream _romStream;
        private readonly IRenderEngine _renderEngine;
        private readonly Random _random = new Random(46546545);

        private bool _gameIsRunning;
        private int _emptyOperationCount;

        public Chip8(Stream romStream, IRenderEngine renderEngine)
        {
            _renderEngine = renderEngine;
            _romStream = romStream;        
        }

        public Func<byte[]> GetKeyMap { get; set; }
        private readonly Action[] _jumpTable = new Action[0xFFFF];
        private OpCodeAttribute[] _opCodes;

        public void Start(CancellationToken cancellationToken)
        {
            Initialize();
            InitializeJumpTable();
            LoadGame();
            GameTick(cancellationToken);
        }

        /// <summary>
        /// The gaming loop of the chip-8 emulator
        /// </summary>
        private void GameTick(CancellationToken cancellationToken)
        {
            //Stopwatch watch = new Stopwatch();
            var instructionCount = 0;
            _gameIsRunning = true;

            var clockWatch = new Stopwatch();
            clockWatch.Start();

            while (_gameIsRunning && !cancellationToken.IsCancellationRequested)
            {
                clockWatch.Restart();

                EmulateCycle();

                //Updates the internal timer at a 60hz frequenz
                //540hz (game tick) divided by 9 equals 60hz (timer tick)
                instructionCount++;
                if(instructionCount == 9)
                {
                    UpdateSoundAndDelay();
                    instructionCount = 0;
                }

                if (_readyToDraw)
                {
                    DrawGraphics();
                    _readyToDraw = false;
                }

                SetKeys();

                //Pause the game to get a virtual clock speed of ca. 540mhz
                var elapsedMicroseconds = clockWatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                while (elapsedMicroseconds < 1852)
                {
                    elapsedMicroseconds = clockWatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                }     
            }
        }

        /// <summary>
        /// Clears the memory, registers and the screen.
        /// </summary>
        private void Initialize()
        {
            //Chip8 programm start always at address: 0x200
            _programCounterRegister = 0x200;
            _currentOpCode = 0;
            _indexRegister = 0;
            _stackPointerRegister = 0;

            //Load fontset into the memory
            //Data like fonts should be stored between 0x000 and 0x1FF (512bit)
            Buffer.BlockCopy(FontSet.GetFontSet(), 0, _memory, 0, 80);
        }

        private void InitializeJumpTable()
        {
            var methods = typeof (Chip8).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(method => method.GetCustomAttribute<OpCodeAttribute>() != null).ToList();
            _opCodes = new OpCodeAttribute[methods.Count];
            var index = 0;

            foreach (var methodInfo in methods)
            {
                var opCode = methodInfo.GetCustomAttribute<OpCodeAttribute>();
                _jumpTable[opCode.OpCode] = Delegate.CreateDelegate(typeof (Action), this, methodInfo) as Action;
                _opCodes[index] = opCode;
                index++;
            }
        }

        private void UpdateSoundAndDelay()
        {
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
        /// Emulate one cycle of the system.
        /// </summary>
        private void EmulateCycle()
        {
            //fetch operation code
            _currentOpCode = (ushort)((_memory[_programCounterRegister] << 8) | _memory[_programCounterRegister + 1]);
            

            if(_currentOpCode == 0x0)
            {
                _emptyOperationCount++;
                if(_emptyOperationCount == 10)
                {
                    _gameIsRunning = false;
                }
            }

            for (var opIndex = 0; opIndex < _opCodes.Length; opIndex++)
            {
                if ((_currentOpCode & _opCodes[opIndex].Mask) == _opCodes[opIndex].OpCode)
                {
                    _jumpTable[_opCodes[opIndex].OpCode]();
                }
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
            if (GetKeyMap == null)
            {
                return;
            }

            _keyMap = GetKeyMap();
        }

        /// <summary>
        /// Loads a game from a file.
        /// </summary>
        private void LoadGame()
        {
            const int gameStartPosition = 512;

            var buffer = new byte[4096 - gameStartPosition];
            byte[] game;
            using(var memoryStream = new MemoryStream())
            {
                int bytesRead;
                while((bytesRead = _romStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                game = memoryStream.ToArray();
            }

            // Roms are always stored at memory index 512.
            Buffer.BlockCopy(game, 0, _memory, gameStartPosition, game.Length);
        }

        #region Processor operations

        [OpCode(0x00E0, 0xFFFF, "0x00E0", "CLS")]
        private void ClearScreen()
        {
            _renderEngine.Clear();
            _pixelMap = new byte[64 * 32];
            _readyToDraw = true;
            _programCounterRegister += 2;
        }

        [OpCode(0x00EE, 0xFFFF, "0x00EE", "RET")]
        private void ReturnFromSubroutine()
        {
            //Decrease stack pointer to restore the original program address.
            _stackPointerRegister--;
            _programCounterRegister = _stack[_stackPointerRegister];
            _programCounterRegister += 2;
        }

        [OpCode(0x1000, 0xF000, "0x1NNN", "JP addr")]
        private void JumpToNnn()
        {
            var address = _currentOpCode & 0xFFF;
            _programCounterRegister = (ushort)address;
        }

        [OpCode(0x2000, 0xF000, "0x2NNN", "Call addr")]
        private void CallSubroutine()
        {
            //Stores current programm address into the stack
            _stack[_stackPointerRegister] = _programCounterRegister;
            //Prevent stored address to override
            _stackPointerRegister++;
            //Call the subroutine at address (op & 0x0FFF)
            _programCounterRegister = (ushort)(_currentOpCode & 0x0FFF);
        }

        [OpCode(0x3000, 0xF000, "0x3XKK", "SE Vx, byte")]
        private void SkipInstructionIfVxEqualsKk()
        {
            var vResult = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
            if (vResult == (_currentOpCode & 0x00FF))
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0x4000, 0xF000, "0x4xKK", "SNE Vx, byte")]
        private void SkipInstructionIfVxNotEqualsKk()
        {
            var result = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
            if (result != (_currentOpCode & 0x00FF))
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0x5000, 0xF000, "0x5XY0", "SE Vx, Vy")]
        private void SkipInstructionIfVxEqualsVy()
        {
            if (_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] == _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4])
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0x6000, 0xF000, "0x6XKK", "LD Vx, byte")]
        private void StoreKkIntoVx()
        {
            var value = _currentOpCode & 0x00FF;
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] = (byte)value;
            _programCounterRegister += 2;
        }

        [OpCode(0x7000, 0xF000, "0x7XKK", "ADD Vx, byte")]
        private void AddsKkToVx()
        {
            var value = _currentOpCode & 0x00FF;
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] += (byte)value;
            _programCounterRegister += 2;
        }

        [OpCode(0x8000, 0xF00F, "0x8XY0", "LD Vx, Vy")]
        private void StoreVyIntoVx()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] = _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x8001, 0xF00F, "0x8XY1", "OR Vx, Vy")]
        private void VxOrVy()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] |= _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x8002, 0xF00F, "0x8XY2", "AND Vx, Vy")]
        private void VxAndVy()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] &= _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x8003, 0xF00F, "0x8XY3", "XOR Vx, Vy")]
        private void VxXorVy()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] ^= _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x8004, 0xF00F, "0x8XY4", "Add Vx, Vy")]
        private void VxAddsVy()
        {
            if (_generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4] > (0xFF - _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8]))
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
        }

        [OpCode(0x8005, 0xF00F, "0x8XY5", "SUB Vx, Vy")]
        private void VxSbubVy()
        {
            if (_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] > (_generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4]))
            {
                //There is no barrow
                _generalPurposeRegistersV[0xF] = 1;
            }
            else
            {
                _generalPurposeRegistersV[0xF] = 0;
            }

            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] -= _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x8006, 0xF00F, "0x8XY6", "SHR Vx {, Vy}")]
        private void VxDivideByTwo()
        {
            _generalPurposeRegistersV[0xF] = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] &= 0x1;
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] >>= 1;
            _programCounterRegister += 2;
        }

        [OpCode(0x8007, 0xF00F, "0x8XY7", "SUBN Vx, Vy")]
        private void VxMinusVyWithBarrow()
        {
            if (_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] > _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4])
            {
                _generalPurposeRegistersV[0xF] = 1;
            }
            else
            {
                _generalPurposeRegistersV[0xF] = 0;
            }

            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] -= _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4];
            _programCounterRegister += 2;
        }

        [OpCode(0x800E, 0xF00F, "0x8XYE", "SHL Vx {, Vy}")]
        private void VxMultipleByTwo()
        {
            _generalPurposeRegistersV[0xF] = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] &= 0x1;
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] <<= 1;
            _programCounterRegister += 2;
        }

        [OpCode(0x900E, 0xF00F, "0x9XYE", "SNE Vx, Vy")]
        private void SkipInstrutionIfVxAndVyDoesNotEquals()
        {
            if (_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] != _generalPurposeRegistersV[(_currentOpCode & 0x00F0) >> 4])
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0xA000, 0xF000, "0xANNN", "LD I, addr")]
        private void SetIToNnn()
        {
            _indexRegister = (ushort)(_currentOpCode & 0x0FFF);
            _programCounterRegister += 2;
        }

        [OpCode(0xB000, 0xF000, "0xBNNN", "JP V0, addr")]
        private void JumpToAddressV0PlusNnn()
        {
            _programCounterRegister = (ushort)(_generalPurposeRegistersV[0] + (_currentOpCode & 0x0FFF));
        }

        [OpCode(0xC000, 0xF000, "0xCXKK", "RND Vx, byte")]
        private void RandomNumberAndKkIntoVx()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] = (byte)((_random.Next(256) & 0xFF) & (_currentOpCode & 0x00FF));
            _programCounterRegister += 2;
        }

        [OpCode(0xD000, 0xF000, "0xDXYN", "DRW Vx, Vy, nibble")]
        private void DrawSprite()
        {
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
                        if (x + xLine + ((y + yLine) * 64) >= _pixelMap.Length)
                        {
                            continue;
                        }

                        if (_pixelMap[(x + xLine + ((y + yLine) * 64))] == 1)
                        {
                            _generalPurposeRegistersV[0xF] = 1;
                        }

                        _pixelMap[x + xLine + ((y + yLine) * 64)] ^= 1;
                    }
                }
            }

            _readyToDraw = true;
            _programCounterRegister += 2;
        }

        [OpCode(0xE09E, 0xF0FF, "0xEX9E", "SKP Vx")]
        private void SkipInstructionIfPressedKeyEqualsVx()
        {
            if (_keyMap[_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8]] != 0)
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0xE0A1, 0xF0FF, "0xEXA1", "SKNP Vx")]
        private void SkipInstructionIfPressedKeyNotEqualsVx()
        {
            if (_keyMap[_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8]] == 0)
            {
                _programCounterRegister += 4;
            }
            else
            {
                _programCounterRegister += 2;
            }
        }

        [OpCode(0xF007, 0xF0FF, "0xFX07", "LD Vx, DT")]
        private void ValueOfDelayTimerIntoVx()
        {
            _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] = _delayTimerRegister;
            _programCounterRegister += 2;
        }

        [OpCode(0xF00A, 0xF0FF, "0xFX0A", "LD Vx, K")]
        private void WaitUntilKeyPressed()
        {
            for (var keyIndex = 0; keyIndex < _keyMap.Length; keyIndex++)
            {
                while (_keyMap[keyIndex] != 1)
                {
                    _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] = (byte)keyIndex;
                }
            }
            _programCounterRegister += 2;
        }

        [OpCode(0xF015, 0xF0FF, "0xFX15", "LD DT, Vx")]
        private void SetsDelayTimerToVx()
        {
            var value = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
            _delayTimerRegister = value;
            _programCounterRegister += 2;
        }

        [OpCode(0xF018, 0xF0FF, "0xFX18", "LD ST, Vx")]
        private void SetsSoundTimerToVx()
        {
            var value = _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
            _soundTimerRegister = value;
            _programCounterRegister += 2;
        }

        [OpCode(0xF01E, 0xF0FF, "0xFX1E", "ADD I, Vx")]
        private void AddsVxToI()
        {
            if (_indexRegister + _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] > 0xFFF)
            {
                _generalPurposeRegistersV[0xF] = 1;
            }
            else
            {
                _generalPurposeRegistersV[0xF] = 1;
            }

            _indexRegister += _generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8];
            _programCounterRegister += 2;
        }

        [OpCode(0xF029, 0xF0FF, "0xFX29", "LD F, Vx")]
        private void SpriteLocationFromVxIntoI()
        {
            _indexRegister = (ushort)(_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] * 0x5);
            _programCounterRegister += 2;
        }

        [OpCode(0xF033, 0xF0FF, "0xFX33", "LD B, Vx")]
        private void VxToBcd()
        {
            _memory[_indexRegister] = (byte)(_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] / 100);
            _memory[_indexRegister + 1] = (byte)((_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] % 100) / 10);
            _memory[_indexRegister + 2] = (byte)((_generalPurposeRegistersV[(_currentOpCode & 0x0F00) >> 8] % 100) % 10);

            _programCounterRegister += 2;
        }

        [OpCode(0xF055, 0xF0FF, "0xFX55", "LD [I], Vx")]
        private void CopyV0TillVxIntoMemory()
        {
            for (var index = 0; index <= ((_currentOpCode & 0x0F00) >> 8); index++)
            {
                _memory[_indexRegister + index] = _generalPurposeRegistersV[index];
            }

            _indexRegister += (ushort)(((_currentOpCode & 0x0F00) >> 8) + 1);
            _programCounterRegister += 2;
        }

        [OpCode(0xF065, 0xF0FF, "0xFX65", "LD Vx, [I]")]
        private void ReadV0TillVxFromMemory()
        {
            for (var index = 0; index <= ((_currentOpCode & 0x0F00) >> 8); index++)
            {
                _generalPurposeRegistersV[index] = _memory[_indexRegister + index];
            }

            _indexRegister += (ushort)(((_currentOpCode & 0x0F00) >> 8) + 1);
            _programCounterRegister += 2;
        }

        public void Dispose()
        {
            _romStream?.Dispose();
        }
        #endregion
    }
}