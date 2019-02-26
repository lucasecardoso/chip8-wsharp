using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8_WSharp.Core {
    public class Chip8 {

        public ushort ProgramCounter { get { return pc; } }
        public ushort CurrentOpcode { get { return opcode; } }
        public bool[,] DisplayBuffer { get { return gfx;  } }

        public ushort ScreenWidth = 64;
        public ushort ScreenHeight = 32;
        
        private bool[,] gfx;

        private byte[] memory = new byte[4096];
        private byte[] V = new byte[16];
        private byte delayTimer;
        private byte soundTimer;

        private ushort opcode;
        private ushort I;
        private ushort pc;
        private ushort[] stack = new ushort[16];
        private ushort sp;

        private Dictionary<ushort, Action<OpcodeData>> opcodeDict;

        private HashSet<byte> keys;
        private Random rnd;

        #region CPU Initialization and setup
        public Chip8() {
            gfx = new bool[ScreenWidth, ScreenHeight];
            Initialize();
            InitializeOpcodeDict();
            rnd = new Random();
            keys = new HashSet<byte>();
        }

        public void Initialize() {
            pc = 0x200;     // Program counter starts at 0x200;
            opcode = 0;     // Reset current opcode
            I = 0;          // Reset index register
            sp = 0;         // Reset stack pointer

            // Clear display, V registers, memory and stack
            ClearScreen();
            ClearByteArray(V);
            ClearByteArray(memory);
            
            for (int i = 0; i < stack.Length; i++) {
                stack[i] = 0;
            }

            // Load fontset
            WriteFonts();
        }

        public void LoadROM(byte[] data) => Array.Copy(data, 0, memory, 0x200, data.Length);

        void InitializeOpcodeDict() {
            opcodeDict = new Dictionary<ushort, Action<OpcodeData>>() {
                { 0x0, ClearOrReturn },
                { 0x1, Jump },
                { 0x2, Call },
                { 0x3, SkipIfVxEqualsNN },
                { 0x4, SkipIfVxNotEqualNN },
                { 0x5, SkipIfVxEqualsVy },
                { 0x6, SetVx },
                { 0x7, AddVx },
                { 0x8, Arithmetic },
                { 0x9, SkipIfVxNotEqualsVy },
                { 0xA, SetI },
                { 0xB, JumpToV0 },
                { 0xC, SetVxRandom },
                { 0xD, DrawSprite },
                { 0xE, SkipByInput },
                { 0xF, Miscellaneous }
            };
        }

        #endregion

        #region Key Input

        public void KeyDown(byte key) {
            keys.Add(key);
        }

        public void KeyUp(byte key) {
            keys.Remove(key);
        }

        #endregion

        #region Opcode processing
        public void EmulateCycle() {
            ProcessOpcode();

            if (delayTimer > 0) {
                delayTimer--;
            }

            if (soundTimer > 0) {
                // if (soundTimer == 1) BEEP!
                soundTimer--;
            }
        }

        void ProcessOpcode() {
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);

            var data = new OpcodeData() {
                Opcode = opcode,
                NNN = (ushort)(opcode & 0x0FFF),
                NN = (byte)(opcode & 0x00FF),
                N = (byte)(opcode & 0x000F),
                X = (byte)((opcode & 0x0F00) >> 8),
                Y = (byte)((opcode & 0x00F0) >> 4)
            };

            opcodeDict[(byte)(opcode >> 12)](data);
        }
        #endregion

        #region Utility functions
        static void ClearByteArray(byte[] array) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = 0;
            }
        }

        void ClearScreen() {
            for (int x = 0; x < ScreenWidth; x++) {
                for (int y = 0; y < ScreenHeight; y++) {
                    gfx[x, y] = false;
                }
            }
        }

        void WriteFonts() {
            var offset = 0x0;

            foreach (var font in Fontset.Fonts) {
                // Each font is 5 bytes long
                WriteFont(5 * offset, font);
                offset++;
            }
        }

        void WriteFont(int offset, long font) {
            // Fonts are stored in 64 bit numbers, so we need 
            // to manually store them back as shorts
            memory[offset + 0] = (byte)((font & 0xF000000000) >> (8 * 4));
            memory[offset + 1] = (byte)((font & 0x00F0000000) >> (8 * 3));
            memory[offset + 2] = (byte)((font & 0x0000F00000) >> (8 * 2));
            memory[offset + 3] = (byte)((font & 0x000000F000) >> (8 * 1));
            memory[offset + 4] = (byte)((font & 0x00000000F0) >> (8 * 0));
        }

        #endregion

        #region Opcode Implementations
        // 0x0
        void ClearOrReturn(OpcodeData opcode) {
            if (opcode.NN == 0xE0) {
                // Clear instruction
                ClearScreen();
            }
            else if (opcode.NN == 0xEE) {
                // Return program counter to the previous stack pointer
                pc = stack[--sp];
            }

            pc += 2;
        }

        // 0x1
        void Jump(OpcodeData opcode) {
            pc = opcode.NNN;
        }

        // 0x2
        void Call(OpcodeData opcode) {
            // Set the current program counter to the stack pointer
            stack[sp] = pc;

            // Increment the stack pointer so we don't overwrite it in further subroutine calls
            sp++;

            if (sp >= stack.Length) {
                Debug.WriteLine("ROM exceeded stack size");
                Console.WriteLine("ROM exceeded stack size");
                Environment.Exit(-1);
            }

            // Finally, set the program counter to the last three nibbles
            pc = opcode.NNN;

            Console.WriteLine("Jumped to program counter: " + pc.ToString("X"));
            Console.WriteLine("Data at PC in memory: " + ((ushort)memory[pc] << 8 | memory[pc + 1]).ToString("X"));
        }

        // 0x3
        void SkipIfVxEqualsNN(OpcodeData op) {
            if (V[op.X] == op.NN)
                pc += 2;

            pc += 2;
        }

        // 0x4
        void SkipIfVxNotEqualNN(OpcodeData op) {
            if (V[op.X] != op.NN)
                pc += 2;

            pc += 2;
        }

        // 0x5
        void SkipIfVxEqualsVy(OpcodeData op) {
            if (V[op.X] == V[op.Y])
                pc += 2;

            pc += 2;
        }

        // 0x6
        void SetVx(OpcodeData op) {
            V[op.X] = op.NN;
            pc += 2;
        }

        // 0x7
        void AddVx(OpcodeData op) {
            V[op.X] += op.NN;
            pc += 2;
        }

        // 0x8
        void Arithmetic(OpcodeData op) {
            switch (op.N) {
                case 0x0: // Vx = Vy
                    V[op.X] = V[op.Y];
                    break;
                case 0x1: // Vx = Vx | Vy
                    V[op.X] = (byte)(V[op.X] | V[op.Y]);
                    break;
                case 0x2: // Vx = Vx & Vy
                    V[op.X] = (byte)(V[op.X] & V[op.Y]);
                    break;
                case 0x3: // Vx = Vx ^ Vy
                    V[op.X] = (byte)(V[op.X] ^ V[op.Y]);
                    break;
                case 0x4: // Vx += Vy (set carry flag if overflow)
                    V[0xF] = (byte)(V[op.Y] > 0xFF - V[op.X] ? 1 : 0);
                    V[op.X] += V[op.Y];
                    break;
                case 0x5: // Vx -= Vy (set carry flag if underflow)
                    V[0xF] = (byte)(V[op.Y] > V[op.X] ? 1 : 0);
                    V[op.X] -= V[op.Y];
                    break;
                case 0x6: // Store the LSB of Vx in V[F], and shift Vx to the right by 1
                    // We bitwise AND with 0x1 because we're dealing with BITS, not BYTES
                    V[0xF] = (byte)((V[op.X] & 0x1) != 0 ? 1 : 0);
                    V[op.X] = (byte)(V[op.X] >> 1);
                    break;
                case 0x7: // Vx = Vy - Vx. Set carry flag if there's no underflow
                    V[0xF] = (byte)((V[op.X] > V[op.Y]) ? 0 : 1);
                    V[op.X] = (byte)(V[op.Y] - V[op.X]);
                    break;
                case 0x8: // Store the MSB of Vx in V[F], and shift Vy to the left by 1
                    V[0xF] = (byte)((V[op.X] & 0xF) != 0 ? 1 : 0);
                    V[op.X] = (byte)(V[op.X] << 1);
                    break;
            }

            pc += 2;
        }

        // 0x9
        void SkipIfVxNotEqualsVy(OpcodeData op) {
            if (V[op.X] != V[op.Y])
                pc += 2;

            pc += 2;
        }

        // 0xA
        void SetI(OpcodeData op) {
            I = op.NNN;
            pc += 2;
        }

        // 0xB
        void JumpToV0(OpcodeData op) {
            pc = (ushort)(op.NNN + V[0]);
        }

        // 0xC
        void SetVxRandom(OpcodeData op) {
            V[op.X] = (byte)(op.NN & (byte)rnd.Next(0, 256));
            pc += 2;
        }

        // 0xD
        // 0xDXYN: Draws a sprite at the coordinate VX, VY that has a width of 8 pixels and a height of N pixels
        // Each row of 8 pixels is read as bit-coded starting from memory location I; 
        // I value doesn’t change after the execution of this instruction. As described above, 
        // VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
        // and to 0 if that doesn’t happen.
       void DrawSprite(OpcodeData op) {
            ushort startX = V[op.X];
            ushort startY = V[op.Y];
            ushort height = op.N;
            V[0xF] = 0;

            // The opcode tells us how many bytes high the sprite is. We step through each byte and read the line,
            // setting the value to the screen
            for (ushort i = 0; i < height; i++) {
                
                // Read the next line in the sprite. Sprites start at the I index in RAM
                ushort line = memory[I + i];

                // For this byte, set each bit in the screen array
                for (ushort j = 0; j < 8; j++) {

                    // Calculate the screen position based on the current line and the current bit we're checking
                    var x = (startX + j) % ScreenWidth;
                    var y = (startY + i) % ScreenHeight;

                    int oldBit = gfx[x, y] ? 1 : 0;

                    // Do a bit-by-bit check of our current line, starting at the rightmost position and working backwards
                    // Shift the line to the right thus:
                    // 01100110: first bit 0, second bit 1, third bit 1, etc
                    int spriteValue = (line >> (7 - j)) & 1;

                    int newBit = oldBit ^ spriteValue;

                    // TODO: Maintain a separate pending clear buffer
                    gfx[x, y] = newBit == 1;

                    if (oldBit != 0 && newBit == 0)
                        V[0xF] = 0x1;
                }
            }

            Console.WriteLine("V[F] value at the end of draw = " + V[0xF]);

            pc += 2;
        }

        // 0xE
        void SkipByInput(OpcodeData op) {
            switch (op.NN) {
                case 0x9E: if (keys.Contains(V[op.X])) pc += 2; break;
                case 0xA1: if (!keys.Contains(V[op.X])) pc += 2; break;
            }

            pc += 2;
        }

        // 0xF
        void Miscellaneous(OpcodeData op) {
            switch (op.NN) {
                default:
                    Debug.WriteLine("Illegal opcode: " + op.ToString());
                    break;
                case 0x07:
                    V[op.X] = delayTimer;
                    break;
                case 0x0A:
                    // Wait for a key being pressed by looping the current instruction
                    if (keys.Count == 0)
                        pc -= 2;
                    else
                        V[op.X] = keys.First();
                    break;
                case 0x15:
                    delayTimer = V[op.X];
                    break;
                case 0x18:
                    soundTimer = V[op.X];
                    break;
                case 0x1E:
                    I += V[op.X];
                    break;
                case 0x29: // Set I = location of sprite for digit Vx. X is the letter/number we're printing
                           // Every font is 5 bytes long so we just multiply by 5 to get the correct address
                    I = (byte)(V[op.X] * 5);
                    break;
                case 0x33: // Store binary-coded decimal values of Vx into RAM at I, I + 1 and I + 2
                    memory[I + 0] = (byte)((V[op.X] / 100) % 10);
                    memory[I + 1] = (byte)((V[op.X] / 10) % 10);
                    memory[I + 2] = (byte)(V[op.X] % 10);
                    break;
                case 0x55: // Save V0 through VX registers to memory, starting at index I
                    for (int i = 0; i <= op.X; i++) {
                        memory[I + i] = V[i];
                    }
                    break;
                case 0x65: // Load V0 through VX registers from memory, starting at index I
                    for (int i = 0; i <= op.X; i++) {
                        V[i] = memory[I + i];
                    }
                    break;
            }

            pc += 2;
        }


        #endregion
    }
}


public struct OpcodeData {
    public ushort Opcode;
    public ushort NNN;
    public byte NN, X, Y, N;
}

