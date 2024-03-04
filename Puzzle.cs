using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaijiRandomizer
{
    internal class Puzzle
    {
        public enum Symbol
        {
            None = 0,

            OnePip = 1,
            TwoPips = 2,
            ThreePips = 3,
            FourPips = 4,
            FivePips = 5,
            SixPips = 6,
            SevenPips = 7,
            EightPips = 8,
            NinePips = 9,

            OneAntiPip = -1,
            TwoAntiPips = -2,
            ThreeAntiPips = -3,
            FourAntiPips = -4,
            FiveAntiPips = -5,
            SixAntiPips = -6,
            SevenAntiPips = -7,
            EightAntiPips = -8,
            NineAntiPips = -9,

            ZeroPetals = 50,
            OnePetal = 10,
            TwoPetals = 20,
            ThreePetals = 30,
            FourPetals = 40,

            Diamond = 60,
            Bar = 80,
            Slash = 90,
        }

        public enum Color
        {
            Black = 0,
            Gray = 1,
            Teal = 2,
            Gold = 3,
            Purple = 4,
            Blue = 5,

            Red = 7,
            White = 8,
            Yellow = 9,
            PetalPurple = 10,

            BloodRed = 13,
        }

        // Row major, left to right, bottom to top
        private int _width = 0;
        private int _height = 0;
        private List<Symbol> _symbols = new();
        private List<Color> _symbolColors = new();
        private List<bool> _disabledTiles = new();

        public void SetSize(int width, int height)
        {
            _width = width;
            _height = height;
            _symbols.Clear();
            _symbolColors.Clear();
            _disabledTiles.Clear();

            for (int i=0; i<_width*_height; i++)
            {
                _symbols.Add(Symbol.None);
                _symbolColors.Add(Color.Black);
                _disabledTiles.Add(false);
            }
        }

        public void Load(uint id)
        {
            PuzzlePanel panel = SaveSystem.PanelIDtoPanelMap[id];

            SetSize(panel.width, panel.height);

            _disabledTiles = new List<bool>(panel.disabledTiles);
        }

        public void Save(uint id)
        {
            PuzzlePanel panel = SaveSystem.PanelIDtoPanelMap[id];

            for (int i = 0; i < _width * _height; i++)
            {
                panel.symbols[i] = (int)_symbols[i];
                panel.symbolColors[i] = (short)_symbolColors[i];
            }
        }

        public Symbol GetSymbol(int x, int y)
        {
            return _symbols[x + y * _width];
        }

        public Color GetColor(int x, int y)
        {
            return _symbolColors[x + y * _width];
        }

        public void SetSymbol(int x, int y, Symbol symbol, Color color)
        {
            _symbols[x + y * _width] = symbol;
            _symbolColors[x + y * _width] = color;
        }
    }
}
