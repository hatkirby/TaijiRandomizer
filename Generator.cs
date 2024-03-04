using Il2CppSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaijiRandomizer
{
    internal class Generator
    {
        private readonly uint _id;

        private readonly Puzzle _puzzle = new();

        private readonly List<(Puzzle.Symbol symbol, Puzzle.Color color, int amount)> _symbols = new();
        private int _locks = 0;
        private readonly List<(int x, int y, Puzzle.Symbol symbol, Puzzle.Color color)> _forced = new();
        private readonly List<(int x, int y, bool lit)> _locked = new();

        public Generator(uint id)
        {
            _id = id;
            _puzzle.Load(_id);
        }

        public void Add(Puzzle.Symbol symbol, Puzzle.Color color, int amount)
        {
            _symbols.Add((symbol, color, amount));
        }

        public void ForceTile(int x, int y, Puzzle.Symbol symbol, Puzzle.Color color)
        {
            _forced.Add((x, y, symbol, color));
        }

        public void LockTile(int x, int y, bool lit)
        {
            _locked.Add((x, y, lit));
        }

        // This sets the number of solution tiles that will be locked after
        // generation, not including specific locks set with LockTile().
        public void SetLocks(int val)
        {
            _locks = val;
        }

        public void Generate()
        {
            while (!GenerateHelper())
            {
                // Try again.
            }

            _puzzle.Save(_id);
        }

        private bool GenerateHelper()
        {
            _puzzle.Clear();

            // Load in forced tiles.
            foreach (var tile in _forced)
            {
                _puzzle.SetSymbol(tile.x, tile.y, tile.symbol, tile.color);
            }

            // Load in pre-locked tiles.
            foreach (var tile in _locked)
            {
                _puzzle.LockTile(tile.x, tile.y, tile.lit);
                _puzzle.SetSolution(tile.x, tile.y, tile.lit);
            }

            // Create a random solution.
            for (int y = 0; y < _puzzle.Height; y++)
            {
                for (int x = 0; x < _puzzle.Width; x++)
                {
                    if (_puzzle.IsDisabled(x, y) || _puzzle.IsLocked(x, y))
                    {
                        continue;
                    }

                    if (Randomizer.Instance?.Rng?.Next(2) == 0)
                    {
                        _puzzle.SetSolution(x, y, true);
                    }
                }
            }

            // Place any requested diamond symbols.
            foreach (var req in _symbols)
            {
                if (req.symbol == Puzzle.Symbol.Diamond)
                {
                    if (!PlaceDiamonds(req.color, req.amount))
                    {
                        return false;
                    }
                }
            }

            // Randomly lock tiles.
            if (_locks > 0)
            {
                List<Puzzle.Coord> coords = new();
                for (int y = 0; y < _puzzle.Height; y++)
                {
                    for (int x = 0; x < _puzzle.Width; x++)
                    {
                        if (!_puzzle.IsDisabled(x, y) && !_puzzle.IsLocked(x, y))
                        {
                            coords.Add(new(x, y));
                        }
                    }
                }

                for (int i = 0; i < _locks; i++)
                {
                    Puzzle.Coord pos = coords[Randomizer.Instance?.Rng?.Next(coords.Count) ?? 0];
                    _puzzle.LockTile(pos.x, pos.y);
                    coords.Remove(pos);
                }
            }

            return true;
        }

        private List<Puzzle.Coord> GetRegion(Puzzle.Coord pos)
        {
            bool regionState = _puzzle.IsInSolution(pos.x, pos.y);

            HashSet<Puzzle.Coord> visited = new();
            List<Puzzle.Coord> result = new();

            Queue<Puzzle.Coord> queue = new();
            queue.Enqueue(pos);

            Puzzle.Coord next;
            while (queue.TryDequeue(out next))
            {
                if (visited.Contains(next))
                {
                    continue;
                }

                visited.Add(next);

                if (next.x < 0 || next.x >= _puzzle.Width || next.y < 0 || next.y >= _puzzle.Height || _puzzle.IsDisabled(next.x, next.y) || _puzzle.IsInSolution(next.x, next.y) != regionState)
                {
                    continue;
                }

                result.Add(next);

                queue.Enqueue(new(next.x - 1, next.y));
                queue.Enqueue(new(next.x + 1, next.y));
                queue.Enqueue(new(next.x, next.y - 1));
                queue.Enqueue(new(next.x, next.y + 1));
            }

            return result;
        }

        private int CountColor(List<Puzzle.Coord> tiles, Puzzle.Color color)
        {
            int result = 0;
            foreach (var tile in tiles)
            {
                if (_puzzle.GetColor(tile.x, tile.y) == color && _puzzle.GetSymbol(tile.x, tile.y) != Puzzle.Symbol.None)
                {
                    result++;
                }
            }

            return result;
        }

        private bool PlaceDiamonds(Puzzle.Color color, int amount)
        {
            List<Puzzle.Coord> openSet = new(_puzzle.OpenTiles);

            while (amount > 0)
            {
                if (openSet.Count == 0)
                {
                    return false;
                }

                Puzzle.Coord pos = openSet[Randomizer.Instance?.Rng?.Next(openSet.Count) ?? 0];
                List<Puzzle.Coord> region = GetRegion(pos);
                List<Puzzle.Coord> regionOpen = new();

                foreach (Puzzle.Coord p in region)
                {
                    if (openSet.Remove(p))
                    {
                        regionOpen.Add(p);
                    }
                }

                int count = CountColor(region, color);
                if (count >= 2)
                {
                    // Too many of that color.
                    continue;
                }

                if (regionOpen.Count + count < 2)
                {
                    // Not enough space to get two of that color.
                    continue;
                }

                if (count == 0 && amount == 1)
                {
                    // We only want one diamond, but this region doesn't contain a matching item.
                    continue;
                }

                _puzzle.SetSymbol(pos.x, pos.y, Puzzle.Symbol.Diamond, color);
                amount--;

                if (count == 0)
                {
                    // Add a second diamond of that color.
                    regionOpen.Remove(pos);
                    if (regionOpen.Count == 0)
                    {
                        return false;
                    }

                    Puzzle.Coord pos2 = regionOpen[Randomizer.Instance?.Rng?.Next(regionOpen.Count) ?? 0];
                    _puzzle.SetSymbol(pos2.x, pos2.y, Puzzle.Symbol.Diamond, color);
                    amount--;
                }
            }

            return true;
        }
    }
}
