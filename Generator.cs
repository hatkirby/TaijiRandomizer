using static TaijiRandomizer.Canvas;

namespace TaijiRandomizer
{
    internal class Generator
    {
        private readonly uint _id;

        private readonly Puzzle _puzzle = new();

        private readonly List<(Puzzle.Symbol symbol, Puzzle.Color color, int amount)> _symbols = new();

        private readonly Dictionary<Puzzle.Color, int> _shapes = new();

        private readonly Dictionary<Puzzle.Color, List<int>> _dice = new();

        private readonly Dictionary<int, int> _flowers = new();
        private int _wildcardFlowers = 0;

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
            if (Puzzle.IsDice(symbol) || symbol == Puzzle.Symbol.Dice || symbol == Puzzle.Symbol.AntiDice)
            {
                if (!_dice.ContainsKey(color))
                {
                    _dice[color] = new();
                }

                for (int i = 0; i < amount; i++)
                {
                    _dice[color].Add(Puzzle.CountDicePips(symbol));
                }
            }
            else if (symbol == Puzzle.Symbol.Bar)
            {
                _shapes[color] = amount;
            }
            else
            {
                _symbols.Add((symbol, color, amount));
            }
        }

        public void SetFlowers(int petals, int amount)
        {
            if (petals >= 0 && petals <= 4)
            {
                _flowers[petals] = amount;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("petals");
            }
        }

        public void SetWildcardFlowers(int amount)
        {
            _wildcardFlowers = amount;
        }

