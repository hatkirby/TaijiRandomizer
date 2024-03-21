namespace TaijiRandomizer
{
    internal class Canvas
    {
        public enum CellType
        {
            Undecided,
            Off,
            On,
            Disabled
        }

        public enum Decision
        {
            No,
            YesOff,
            YesOn,
            YesEither
        }

        class Cell
        {
            public CellType type;
            public bool occupied;
        }

        private readonly int _width;
        private readonly int _height;
        private readonly List<Cell> _cells;

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        public CellType GetCell(int x, int y)
        {
            return _cells[x + y * _width].type;
        }

        public void SetCell(int x, int y, CellType cellType)
        {
            _cells[x + y * _width].type = cellType;
        }

        public Canvas(Puzzle puzzle)
        {
            _width = puzzle.Width;
            _height = puzzle.Height;

            // TODO: Handle pre-locked cells.
            _cells = new(_width * _height);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _cells.Add(new()
                    {
                        type = puzzle.IsDisabled(x, y) ? CellType.Disabled : CellType.Undecided,
                        occupied = false
                    });
                }
            }
        }

        public Decision CanPlaceShape(int x, int y, Shape shape)
        {
            Decision decision = Decision.YesEither;

            // If the pivot has been placed, check if the position that the pivot would be placed in is open.
            if (shape.Pivot != null && _cells[x - 1 + (shape.Pivot?.x ?? 0) + (y - 1 + (shape.Pivot?.y ?? 0)) * _width].occupied)
            {
                return Decision.No;
            }

            for (int sy = 0; sy < shape.Height; sy++)
            {
                // The outer border of the shape is just boundary, so we offset
                // the position by one to account for this. If this places the
                // shape border off-screen, we just ignore the boundary.
                int cy = y - 1 + sy;
                if (cy < 0 || cy >= _height)
                {
                    continue;
                }

                for (int sx = 0; sx < shape.Width; sx++)
                {
                    int cx = x - 1 + sx;
                    if (cx < 0 || cx >= _width)
                    {
                        continue;
                    }

                    CellType canvasCell = _cells[cx + cy * _width].type;
                    Shape.CellType shapeCell = shape.GetCell(sx, sy);

                    // If this would involve placing the shape into a disabled cell, it's a no-go.
                    if (canvasCell == CellType.Disabled && shapeCell == Shape.CellType.Shape)
                    {
                        return Decision.No;
                    }

                    // If the canvas cell is empty or there's no shape at this position, we're fine.
                    if (canvasCell == CellType.Undecided || canvasCell == CellType.Disabled || shapeCell == Shape.CellType.None)
                    {
                        continue;
                    }

                    // This canvas cell is decided, and we also need to place a part of the shape here.
                    // If this is the shape's boundary, we know the shape can only be placed if it has
                    // the opposite state as the cell that's present. If this is part of the shape, then
                    // it defines the actual shape state.
                    CellType expectedType;
                    if (shapeCell == Shape.CellType.Boundary)
                    {
                        if (canvasCell == CellType.Off)
                        {
                            expectedType = CellType.On;
                        }
                        else
                        {
                            expectedType = CellType.Off;
                        }
                    }
                    else
                    {
                        expectedType = canvasCell;
                    }

                    // This helps us narrow down whether the shape can be placed here.
                    if (decision == Decision.YesEither)
                    {
                        if (expectedType == CellType.Off)
                        {
                            decision = Decision.YesOff;
                        }
                        else
                        {
                            decision = Decision.YesOn;
                        }
                    }
                    else if (decision == Decision.YesOff)
                    {
                        if (expectedType != CellType.Off)
                        {
                            return Decision.No;
                        }
                    }
                    else if (decision == Decision.YesOn)
                    {
                        if (expectedType != CellType.On)
                        {
                            return Decision.No;
                        }
                    }
                }
            }

            return decision;
        }

        public void PlaceShape(int x, int y, Shape shape, bool on)
        {
            for (int sy = 0; sy < shape.Height; sy++)
            {
                // The outer border of the shape is just boundary, so we offset
                // the position by one to account for this. If this places the
                // shape border off-screen, we just ignore the boundary.
                int cy = y - 1 + sy;
                if (cy < 0 || cy >= _height)
                {
                    continue;
                }

                for (int sx = 0; sx < shape.Width; sx++)
                {
                    int cx = x - 1 + sx;
                    if (cx < 0 || cx >= _width)
                    {
                        continue;
                    }

                    Shape.CellType shapeCell = shape.GetCell(sx, sy);
                    if (on)
                    {
                        if (shapeCell == Shape.CellType.Boundary)
                        {
                            _cells[cx + cy * _width].type = CellType.Off;
                        }
                        else if (shapeCell == Shape.CellType.Shape)
                        {
                            _cells[cx + cy * _width].type = CellType.On;
                        }
                    }
                    else
                    {
                        if (shapeCell == Shape.CellType.Boundary)
                        {
                            _cells[cx + cy * _width].type = CellType.On;
                        }
                        else if (shapeCell == Shape.CellType.Shape)
                        {
                            _cells[cx + cy * _width].type = CellType.Off;
                        }
                    }
                }
            }

            if (shape.Pivot != null)
            {
                _cells[x - 1 + (shape.Pivot?.x ?? 0) + (y - 1 + (shape.Pivot?.y ?? 0)) * _width].occupied = true;
            }
        }

        // The results are in the shape's coordinates, not the canvas's.
        public List<Puzzle.Coord> GetOpenCellsInShape(int x, int y, Shape shape)
        {
            List<Puzzle.Coord> result = new();

            for (int sy = 1; sy < shape.Height - 1; sy++)
            {
                int cy = y - 1 + sy;

                for (int sx = 1; sx < shape.Width - 1; sx++)
                {
                    int cx = x - 1 + sx;

                    if (shape.GetCell(sx, sy) == Shape.CellType.Shape && !_cells[cx + cy * _width].occupied)
                    {
                        result.Add(new(sx, sy));
                    }
                }
            }

            return result;
        }
    }
}
