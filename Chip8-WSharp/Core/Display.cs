using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Chip8_WSharp.Core {
    class Display {

        public static void DrawSFMLSingle(bool[,] gfx, uint width, uint height) {
            var window = new RenderWindow(new VideoMode(640, 320), "Chip8-Sharp");

            var img = ImageFromGfxBuffer(gfx, width, height);
            var texture = new Texture(img);
            var sprite = new Sprite {
                Scale = new SFML.System.Vector2f(window.Size.X / width, window.Size.Y / height)
            };
            sprite.Texture = texture;

            window.Closed += (sender, e) => { ((Window)sender).Close(); };

            while (window.IsOpen) {
                window.Clear();
                window.DispatchEvents();
                window.Draw(sprite);
                window.Display();
            }
        }

        public static void DrawOnConsole(bool[,] gfx, int width, int height) {
            for (int y = 0; y < height; y++) {
                string line = "";

                for (int x = 0; x < width; x++) {
                    line += gfx[x, y] ? "#" : ".";
                }

                Console.WriteLine(line);
            }

        }


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
