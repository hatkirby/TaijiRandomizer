using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaijiRandomizer
{
    internal class PanelList
    {
        public static void Generate()
        {
            Generator generator = new(26);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Teal, 2);
            generator.Generate();

            generator = new(255);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Teal, 2);
            generator.SetLocks(3);
            generator.Generate();

            generator = new(107);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Teal, 4);
            generator.SetLocks(2);
            generator.Generate();

            generator = new(81);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Teal, 4);
            generator.SetLocks(2);
            generator.Generate();

            generator = new(59);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 6);
            generator.SetLocks(2);
            generator.Generate();

            generator = new(62);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 6);
            generator.Generate();

            generator = new(253);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 6);
            generator.SetLocks(3);
            generator.Generate();

            generator = new(347);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 8);
            generator.Generate();

            generator = new(110);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Black, 10);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.White, 4);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 2);
            generator.SetLocks(7);
            generator.Generate();


            // These two are broken.
            generator = new(19);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 16);
            generator.SetLocks(4);
            generator.Generate();

            generator = new(128);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 16);
            generator.SetLocks(4);
            generator.Generate();
        }
    }
}
