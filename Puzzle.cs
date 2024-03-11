using Il2Cpp;
using System.Collections.Immutable;
using UnityEngine;

namespace TaijiRandomizer
{
    internal class Puzzle
    {
        public enum Symbol
        {
            None = 0,

            Dice = 1000,
            OnePip = 1,
            TwoPips = 2,
            ThreePips = 3,
            FourPips = 4,
            FivePips = 5,
            SixPips = 6,
            SevenPips = 7,
            EightPips = 8,
            NinePips = 9,

            AntiDice = 1001,
            OneAntiPip = -1,
            TwoAntiPips = -2,
            ThreeAntiPips = -3,
            FourAntiPips = -4,
            FiveAntiPips = -5,
            SixAntiPips = -6,
            SevenAntiPips = -7,
            EightAntiPips = -8,
            NineAntiPips = -9,

            Flower = 1002,
            ZeroPetals = 50,
            OnePetal = 10,
            TwoPetals = 20,
            ThreePetals = 30,
            FourPetals = 40,

            Diamond = 60,
            Bar = 80,
            Slash = 90,
        }

        public static Symbol GetFlowerWithPetals(int petals)
        {
            switch (petals)
            {
                case 0: return Symbol.ZeroPetals;
                case 1: return Symbol.OnePetal;
                case 2: return Symbol.TwoPetals;
                case 3: return Symbol.ThreePetals;
                default: return Symbol.FourPetals;
            }
        }

        public static int CountFlowerPetals(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.ZeroPetals: return 0;
                case Symbol.OnePetal: return 1;
                case Symbol.TwoPetals: return 2;
                case Symbol.ThreePetals: return 3;
                case Symbol.FourPetals: return 4;
                default: return -1;
            }
        }

        public static bool IsFlower(Symbol symbol)
        {
            return (symbol == Symbol.ZeroPetals || symbol == Symbol.OnePetal || symbol == Symbol.TwoPetals || symbol == Symbol.ThreePetals || symbol == Symbol.FourPetals);
        }

        public static Symbol GetDiceWithPips(int pips)
        {
            switch (pips)
            {
                case 1: return Symbol.OnePip;
                case 2: return Symbol.TwoPips;
                case 3: return Symbol.ThreePips;
                case 4: return Symbol.FourPips;
                case 5: return Symbol.FivePips;
                case 6: return Symbol.SixPips;
                case 7: return Symbol.SevenPips;
                case 8: return Symbol.EightPips;
                case 9: return Symbol.NinePips;
                case -1: return Symbol.OneAntiPip;
                case -2: return Symbol.TwoAntiPips;
                case -3: return Symbol.ThreeAntiPips;
                case -4: return Symbol.FourAntiPips;
                case -5: return Symbol.FiveAntiPips;
                case -6: return Symbol.SixAntiPips;
                case -7: return Symbol.SevenAntiPips;
                case -8: return Symbol.EightAntiPips;
                case -9: return Symbol.NineAntiPips;
                default: return Symbol.None;
            }
        }

