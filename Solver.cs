using Harmony;
using Il2Cpp;
using Il2CppSystem;
using Il2CppTMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TaijiRandomizer
{
    internal class Solver
    {
        private List<bool> _targetSolution = new();
        public List<List<bool>> solutions = new();
        private int _iterations = 0;
        private int _depthEnd = 0;
        private bool _solveFinished = false;
        private Puzzle _puzzle = new();
        private static Puzzle.Coord _up = new(0, -1), _down = new(0, 1), _left = new(-1, 0), _right = new(1, 0);
        private static Puzzle.Coord[] _directions = { _up, _down, _left, _right };
        private static Puzzle.Coord[] _directionsCheck = { _up, _left, new(0, 0) };
        private Puzzle.Coord _solvePos = new();

        public int minAttempts { get; set; } = 5;
        public int maxIterations { get; set; } = 100000000;
        public int maxQueue { get; set; } = 1000;
        public int maxSolutions { get; set; } = 1;
        public int maxLocksProportion { get; set; } = 1; //1 in X
        public static Solver Instance = new Solver();

        private void Clear()
        {
            for (int x = 0; x < _puzzle.Width; x++)
            {
                for (int y = 0; y < _puzzle.Height; y++)
                {
                    _puzzle.SetSolution(x, y, false);
                }
            }
        }

        public void SolveAndLockTiles(Puzzle puzzle)
        {
            _puzzle = puzzle;
            solutions = new();
            _solvePos = new(_puzzle.Width, _puzzle.Height);
            _solveFinished = false;
            solutions = new();
            _iterations = 0;
            _depthEnd = 0;
            _targetSolution = GetCurrentSolution();
            _solvePos = new(0, 0);
            Clear();
            int iterations = maxIterations;
            while (!_solveFinished && --iterations > 0)
            {
                SolveHelper();
                LockTiles();
            }
            Clear();
        }

        private void SolveHelper()
        {
            while (_iterations < maxIterations)
            {
                if (solutions.Count > maxQueue)
                {
                    return;
                }
                _puzzle.SetSolution(_solvePos.x, _solvePos.y, false);
                if (CheckSymbols(_solvePos))
                {
                    solutions.Add(GetCurrentSolution());
                }
                _puzzle.SetSolution(_solvePos.x, _solvePos.y, true);
                if (CheckSymbols(_solvePos))
                {
                    solutions.Add(GetCurrentSolution());
                }
                if (solutions.Count == 0)
                {
                    Randomizer.Instance?.LoggerInstance.Msg("No Solution Found");
                    _iterations = maxIterations + 1;
                    break;
                }
                if (_iterations >= _depthEnd)
                {
                    _solvePos = _solvePos + _right;
                    if (_solvePos.x >= _puzzle.Width)
                    {
                        _solvePos = new Puzzle.Coord(0, _solvePos.y + 1);
                        if (_solvePos.y >= _puzzle.Height)
                        {
                            break;
                        }
                    }
                    _depthEnd =_iterations + solutions.Count;
                }
                _iterations++;
                List<bool> next = solutions[0];
                solutions.RemoveAt(0);
                LoadSolution(next);
            }
            _solveFinished = true;
        }

        private void LockTiles()
        {
            int iterations = maxIterations;
            List<List<bool>> solutionSet = new(solutions);
            while (solutionSet.Count > maxSolutions - 1 && --iterations > 0)
            {
                List<bool> randomSol = solutionSet.ElementAt(Randomizer.RandomInt(solutionSet.Count));
                List<int> diffs = (from i in Enumerable.Range(0, randomSol.Count) where randomSol[i] != _targetSolution[i] select i).ToList();
                if (diffs.Count == 0)
                {
                    solutionSet = (List<List<bool>>)(from s in solutionSet where s != randomSol select s).ToList();
                }
                else
                {
                    int lockPoint = diffs[Randomizer.RandomInt(diffs.Count)];
                    _puzzle.LockTile(lockPoint, _targetSolution[lockPoint]);
                    solutionSet = (List<List<bool>>)(from s in solutionSet where s.Count > lockPoint && s[lockPoint] == _targetSolution[lockPoint] select s).ToList();
                }
                if (!_solveFinished)
                {
                    solutions = solutionSet;
                    return;
                }
            }
        }

        public void CleanupLocks(Puzzle puzzle)
        {
            List<int> locks = (from i in Enumerable.Range(0, _targetSolution.Count) where puzzle.IsLocked(i % puzzle.Width, i / puzzle.Width) select i).ToList();
            if (_targetSolution.Count < puzzle.Width * puzzle.Height)
            {
                locks.Clear();
            }
            int remainingLocks = locks.Count;
            while (locks.Count > 0)
            {
                int l = locks[Randomizer.RandomInt(locks.Count)];
                List<bool> altSolution = new(_targetSolution);
                altSolution[l] = !_targetSolution[l];
                if ((false && solutions.Any(s => s.SequenceEqual(altSolution))) ||
                    solutions.All(s => s[l] == _targetSolution[l]))
                {
                    puzzle.UnlockTile(l);
                    remainingLocks--;
                }
                locks.Remove(l);
                if (remainingLocks == 1)
                {
                    for (int i = 0; i < _targetSolution.Count; i++)
                    {
                        puzzle.UnlockTile(i);
                    }
                }
            }
            locks.Clear();
        }

        private bool IsOnGrid(Puzzle.Coord pos)
        {
            return !(pos.x < 0 || pos.x >= _puzzle.Width || pos.y < 0 || pos.y >= _puzzle.Height || _puzzle.IsDisabled(pos.x, pos.y));
        }

        private bool IsTileSolvedYet(int x, int y)
        {
            return y < _solvePos.y || y == _solvePos.y && x <= _solvePos.x;
        }

        private List<bool> GetCurrentSolution()
        {
            List<bool> solution = new(); 
            for (int y = 0; y < _puzzle.Height; y++)
            {
                for (int x = 0; x < _puzzle.Width; x++)
                {
                    if (!(IsTileSolvedYet(x, y)))
                    {
                        return solution;
                    }
                    solution.Add(_puzzle.IsInSolution(x, y));
                }
            }
            return solution;
        }
        private List<bool> LoadSolution(List<bool> solution)
        {
            foreach (int i in Enumerable.Range(0, solution.Count))
            {
                _puzzle.SetSolution(i % _puzzle.Width, i / _puzzle.Width, solution[i]);
            }
            return solution;
        }

        private List<Puzzle.Coord> GetRegion(Puzzle.Coord pos, out bool enclosed)
        {
            enclosed = true;

            bool regionState = _puzzle.IsInSolution(pos.x, pos.y);

            HashSet<Puzzle.Coord> visited = new();
            List<Puzzle.Coord> result = new();

            Queue<Puzzle.Coord> queue = new();
            queue.Enqueue(pos);

            Puzzle.Coord next;
            while (queue.TryDequeue(out next))
            {
                if (!IsTileSolvedYet(next.x, next.y))
                {
                    enclosed = false;
                    continue;
                }
                if (visited.Contains(next))
                {
                    continue;
                }

                visited.Add(next);

                if (!IsOnGrid(next) || _puzzle.IsInSolution(next.x, next.y) != regionState)
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

        private bool CheckSymbols(Puzzle.Coord pos)
        {
            if (!IsOnGrid(pos))
            {
                return true;
            }
            foreach (Puzzle.Coord dir in _directionsCheck)
            {
                Puzzle.Coord next = pos + dir;
                if (!IsOnGrid(next))
                {
                    continue;
                }
                Puzzle.Symbol symbol = _puzzle.GetSymbol(next.x, next.y);
                if (Puzzle.IsFlower(symbol) && !CheckFlower(next))
                {
                    return false;
                }
            }
            if (!CheckRegion(pos))
            {
                return false;
            }
            if (pos.x > 0 && _puzzle.IsInSolution(pos.x, pos.y) != _puzzle.IsInSolution(pos.x - 1, pos.y) && !CheckRegion(pos + _left))
            {
                return false;
            }
            if (pos.y > 0 && _puzzle.IsInSolution(pos.x, pos.y) != _puzzle.IsInSolution(pos.x, pos.y - 1) && !CheckRegion(pos + _up))
            {
                return false;
            }
            return true;
        }

        private bool CheckRegion(Puzzle.Coord pos)
        {
            bool enclosed;
            List<Puzzle.Coord> region = GetRegion(pos, out enclosed);
            HashSet<Puzzle.Color> diamondColors = new();
            List<Puzzle.Color> allColors = new();
            Puzzle.Color? diceColor = null;
            int diceTotal = 0;
            int negDiceTotal = 0;
            foreach (Puzzle.Coord p in region)
            {
                Puzzle.Symbol symbol = _puzzle.GetSymbol(p.x, p.y);
                if (symbol == Puzzle.Symbol.None)
                {
                    continue;
                }
                Puzzle.Color color = _puzzle.GetColor(p.x, p.y);
                allColors.Add(color);
                if (symbol == Puzzle.Symbol.Diamond)
                {
                    diamondColors.Add(color);
                }
                if (diamondColors.Contains(color) && (from c in allColors where c == color select c).Count() > 2)
                {
                    return false;
                }
                if (symbol == Puzzle.Symbol.Dice)
                {
                    if ((diceColor ?? color) != color)
                    {
                        return false;
                    }
                    diceColor = color;
                    int pips = Puzzle.CountDicePips(symbol);
                    if (pips > 0)
                    {
                        diceTotal += pips;
                        if (enclosed && region.Count > diceTotal)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        negDiceTotal += pips;
                    }
                }
            }
            if (enclosed)
            {
                foreach (Puzzle.Color color in diamondColors)
                {
                    if ((from c in allColors where c == color select c).Count() != 2)
                    {
                        return false;
                    }
                }
                if (diceTotal > 0 && enclosed && region.Count != diceTotal)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckFlower(Puzzle.Coord pos)
        {
            int petals = Puzzle.CountFlowerPetals(_puzzle.GetSymbol(pos.x, pos.y));
            int countMatch = 0;
            int countNonMatch = 0;
            int openSpaces = 4;
            foreach (Puzzle.Coord dir in _directions)
            {
                Puzzle.Coord next = pos + dir;
                if (!IsOnGrid(next))
                {
                    openSpaces--;
                    continue;
                }
                if (!IsTileSolvedYet(next.x, next.y))
                {
                    continue;
                }
                if (_puzzle.IsInSolution(next.x, next.y) == _puzzle.IsInSolution(pos.x, pos.y))
                {
                    countMatch++;
                }
                else
                {
                    countNonMatch++;
                }
            }
            if (countMatch > petals || countNonMatch > openSpaces - petals)
            {
                return false;
            }
            return true;
        }
    }
}
