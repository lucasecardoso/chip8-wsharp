using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chip8_WSharp.View
{
    /// <summary>
    /// Interaction logic for MemoryView.xaml
    /// </summary>
    public partial class MemoryView : UserControl
    {
        List<OpcodeListItem> opcodeItems = new List<OpcodeListItem>();

        public MemoryView()
        {
            InitializeComponent();
            
            memoryDataBinding.Items.Clear();
            
            byte[] rom = File.ReadAllBytes(@"E:\dev\emu\Chip8-WSharp\Chip8-WSharp\roms\Tetris [Fran Dachille, 1991].ch8");

            for (int i = 0; i < rom.Length; i += 2) {
                var opcode = new OpcodeListItem(rom[i], rom[i + 1]);
                opcodeItems.Add(opcode);
            }

            memoryDataBinding.ItemsSource = opcodeItems;
        }

        public void UpdateProgramCounter(ushort pc) {
            int index = (pc - 0x200) / 2;

            memoryDataBinding.SelectedIndex = index;
            memoryDataBinding.ScrollIntoView(memoryDataBinding.SelectedItem);
        }
    }

    class OpcodeListItem {

        byte op1;
        byte op2;

        public OpcodeListItem(byte op1, byte op2) {
            this.op1 = op1;
            this.op2 = op2;
        }

        public override string ToString() {
            return op1.ToString("X2") + op2.ToString("X2");
        }
    }
}