        public static int CountDicePips(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.OnePip: return 1;
                case Symbol.TwoPips: return 2;
                case Symbol.ThreePips: return 3;
                case Symbol.FourPips: return 4;
                case Symbol.FivePips: return 5;
                case Symbol.SixPips: return 6;
                case Symbol.SevenPips: return 7;
                case Symbol.EightPips: return 8;
                case Symbol.NinePips: return 9;
                case Symbol.Dice: return 0;
                case Symbol.OneAntiPip: return -1;
                case Symbol.TwoAntiPips: return -2;
                case Symbol.ThreeAntiPips: return -3;
                case Symbol.FourAntiPips: return -4;
                case Symbol.FiveAntiPips: return -5;
                case Symbol.SixAntiPips: return -6;
                case Symbol.SevenAntiPips: return -7;
                case Symbol.EightAntiPips: return -8;
                case Symbol.NineAntiPips: return -9;
                case Symbol.AntiDice: return -10;
                default: return 0;
            }
        }

        public static bool IsDice(Symbol symbol)
        {
            switch (symbol)
            {
                case Symbol.OnePip:
                case Symbol.TwoPips:
                case Symbol.ThreePips:
                case Symbol.FourPips:
                case Symbol.FivePips:
                case Symbol.SixPips:
                case Symbol.SevenPips:
                case Symbol.EightPips:
                case Symbol.NinePips:
                case Symbol.OneAntiPip:
                case Symbol.TwoAntiPips:
                case Symbol.ThreeAntiPips:
                case Symbol.FourAntiPips:
                case Symbol.FiveAntiPips:
                case Symbol.SixAntiPips:
                case Symbol.SevenAntiPips:
                case Symbol.EightAntiPips:
                case Symbol.NineAntiPips:
                    return true;
                default: return false;
            }
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

        public readonly struct Coord : IEquatable<Coord>
        {
            public readonly int x;
            public readonly int y;

            public Coord(int xv, int yv)
            {
                x = xv;
                y = yv;
            }

            public bool Equals(Coord other)
            {
                return (x, y) == (other.x, other.y);
            }

            public override bool Equals(object? obj)
            {
                if (obj == null)
                {
                    return false;
                }

                return Equals(obj as Coord?);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }
        }

        class Tile
        {
            public Symbol symbol;
            public Color color;

            public bool disabled;
            public bool locked;
            public bool lit;

            public bool solution;
        }

        // Row major, left to right, bottom to top
        private int _width = 0;
        private int _height = 0;
        private readonly List<Tile> _tiles = new();
        private readonly List<Coord> _open = new();

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public IImmutableList<Coord> OpenTiles
        {
            get { return _open.ToImmutableList(); }
        }

        public void SetSize(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles.Clear();

            for (int i = 0; i < _width * _height; i++)
            {
                _tiles.Add(new()
                {
                    symbol = Symbol.None,
                    color = Color.Black,
                    disabled = false,
                    locked = false,
                    lit = false,
                    solution = false
                });

                _open.Add(new(i % _width, i / _width));
            }
        }

        // Clears out generated information like symbols, whether the tile is
        // locked, whether it's on in the generated solution, etc. Does not
        // affect puzzle dimensions or disabled tiles.
        public void Clear()
        {
            _open.Clear();

            for (int i = 0; i < _width * _height; i++)
            {
                _tiles[i].symbol = Symbol.None;
                _tiles[i].color = Color.Black;
                _tiles[i].locked = false;
                _tiles[i].lit = false;
                _tiles[i].solution = false;

                if (!_tiles[i].disabled)
                {
                    _open.Add(new(i % _width, i / _width));
                }
            }
        }

        // Loads puzzle dimensions and disabled tiles from game.
        public void Load(uint id)
        {
            PuzzlePanel panel = SaveSystem.PanelIDtoPanelMap[id];

            SetSize(panel.width, panel.height);

            _open.Clear();

            for (int i = 0; i < _width * _height; i++)
            {
                if (panel.disabledTiles[i])
                {
                    _tiles[i].disabled = true;
                }
                else
                {
                    _open.Add(new(i % _width, i / _width));
                }
            }
        }

        public void Save(uint id)
        {
            PuzzlePanel panel = SaveSystem.PanelIDtoPanelMap[id];

            for (int i = 0; i < _width * _height; i++)
            {
                panel.symbols[i] = (int)_tiles[i].symbol;
                panel.symbolColors[i] = (short)_tiles[i].color;
                panel.SetLockedTile(i % _width, i / _width, _tiles[i].locked);
                panel.startingState[i] = _tiles[i].lit;

                // Only do this when not loading from a save.
                panel.currentState[i] = _tiles[i].lit;
            }

            if (panel.transform.childCount > 1)
            {
                GameObject.Destroy(panel.transform.FindChild("Main Sorting Group").gameObject);
                GameObject.Destroy(panel.transform.FindChild("Symbols Sorting Group").gameObject);
                panel.isInitialized = false;
            }
        }

        public void WriteSolution(uint id)
        {
            PuzzlePanel panel = SaveSystem.PanelIDtoPanelMap[id];

            for (int i = 0; i < _width * _height; i++)
            {
                panel.fixedSolution[i] = _tiles[i].solution;
            }
        }

        public Symbol GetSymbol(int x, int y)
        {
            return _tiles[x + y * _width].symbol;
        }

        public Color GetColor(int x, int y)
        {
            return _tiles[x + y * _width].color;
        }

        public void SetSymbol(int x, int y, Symbol symbol, Color color)
        {
            _tiles[x + y * _width].symbol = symbol;
            _tiles[x + y * _width].color = color;

            if (symbol == Symbol.None)
            {
                _open.Add(new(x, y));
            }
            else
            {
                _open.Remove(new(x, y));
            }
        }

        public bool IsDisabled(int x, int y)
        {
            return _tiles[x + y * _width].disabled;
        }

        public bool IsLocked(int x, int y)
        {
            return _tiles[x + y * _width].locked;
        }

        public bool IsLit(int x, int y)
        {
            return _tiles[x + y * _width].lit;
        }

        public void LockTile(int x, int y, bool lit)
        {
            _tiles[x + y * _width].locked = true;
            _tiles[x + y * _width].lit = lit;
        }

        public void LockTile(int x, int y)
        {
            LockTile(x, y, _tiles[x + y * _width].solution);
        }

        public bool IsInSolution(int x, int y)
        {
            return _tiles[x + y * _width].solution;
        }

        public void SetSolution(int x, int y, bool val)
        {
            _tiles[x + y * _width].solution = val;
        }
    }
}
