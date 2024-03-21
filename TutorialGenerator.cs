using UnityEngine;

namespace TaijiRandomizer
{
    internal class TutorialGenerator
    {
        private static GameObject? _templateWhiteBlock = null;
        private static GameObject? _templateBlackBlock = null;

        public static void Initialize()
        {
            _templateWhiteBlock = GameObject.Instantiate(GameObject.Find("StartingArea_HintPillarBase (7)/StartingArea_HintBlocks_0 (20)"));
            _templateWhiteBlock.hideFlags = HideFlags.HideAndDontSave;
            _templateWhiteBlock.active = false;

            _templateBlackBlock = GameObject.Instantiate(GameObject.Find("StartingArea_HintPillarBase (7)/StartingArea_HintBlocks_0 (25)"));
            _templateBlackBlock.hideFlags = HideFlags.HideAndDontSave;
            _templateBlackBlock.active = false;
        }

        private readonly uint _id;
        private readonly int _minGaps;
        private readonly int _maxGaps;
        private readonly string _path;
        private readonly float _firstX;
        private readonly float _firstY;
        private readonly List<(int x, int y, bool on, bool locked)> _locked;

        private readonly Puzzle _puzzle = new();

        private Canvas? _canvas = null;

        public TutorialGenerator(uint id, int minGaps, int maxGaps, string path, float firstX, float firstY)
        {
            _id = id;
            _minGaps = minGaps;
            _maxGaps = maxGaps;
            _path = path;
            _firstX = firstX;
            _firstY = firstY;
            _locked = new();
            _puzzle.Load(_id);
        }

        public void ForceCell(int x, int y, bool on)
        {
            _locked.Add(new() { x = x, y = y, on = on, locked = false });
        }

        public void LockCell(int x, int y, bool on)
        {
            _locked.Add(new() { x = x, y = y, on = on, locked = true });
        }

        public void Generate()
        {
            while (!GenerateHelper())
            {
                // Try again.
            }

            _puzzle.Save(_id);
            _puzzle.WriteSolution(_id);

            // Deactivate the blocks that are already there.
            GameObject pillarBase = Randomizer.Instance.LookupGameObject(_path);
            for (int i=0; i < pillarBase.transform.childCount; i++)
            {
                GameObject child = pillarBase.transform.GetChild(i).gameObject;

                if (child.name.StartsWith("StartingArea_HintBlocks_0") || child.name.StartsWith("TutorialBlock_"))
                {
                    GameObject.Destroy(child);
                }
            }

            if (_templateBlackBlock == null || _templateWhiteBlock == null)
            {
                Randomizer.Instance?.LoggerInstance.Msg("Could not generate tutorial puzzle because block templates are not initialized.");
                return;
            }

            // Instantiate blocks for the puzzle.
            for (int y=0; y<_puzzle.Height;y++)
            {
                for (int x=0; x<_puzzle.Width;x++)
                {
                    Canvas.CellType cellType = _canvas.GetCell(x, y);
                    if (cellType == Canvas.CellType.Undecided)
                    {
                        continue;
                    }

                    GameObject templateToUse;
                    if (cellType == Canvas.CellType.Off)
                    {
                        templateToUse = _templateBlackBlock;
                    } else
                    {
                        templateToUse = _templateWhiteBlock;
                    }

                    GameObject newBlock = GameObject.Instantiate(templateToUse);
                    newBlock.name = $"TutorialBlock_{_id}_{x}_{y}";
                    newBlock.transform.parent = pillarBase.transform;
                    newBlock.transform.set_localPosition_Injected(new(_firstX + x, _firstY + y, 0));
                    newBlock.active = true;
                }
            }
        }

