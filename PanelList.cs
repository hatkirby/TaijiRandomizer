using static TaijiRandomizer.Puzzle.Symbol;
using static TaijiRandomizer.Puzzle.Color;

namespace TaijiRandomizer
{
    internal class PanelList {
        public static void Generate() {
            GenerateTutorial();
            GenerateMines();
            GenerateGardens();
        }

        public static void Generate_Test()
        {
            
        }

        public static void GenerateMines()
        {
            Generator generator;
            //Tutorial (1x2)
            //Entry Set 1 (3x3)
            generator = new(26);
            generator.Add(Diamond, Teal, 4);
            generator.Generate();
            generator = new(255);
            generator.Add(Diamond, Teal, 4);
            generator.Generate();
            //generator = new(87);
            //generator.Add(Diamond, Teal, 4);
            //generator.Generate();
            generator = new(107);
            generator.Add(Diamond, Teal, 6);
            generator.Generate();
            generator = new(81);
            generator.Add(Diamond, Teal, 6);
            generator.Generate();
            //Entry Set 2 (4x4)
            //generator = new(59);
            //generator.Add(Diamond, Gold, 6);
            //generator.Generate();
            generator = new(62);
            generator.Add(Diamond, Gold, 8);
            generator.Generate();
            generator = new(253);
            generator.Add(Diamond, Gold, 8);
            generator.Generate();
            generator = new(224);
            generator.Add(Diamond, Gold, 10);
            generator.Generate();
            generator = new(347);
            generator.Add(Diamond, Gold, 10);
            generator.Generate();
            //Boat (3x4)
            //Left Set 1 (4x4)
            generator = new(5);
            generator.Add(Diamond, Gold, 10);
            generator.Generate();
            generator = new(17);
            generator.Add(Diamond, Gold, 12);
            generator.Generate();
            generator = new(73);
            generator.Add(Diamond, Gold, 12);
            generator.Generate();
            //Left Set 2 (5x5) Don't generate properly?
            generator = new(19);
            generator.Add(Diamond, Gold, 14);
            generator.Generate();
            generator = new(128);
            generator.Add(Diamond, Gold, 14);
            generator.Generate();
            //Left Shortcut (5x4)
            generator = new(76);
            generator.Add(Diamond, Gold, 12);
            generator.Generate();
            //Right Set 1 (3x3)
            generator = new(29);
            generator.Add(Diamond, Gold, 2);
            generator.Add(Diamond, Teal, 2);
            generator.Add(Diamond, Black, 2);
            generator.Generate();
            generator = new(116);
            generator.Add(Diamond, Gold, 4);
            generator.Add(Diamond, Teal, 2);
            generator.Generate();
            generator = new(448);
            generator.Add(Diamond, Gold, 4);
            generator.Add(Diamond, Teal, 4);
            generator.Generate();
            generator = new(458);
            generator.Add(Diamond, Gold, 6);
            generator.Add(Diamond, Teal, 2);
            generator.Generate();
            generator = new(209);
            generator.Add(Diamond, Gold, 4);
            generator.Add(Diamond, Teal, 2);
            generator.Add(Diamond, Black, 2);
            generator.Generate();
            //Right Set 2 (4x4)
            generator = new(411);
            generator.Add(Diamond, Gold, 6);
            generator.Add(Diamond, Black, 4);
            generator.Generate();
            generator = new(98);
            generator.Add(Diamond, Gold, 6);
            generator.Add(Diamond, Black, 6);
            generator.Generate();
            generator = new(63);
            generator.Add(Diamond, Gold, 8);
            generator.Add(Diamond, Black, 6);
            generator.Generate();
            //Right Set 3 (5x5) Nerf to 5x4?
            generator = new(140);
            generator.Add(Diamond, Gold, 6);
            generator.Add(Diamond, Black, 6);
            generator.Add(Diamond, Blue, 6);
            generator.Generate();
            generator = new(21);
            generator.Add(Diamond, Gold, 8);
            generator.Add(Diamond, Black, 8);
            generator.Add(Diamond, Blue, 6);
            generator.Generate();
            //Right Shortcut
            generator = new(462);
            generator.Add(Diamond, White, 6);
            generator.Add(Diamond, Gold, 4);
            generator.Add(Diamond, Purple, 4);
            generator.Generate();
            //Snake Set
            //Snake Shortcut
            //Final Puzzle
            //Dice Mixin Elevator (3x3)
            //Dice Mixin F1 Entry (5x5)
            //Dice Mixin Set 1 (4x4)
            //Dice Mixin Set 2 (4x4)
            //Dice Mixin F3 Entry (4x4)
            //River
        }

        public static void GenerateGardens()
        {
            Generator generator;
            //Flowers Tutorial 1 (3x3)
            //Flowers Tutorial 2 (3x3)
            //Flowers Set 1 (3x3)
            //Flowers Set 2 (4x4)
            generator = new(233);
            generator.SetWildcardFlowers(4);
            generator.Generate();
            generator = new(100);
            generator.SetWildcardFlowers(5);
            generator.Generate();
            generator = new(531);
            generator.SetWildcardFlowers(6);
            generator.Generate();
            generator = new(526);
            generator.SetWildcardFlowers(8);
            generator.Generate();
            generator = new(538);
            generator.SetWildcardFlowers(10);
            generator.Generate();
            //Flowers + Snake (5x5)
            //Shortcut to Hub (6x3)
            generator = new(66);
            generator.SetWildcardFlowers(10);
            generator.Generate();
            //Shortcut to Orchard (4x4)

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
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
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