        public void ForceTile(int x, int y, Puzzle.Symbol symbol, Puzzle.Color color)
        {
            // Does not work yet.
            //_forced.Add((x, y, symbol, color));
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

            // If there are shapes in this puzzle, we have to be strategic with the solution we create.
            // Otherwise, we can just generate a random solution.
            if (_shapes.Count > 0)
            {
                if (!PlaceShapes())
                {
                    return false;
                }
            }
            else
            {
                GenerateRandomSolution();
            }

            // Place any requested dice symbols.
            if (_dice.Count > 0)
            {
                if (!PlaceDice())
                {
                    return false;
                }
            }

            // Place any requested specific flower symbols.
            if (_flowers.Count > 0)
            {
                if (!PlaceFlowers())
                {
                    return false;
                }
            }

            // Place any requested wildcard flower symbols.
            if (_wildcardFlowers > 0)
            {
                if (!PlaceWildcardFlowers())
                {
                    return false;
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

        private void GenerateRandomSolution()
        {
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

        private List<List<Puzzle.Coord>> GetAllOpenRegions()
        {
            List<List<Puzzle.Coord>> result = new();

            List<Puzzle.Coord> search = new(_puzzle.OpenTiles);

            while (search.Count > 0)
            {
                List<Puzzle.Coord> region = GetRegion(search[0]);
                foreach (Puzzle.Coord pos in region)
                {
                    search.Remove(pos);
                }

                result.Add(region);
            }

            return result;
        }

        private int CountColor(List<Puzzle.Coord> tiles, Puzzle.Color color)
        {
            int result = 0;
            foreach (var tile in tiles)
            {
                Puzzle.Symbol symbol = _puzzle.GetSymbol(tile.x, tile.y);

                if (Puzzle.IsFlower(symbol))
                {
                    if (color == Puzzle.Color.Gold && Puzzle.CountFlowerPetals(symbol) >= 1)
                    {
                        result++;
                    }
                    else if (color == Puzzle.Color.PetalPurple && Puzzle.CountFlowerPetals(symbol) < 4)
                    {
                        result++;
                    }
                }
                else if (symbol != Puzzle.Symbol.None)
                {
                    if (_puzzle.GetColor(tile.x, tile.y) == color)
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        private int CountAdjacentMatchingTiles(Puzzle.Coord coord)
        {
            int result = 0;
            bool match = _puzzle.IsInSolution(coord.x, coord.y);
            List<Puzzle.Coord> coords = new()
            {
                new(coord.x-1, coord.y),
                new(coord.x+1, coord.y),
                new(coord.x, coord.y-1),
                new(coord.x, coord.y+1)
            };

            foreach (Puzzle.Coord adj in coords)
            {
                if (adj.x >= 0 && adj.x < _puzzle.Width && adj.y >= 0 && adj.y < _puzzle.Height && !_puzzle.IsDisabled(adj.x, adj.y) && _puzzle.IsInSolution(adj.x, adj.y) == match)
                {
                    result++;
                }
            }

            return result;
        }

        private bool PlaceShapes()
        {
            Canvas canvas = new(_puzzle);

            foreach ((Puzzle.Color color, int bars) in _shapes)
            {
                // TODO: Ensure different colors don't have the same shape.
                Shape shape = new();
                shape.Generate(canvas.Width, canvas.Height, 1, 10);

                for (int i = 0; i < bars; i++)
                {
                    // TODO: Canvas could do this, as well as cache untouched choices.
                    List<(int x, int y, Canvas.Decision decision)> choices = new();
                    for (int cy = 0; cy <= canvas.Height - shape.Height + 2; cy++)
                    {
                        for (int cx = 0; cx <= canvas.Width - shape.Width + 2; cx++)
                        {
                            Canvas.Decision decision = canvas.CanPlaceShape(cx, cy, shape);
                            if (decision == Canvas.Decision.No)
                            {
                                continue;
                            }

                            choices.Add(new(cx, cy, decision));
                        }
                    }

                    if (choices.Count == 0)
                    {
                        return false;
                    }

                    var choice = choices[Randomizer.Instance?.Rng?.Next(choices.Count) ?? 0];
                    bool on = false;
                    if (choice.decision == Canvas.Decision.YesOn)
                    {
                        on = true;
                    }
                    else if (choice.decision == Canvas.Decision.YesEither && (Randomizer.Instance?.Rng?.Next(2) == 0))
                    {
                        on = true;
                    }

                    if (shape.Pivot == null)
                    {
                        List<Puzzle.Coord> openTiles = canvas.GetOpenCellsInShape(choice.x, choice.y, shape);
                        if (openTiles.Count == 0)
                        {
                            // TODO: Try another choice rather than quitting.
                            return false;
                        }

                        shape.Pivot = openTiles[Randomizer.Instance?.Rng?.Next(openTiles.Count) ?? 0];
                    }

                    canvas.PlaceShape(choice.x, choice.y, shape, on);

                    _puzzle.SetSymbol((shape.Pivot?.x ?? 0) - 1 + choice.x, (shape.Pivot?.y ?? 0) - 1 + choice.y, Puzzle.Symbol.Bar, color);
                }
            }

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    Canvas.CellType canvasCell = canvas.GetCell(x, y);
                    if (canvasCell == CellType.Undecided)
                    {
                        if (Randomizer.Instance?.Rng?.Next(2) == 0)
                        {
                            _puzzle.SetSolution(x, y, true);
                        }
                    }
                    else if (canvasCell == CellType.On)
                    {
                        _puzzle.SetSolution(x, y, true);
                    }
                }
            }

            return true;
        }

        private bool PlaceDice()
        {
            // Gotta do a deep copy.
            Dictionary<Puzzle.Color, List<int>> dice = new();
            foreach (var mapping in _dice)
            {
                dice[mapping.Key] = new(mapping.Value);
            }

            List<(int pips, Puzzle.Color color)> items = new();
            Dictionary<Puzzle.Color, bool> colorHasAntis = new();
            foreach (Puzzle.Color color in dice.Keys)
            {
                colorHasAntis[color] = false;
            }
            foreach (var mapping in dice)
            {
                foreach (int item in mapping.Value)
                {
                    items.Add((item, mapping.Key));

                    if (item < 0)
                    {
                        colorHasAntis[mapping.Key] = true;
                    }
                }
            }

            items.Sort(new Comparison<(int pips, Puzzle.Color)>((i1, i2) => i2.pips.CompareTo(i1.pips)));

            List<List<Puzzle.Coord>> regions = GetAllOpenRegions().OrderBy(_ => Randomizer.Instance?.Rng?.Next()).ToList();

            while (items.Count > 0)
            {
                if (regions.Count == 0)
                {
                    // No more regions left to fill.
                    return false;
                }

                var item = items[0];

                if (item.pips < 0)
                {
                    // We ran out of dice but still have anti-dice remaining.
                    return false;
                }

                List<int> set = new(dice[item.color]);
                set.Remove(item.pips);

                int? placed = null;
                for (int regionIndex = 0; regionIndex < regions.Count; regionIndex++)
                {
                    var region = regions[regionIndex];

                    if (item.pips > region.Count && !colorHasAntis[item.color])
                    {
                        // Short-circuit out of this region if the die is too big and we have no antis.
                        continue;
                    }

                    List<Puzzle.Coord> openTiles = new();
                    foreach (Puzzle.Coord pos in region)
                    {
                        if (_puzzle.GetSymbol(pos.x, pos.y) == Puzzle.Symbol.None)
                        {
                            openTiles.Add(pos);
                        }
                    }

                    List<List<int>> options = new();

                    if (region.Count == item.pips && !colorHasAntis[item.color])
                    {
                        options.Add(new(item.pips));
                    }
                    else
                    {
                        // Brute force the subset sum problem. The sets are likely pretty small so it should be okay.
                        int powerSetCount = 1 << set.Count;
                        for (int setMask = 0; setMask < powerSetCount; setMask++)
                        {
                            int subsetSum = 0;
                            int symbolCount = 1;
                            int wildcards = 0;
                            int antiWildcards = 0;

                            for (int i = 0; i < set.Count; i++)
                            {
                                if ((setMask & (1 << i)) > 0)
                                {
                                    symbolCount++;

                                    if (set[i] == 0)
                                    {
                                        wildcards++;
                                    }
                                    else if (set[i] == -10)
                                    {
                                        antiWildcards++;
                                    }
                                    else
                                    {
                                        subsetSum += set[i];
                                    }
                                }
                            }

                            if (symbolCount > openTiles.Count)
                            {
                                continue;
                            }

                            if (item.pips == 0)
                            {
                                wildcards++;
                            }

                            int goal = region.Count - item.pips;
                            int lowerBound = subsetSum - antiWildcards * 9 + wildcards * 2;
                            int upperBound = subsetSum + wildcards * 9 - antiWildcards * 2;

                            if (goal >= lowerBound && goal <= upperBound)
                            {
                                List<int> choices = new();
                                for (int i = 0; i < set.Count; i++)
                                {
                                    if ((setMask & (1 << i)) > 0)
                                    {
                                        choices.Add(set[i]);
                                    }
                                }

                                options.Add(choices);
                            }
                        }
                    }

                    if (options.Count > 0)
                    {
                        List<int> chosen = options[Randomizer.Instance?.Rng?.Next(options.Count) ?? 0];
                        chosen.Add(item.pips);

                        int wildcards = 0;
                        int antiWildcards = 0;
                        int areaSum = 0;
                        List<int> finalDice = new();
                        foreach (int pips in chosen)
                        {
                            items.Remove((pips, item.color));
                            dice[item.color].Remove(pips);

                            if (pips == 0)
                            {
                                wildcards++;
                            }
                            else if (pips == -10)
                            {
                                antiWildcards++;
                            }
                            else
                            {
                                areaSum += pips;
                                finalDice.Add(pips);
                            }
                        }

                        if (wildcards > 0 || antiWildcards > 0)
                        {
                            int difference = region.Count - areaSum;
                            for (int i = 0; i < wildcards; i++)
                            {
                                int lowerBound = Math.Clamp(difference - antiWildcards * 9 + (wildcards - i - 1), 1, 9);
                                int upperBound = Math.Clamp(difference - antiWildcards + (wildcards - i - 1) * 9, 1, 9);

                                int rolled = Randomizer.Instance?.Rng?.Next(lowerBound, upperBound + 1) ?? lowerBound;
                                difference -= rolled;
                                finalDice.Add(rolled);
                            }

                            for (int i = 0; i < antiWildcards; i++)
                            {
                                int lowerBound = Math.Clamp(difference - (antiWildcards - i - 1) * 9, -9, -1);
                                int upperBound = Math.Clamp(difference - (antiWildcards - i - 1), -9, -1);

                                int rolled = Randomizer.Instance?.Rng?.Next(lowerBound, upperBound + 1) ?? lowerBound;
                                difference -= rolled;
                                finalDice.Add(rolled);
                            }

                            if (difference != 0)
                            {
                                // Something went wrong.
                                return false;
                            }
                        }

                        foreach (int pips in finalDice)
                        {
                            Puzzle.Coord pos = openTiles[Randomizer.Instance?.Rng?.Next(openTiles.Count) ?? 0];
                            _puzzle.SetSymbol(pos.x, pos.y, Puzzle.GetDiceWithPips(pips), item.color);
                            openTiles.Remove(pos);
                        }

                        placed = regionIndex;
                        break;
                    }
                }

                if (placed == null)
                {
                    // Could not assign the symbol to a region.
                    return false;
                }
                else
                {
                    regions.RemoveAt(placed ?? 0);
                }
            }

            return true;
        }

        private bool PlaceFlowers()
        {
            Dictionary<int, List<Puzzle.Coord>> spots = new();
            foreach (int petals in _flowers.Keys)
            {
                spots[petals] = new();
            }

            foreach (Puzzle.Coord pos in _puzzle.OpenTiles)
            {
                int matching = CountAdjacentMatchingTiles(pos);
                if (_flowers.Keys.Contains(matching))
                {
                    spots[matching].Add(pos);
                }
            }

            foreach ((int petals, int amount) in _flowers)
            {
                if (spots[petals].Count < amount)
                {
                    return false;
                }

                for (int i = 0; i < amount; i++)
                {
                    Puzzle.Coord pos = spots[petals][Randomizer.Instance?.Rng?.Next(spots[petals].Count) ?? 0];
                    _puzzle.SetSymbol(pos.x, pos.y, Puzzle.GetFlowerWithPetals(petals), Puzzle.Color.Black);
                    spots[petals].Remove(pos);
                }
            }

            return true;
        }

        private bool PlaceWildcardFlowers()
        {
            int amount = _wildcardFlowers;

            while (amount > 0)
            {
                if (_puzzle.OpenTiles.Count == 0)
                {
                    return false;
                }

                Puzzle.Coord pos = _puzzle.OpenTiles[Randomizer.Instance?.Rng?.Next(_puzzle.OpenTiles.Count) ?? 0];
                int matching = CountAdjacentMatchingTiles(pos);
                _puzzle.SetSymbol(pos.x, pos.y, Puzzle.GetFlowerWithPetals(matching), Puzzle.Color.Black);
                amount--;
            }

            return true;
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
