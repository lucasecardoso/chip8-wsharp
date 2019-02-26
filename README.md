# Chip8-WSharp
A simple Chip-8 interpreter for Windows, written in C# and using WPF for the UI and SFML for video.

This is a hobby project I developed to learn about emulation development. It is not intended as a perfect Chip-8 interpreter, and some features haven't been implemented yet. It *should* emulate most of the standard Chip-8 roms somewhat accurately. The UI is very bare-bones and is mainly there for debug purposes, but it should track the program counter correctly.

Input keys are 1-4, Q-R, A-F and Z-V. The clock speed of the CPU has been set to 500hz. 

The interpreter has a debug flag in the code to step through the CPU cycles manually. Once set to true, you can step through the code by pressing O on your keyboard.

Most of this was based off [Cowgod's Chip-8 technical reference](http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#0nnn), and [Danny Tupeny's excellent Chip-8 emulation article](https://blog.dantup.com/2016/06/building-a-chip-8-interpreter-in-csharp/).