        private bool GenerateHelper()
        {
            _puzzle.Clear();

            _canvas = new(_puzzle);

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
                        _canvas.SetCell(x, y, Canvas.CellType.On);
                    } else
                    {
                        _canvas.SetCell(x, y, Canvas.CellType.Off);
                    }
                }
            }

            // Place any locked cells.
            foreach (var locked in _locked)
            {
                _puzzle.SetSolution(locked.x, locked.y, locked.on);
                _canvas.SetCell(locked.x, locked.y, locked.on ? Canvas.CellType.On : Canvas.CellType.Off);

                if (locked.locked)
                {
                    _puzzle.LockTile(locked.x, locked.y);
                }
            }

            if (!RemoveDeducibleCells())
            {
                return false;
            }

            return true;
        }

        private static (int total, int on, int off) CountNeighbors(Canvas canvas, int x, int y)
        {
            int neighbors = 0;
            int on = 0;
            int off = 0;

            if (x > 0)
            {
                neighbors++;

                Canvas.CellType cellType = canvas.GetCell(x - 1, y);
                if (cellType == Canvas.CellType.On)
                {
                    on++;
                } else if (cellType == Canvas.CellType.Off)
                {
                    off++;
                }
            }

            if (x < (canvas.Width - 1))
            {
                neighbors++;

                Canvas.CellType cellType = canvas.GetCell(x + 1, y);
                if (cellType == Canvas.CellType.On)
                {
                    on++;
                }
                else if (cellType == Canvas.CellType.Off)
                {
                    off++;
                }
            }

            if (y > 0)
            {
                neighbors++;

                Canvas.CellType cellType = canvas.GetCell(x, y - 1);
                if (cellType == Canvas.CellType.On)
                {
                    on++;
                }
                else if (cellType == Canvas.CellType.Off)
                {
                    off++;
                }
            }

            if (y < (canvas.Height - 1))
            {
                neighbors++;

                Canvas.CellType cellType = canvas.GetCell(x, y + 1);
                if (cellType == Canvas.CellType.On)
                {
                    on++;
                }
                else if (cellType == Canvas.CellType.Off)
                {
                    off++;
                }
            }

            return (neighbors, on, off);
        }

        private bool RemoveDeducibleCells()
        {
            if (_minGaps == 0)
            {
                return true;
            }

            // Incrementally remove tiles as long as they can be deduced.
            int gaps = 0;
            int notRemoved = 0;
            int finishThreshold = _puzzle.Width * _puzzle.Height * 2;
            while (gaps < _maxGaps)
            {
                notRemoved++;

                int x = Randomizer.Instance?.Rng?.Next(_puzzle.Width) ?? 0;
                int y = Randomizer.Instance?.Rng?.Next(_puzzle.Height) ?? 0;

                Canvas.CellType cellType = _canvas.GetCell(x, y);
                if (cellType == Canvas.CellType.Undecided)
                {
                    continue;
                }

                var neighbors = CountNeighbors(_canvas, x, y);

                // A cell can be deduced if at least half of its neighbors are the opposite state and the same is not true of the same state.
                int threshold = neighbors.total / 2;
                if ((cellType == Canvas.CellType.Off && neighbors.on >= threshold && neighbors.off < threshold) || (cellType == Canvas.CellType.On && neighbors.off >= threshold && neighbors.on < threshold))
                {
                    gaps++;
                    notRemoved = 0;

                    _canvas.SetCell(x, y, Canvas.CellType.Undecided);

                    // If we are over the maximum number of gaps, we are done.
                    if (gaps >= _maxGaps)
                    {
                        return true;
                    }

                    // As long as we have hit the minimum number of gaps, we have a 50% chance of quitting after every removal.
                    if (gaps >= _minGaps && Randomizer.Instance?.Rng?.Next(2) == 0)
                    {
                        return true;
                    }
                }

                if (notRemoved > finishThreshold)
                {
                    // No tiles were removed for a while, which likely means no more will be. This can result in under the minimum number of tiles being removed.
                    return (gaps >= _minGaps);
                }
            }

            return true;
        }
    }
}
