using System;
using System.Windows;

using SFML.Graphics;
using System.Windows.Threading;
using SFML.Window;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Chip8_WSharp.Core;
using System.IO;
using System.Collections.Generic;
using System.Windows.Input;

namespace Chip8_WSharp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window {


        private RenderWindow renderWindow;
        private readonly Stopwatch stopWatch = Stopwatch.StartNew();
        private readonly TimeSpan cpuTargetTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 500);
        private readonly TimeSpan displayTargetTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        private TimeSpan lastElapsedTime;


        private bool debugMode = false;
        private bool waitForNextCycle = true;

        Chip8 chip8;

        public MainWindow() {
            InitializeComponent();
            CreateRenderWindow();
            
            chip8 = new Chip8();
            chip8.LoadROM(LoadFile(@"E:\dev\emu\Chip8-WSharp\Chip8-WSharp\roms\Breakout [Carmelo Cortez, 1979].ch8"));

            Task.Run(CpuLoop);

            KeyUp += SetKeyUp;
            KeyDown += SetKeyDown;
        }

        Task CpuLoop() {
            try {
                while (true) {

                    var currentElapsedTime = stopWatch.Elapsed;
                    var elapsedTime = currentElapsedTime - lastElapsedTime;

                    while (elapsedTime >= displayTargetTime) {
                        Dispatcher.Invoke(TickDisplay);
                        elapsedTime -= displayTargetTime;
                        lastElapsedTime += displayTargetTime;
                    }

                    Dispatcher.Invoke(TickCpu);

                    Thread.Sleep(cpuTargetTime);
                }
            }
            catch (TaskCanceledException ex) {
                Application.Current.Shutdown();
                return null;
            }
        }

        void TickDisplay() {
            Draw();
        }

        void TickCpu() {
            if (debugMode && waitForNextCycle)
                return;

            chip8.EmulateCycle();
            memoryView.UpdateProgramCounter(chip8.ProgramCounter);

            if (debugMode)
                waitForNextCycle = true;
        }

        void Draw() {
            var img = ImageFromGfxBuffer(chip8.DisplayBuffer, chip8.ScreenWidth, chip8.ScreenHeight);
            var texture = new Texture(img);
            var sprite = new Sprite {
                Scale = new SFML.System.Vector2f(renderWindow.Size.X / chip8.ScreenWidth, renderWindow.Size.Y / chip8.ScreenHeight)
            };
            sprite.Texture = texture;

            renderWindow.DispatchEvents();
            renderWindow.Draw(sprite);
            renderWindow.Display();
        }

        private void CreateRenderWindow() {
            if (renderWindow != null) {
                renderWindow.SetActive(false);
                renderWindow.Dispose();
            }

            var context = new ContextSettings { DepthBits = 24 };
            renderWindow = new RenderWindow(DrawSurface.Handle, context);
            renderWindow.SetActive(true);
        }

        private void DrawSurface_SizeChanged(object sender, EventArgs e) {
            CreateRenderWindow();
            Draw();
        }

        Dictionary<Key, byte> keys = new Dictionary<Key, byte>() {
            { Key.D1, 0x1 },
            { Key.D2, 0x2 },
            { Key.D3, 0x3 },
            { Key.D4, 0xC },
            { Key.Q, 0x4 },
            { Key.W, 0x5 },
            { Key.E, 0x6 },
            { Key.R, 0xD },
            { Key.A, 0x7 },
            { Key.S, 0x8 },
            { Key.D, 0x9 },
            { Key.F, 0xE },
            { Key.Z, 0xA },
            { Key.X, 0x0 },
            { Key.C, 0xB },
            { Key.V, 0xF }
        };

        void SetKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (keys.ContainsKey(e.Key))
                chip8.KeyUp(keys[e.Key]);
        }

        void SetKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (keys.ContainsKey(e.Key))
                chip8.KeyDown(keys[e.Key]);

            if (e.Key == Key.O && debugMode)
                waitForNextCycle = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
        }

        static byte[] LoadFile(string path) => File.ReadAllBytes(path);

        static Image ImageFromGfxBuffer(bool[,] buffer, uint width, uint height) {
            Color[,] pixels = new Color[width, height];

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    pixels[x, y] = buffer[x, y] ? Color.White : Color.Black;
                }
            }
            return new Image(pixels);
        }
    }
    
}
