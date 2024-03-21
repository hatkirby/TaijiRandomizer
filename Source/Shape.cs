namespace TaijiRandomizer
{
    internal class Shape
    {
        public enum CellType
        {
            None,
            Boundary,
            Shape
        }

        private int _canvasWidth = 0;
        private int _canvasHeight = 0;
        private int _left = 0;
        private int _bottom = 0;
        private int _shapeWidth = 0;
        private int _shapeHeight = 0;
        private readonly List<CellType> _shape = new();
        private readonly List<Puzzle.Coord> _boundary = new();
        private Puzzle.Coord? _pivot = null;

        // This includes the boundary, so the actual shape width is two less than this.
        public int Width
        {
            get { return _shapeWidth; }
        }

        // This includes the boundary, so the actual shape height is two less than this.
        public int Height
        {
            get { return _shapeHeight; }
        }

        public CellType GetCell(int x, int y)
        {
            return _shape[x + _left + (y + _bottom) * _canvasWidth];
        }

        // This should be in shape coordinates; so, it includes the border, but not the full canvas.
        public Puzzle.Coord? Pivot
        {
            get { return _pivot; }
            set { _pivot = value; }
        }

        public void Generate(int maxWidth, int maxHeight, int minCount, int maxCount)
        {
            if (maxWidth <= 0)
            {
                throw new ArgumentOutOfRangeException("maxWidth");
            }
            else if (maxHeight <= 0)
            {
                throw new ArgumentOutOfRangeException("maxHeight");
            }
            else if (minCount <= 0)
            {
                throw new ArgumentOutOfRangeException("minCount");
            }
            else if (maxCount < minCount)
            {
                throw new ArgumentOutOfRangeException("maxCount");
            }

            _canvasWidth = maxWidth * 2 + 1;
            _canvasHeight = maxHeight * 2 + 1;

            _shape.Clear();
            for (int i = 0; i < _canvasWidth * _canvasHeight; i++)
            {
                _shape.Add(CellType.None);
            }

            _shapeWidth = 3;
            _shapeHeight = 3;
            _left = maxWidth - 1;
            _bottom = maxHeight - 1;

            _shape[maxWidth - 1 + maxHeight * _canvasWidth] = CellType.Boundary;
            _shape[maxWidth + (maxHeight - 1) * _canvasWidth] = CellType.Boundary;
            _shape[maxWidth + maxHeight * _canvasWidth] = CellType.Shape;
            _shape[maxWidth + 1 + maxHeight * _canvasWidth] = CellType.Boundary;
            _shape[maxWidth + (maxHeight + 1) * _canvasWidth] = CellType.Boundary;

            _boundary.Clear();
            _boundary.Add(new(maxWidth - 1, maxHeight));
            _boundary.Add(new(maxWidth + 1, maxHeight));
            _boundary.Add(new(maxWidth, maxHeight - 1));
            _boundary.Add(new(maxWidth, maxHeight + 1));

            int count = 1;

            int chosenCount = minCount;
            if (maxCount > minCount)
            {
                chosenCount = Randomizer.Instance?.Rng?.Next(minCount, maxCount + 1) ?? minCount;
            }

            while (count < chosenCount && _boundary.Count > 0)
            {
                Puzzle.Coord pos = _boundary[Randomizer.Instance?.Rng?.Next(_boundary.Count) ?? 0];
                _boundary.Remove(pos);
                _shape[pos.x + pos.y * _canvasWidth] = CellType.Shape;
                count++;

                // Mark the new boundaries.
                MarkBoundary(new(pos.x, pos.y - 1));
                MarkBoundary(new(pos.x, pos.y + 1));
                MarkBoundary(new(pos.x - 1, pos.y));
                MarkBoundary(new(pos.x + 1, pos.y));
            }
        }

        private void MarkBoundary(Puzzle.Coord boundPos)
        {
            if (boundPos.x < _left)
            {
                _left--;
                _shapeWidth++;
            }
            else if (boundPos.x >= _left + _shapeWidth)
            {
                _shapeWidth++;
            }
            else if (boundPos.y < _bottom)
            {
                _bottom--;
                _shapeHeight++;
            }
            else if (boundPos.y >= _bottom + _shapeHeight)
            {
                _shapeHeight++;
            }

            if (_shape[boundPos.x + boundPos.y * _canvasWidth] == CellType.None)
            {
                _shape[boundPos.x + boundPos.y * _canvasWidth] = CellType.Boundary;

                if (boundPos.x > 0 && boundPos.x < (_canvasWidth - 1) && boundPos.y > 0 && boundPos.y < (_canvasHeight - 1))
                {
                    // If this boundary cell is at the very edge of the canvas, mark it as boundary, but don't allow extending in that direction.
                    _boundary.Add(boundPos);
                }
            }
        }
    }
}
