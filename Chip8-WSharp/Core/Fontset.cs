using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8_WSharp.Core {
    public static class Fontset {

        public static long[] Fonts = new long[] {
            F0,
            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            FA,
            FB,
            FC,
            FD,
            FE,
            FF
        };

        // Font example:
        // DEC   HEX    BIN          SCREEN  
        // 240   0xF0   1111 0000    ****     
        // 144   0x90   1001 0000    *  *      
        // 144   0x90   1001 0000    *  *      
        // 144   0x90   1001 0000    *  *      
        // 240   0xF0   1111 0000    ****      
        //
        // 240  0xF0   1111 0000    ****
        // 16   0x10   0001 0000       *
        // 32   0x20   0010 0000      *
        // 64   0x40   0100 0000     *
        // 64   0x40   0100 0000     *

        // Only the first four nibbles of each byte are used to draw on the screen,
        // so every byte is padded with 0x0

        public const long F0 = 0xF0909090F0;
        public const long F1 = 0x2060202070;
        public const long F2 = 0xF010F080F0;
        public const long F3 = 0xF010F010F0;
        public const long F4 = 0x9090F01010;
        public const long F5 = 0xF080F010F0;
        public const long F6 = 0xF080F090F0;
        public const long F7 = 0xF010204040;
        public const long F8 = 0xF090F090F0;
        public const long F9 = 0xF090F010F0;
        public const long FA = 0xF090F09090;
        public const long FB = 0xE090E090E0;
        public const long FC = 0xF0808080F0;
        public const long FD = 0xE0909090E0;
        public const long FE = 0xF080F080F0;
        public const long FF = 0xF080F08080;
    }
}
