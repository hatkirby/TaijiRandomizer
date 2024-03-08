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




            generator = new(468);
            generator.Add(Puzzle.Symbol.Bar, Puzzle.Color.Black, 3);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Black, 3);
            generator.Add(Puzzle.Symbol.Diamond, Puzzle.Color.White, 2);
            generator.SetWildcardFlowers(1);
            generator.SetLocks(4);
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



            GenerateTutorial();
        }

        private static void GenerateTutorial()
        {
            TutorialGenerator tutorialGenerator;

            // First puzzle. The alternate solution remains the same. Ensure that the normal solution is not the alt.
            tutorialGenerator = new(408, 0, 0, "2ndIsland/GraphicsRoot/StartingArea_HintPillarBase", (float)-0.5, 1);
            tutorialGenerator.ForceCell(0, Randomizer.Instance?.Rng?.Next(2) ?? 0, false);
            tutorialGenerator.Generate();

            // Second floating island.
            tutorialGenerator = new(415, 1, 1, "3rdIsland/GraphicsRoot/StartingArea_HintPillarBase (1)", (float)-0.5, 1);
            tutorialGenerator.Generate();

            // Third floating island.
            tutorialGenerator = new(419, 2, 3, "4thIsland/NewGraphicsRoot/StartingArea_HintPillarBase (2)", (float)-0.5, 1);
            tutorialGenerator.Generate();

            // Fourth floating island.
            tutorialGenerator = new(428, 3, 4, "5thIsland/NewGraphicsRoot/StartingArea_HintPillarBase (3)", (float)-0.5, 1);
            tutorialGenerator.Generate();

            // First mainland.
            tutorialGenerator = new(380, 4, 5, "Mainland/GraphicsRoot/StartingArea_HintPillarBase (4)", (float)-1.5, 1);
            tutorialGenerator.Generate();

            // Second mainland.
            tutorialGenerator = new(381, 5, 6, "Mainland/GraphicsRoot/StartingArea_HintPillarBase (11)", (float)-1.5, 1);
            tutorialGenerator.Generate();

            // Optional mainland.
            tutorialGenerator = new(397, 8, 9, "Mainland/GraphicsRoot/StartingArea_HintPillarBase (7)", (float)-1, 1);
            tutorialGenerator.Generate();

            // The final four.
            uint[] tutorialEndIds = { 163, 436, 442, 449 };
            List<uint> orderedIds = tutorialEndIds.OrderBy(_ => Randomizer.Instance?.Rng?.Next()).ToList();

            List<Puzzle.Coord> coords = new();
            for (int y=0;y<3;y++)
            {
                for (int x=0;x<3;x++)
                {
                    coords.Add(new(x, y));
                }
            }

            Puzzle.Coord coord1 = coords[Randomizer.Instance?.Rng?.Next(coords.Count) ?? 0];
            coords.Remove(coord1);
            Puzzle.Coord coord2 = coords[Randomizer.Instance?.Rng?.Next(coords.Count) ?? 0];
            coords.Remove(coord2);
            Puzzle.Coord coord3 = coords[Randomizer.Instance?.Rng?.Next(coords.Count) ?? 0];
            coords.Remove(coord3);
            Puzzle.Coord coord4 = coords[Randomizer.Instance?.Rng?.Next(coords.Count) ?? 0];

            List<TutorialGenerator> generators = new() {
                new(orderedIds[0], 3, 5, "StartingArea_AboveLowerWorldPath_FadeGroup/StartingArea_HintPillarBase (9)", (float)-1, 1),
                new(orderedIds[1], 3, 5, "StartingArea_AboveLowerWorldPath_FadeGroup/StartingArea_HintPillarBase (7)", (float)-1, 1),
                new(orderedIds[2], 3, 5, "StartingArea_AboveLowerWorldPath_FadeGroup/StartingArea_HintPillarBase (8)", (float)-1, 1),
                new(orderedIds[3], 3, 5, "StartingArea_AboveLowerWorldPath_FadeGroup/StartingArea_HintPillarBase (10)", (float)-1, 1),
            };

            List<TutorialGenerator> orderedGenerators = generators.OrderBy(_ => Randomizer.Instance?.Rng?.Next()).ToList();

            bool cell1 = Randomizer.Instance?.Rng?.Next(2) == 0;
            bool cell2 = Randomizer.Instance?.Rng?.Next(2) == 0;
            bool cell3 = Randomizer.Instance?.Rng?.Next(2) == 0;
            bool cell4 = Randomizer.Instance?.Rng?.Next(2) == 0;

            orderedGenerators[0].LockCell(coord1.x, coord1.y, cell1);
            orderedGenerators[0].LockCell(coord2.x, coord2.y, !cell2);
            orderedGenerators[0].Generate();
            
            orderedGenerators[1].LockCell(coord2.x, coord2.y, cell2);
            orderedGenerators[1].LockCell(coord3.x, coord3.y, !cell3);
            orderedGenerators[1].Generate();
            
            orderedGenerators[2].LockCell(coord3.x, coord3.y, cell3);
            orderedGenerators[2].LockCell(coord4.x, coord4.y, !cell4);
            orderedGenerators[2].Generate();

            orderedGenerators[3].LockCell(coord4.x, coord4.y, cell4);
            orderedGenerators[3].LockCell(coord1.x, coord1.y, !cell1);
            orderedGenerators[3].Generate();
        }
    }
}
