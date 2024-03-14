using static TaijiRandomizer.Puzzle.Symbol;
using static TaijiRandomizer.Puzzle.Color;
using UnityEngine;

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
            //Tutorial (1x2)
            //Generator.SetSize(3, 3);
            Generator.Generate(26, Diamond, Teal, 4);
            Generator.Generate(255, Diamond, Teal, 4);
            //Generator.Generate(87, Diamond, Teal, 6);
            Generator.Generate(107, Diamond, Teal, 6);
            Generator.Generate(81, Diamond, Teal, 6);
            //Entry Set 2 (4x4)
            //Generator.SetSize(4, 3);
            //Generator.Generate(59, Diamond, Gold, 6);
            Generator.Generate(62, Diamond, Gold, 8);
            //Generator.SetSize(4, 4);
            Generator.Generate(253, Diamond, Gold, 8);
            Generator.Generate(224, Diamond, Gold, 10);
            Generator.Generate(347, Diamond, Gold, 10);
            //Boat (3x4)
            //Left Set 1 (4x4)
            Generator.Generate(5, Diamond, Gold, 10);
            Generator.Generate(17, Diamond, Gold, 12);
            Generator.Generate(73, Diamond, Gold, 12);
            //Left Set 2 (5x5)
            Generator.Generate(19, Diamond, Gold, 14);
            Generator.Generate(128, Diamond, Gold, 14);
            //Left Shortcut (5x4)
            Generator.Generate(76, Diamond, Gold, 12);
            //Right Set 1 (3x3)
            Generator.Generate(29, Diamond, Gold, 2, Diamond, Teal, 2, Diamond, Black, 2);
            Generator.Generate(116, Diamond, Gold, 4, Diamond, Teal, 2);
            Generator.Generate(448, Diamond, Gold, 4, Diamond, Teal, 4);
            Generator.Generate(458, Diamond, Gold, 6, Diamond, Teal, 2);
            Generator.Generate(209, Diamond, Gold, 4, Diamond, Teal, 2, Diamond, Black, 2);
            //Right Set 2 (4x4)
            Generator.Generate(411, Diamond, Gold, 6, Diamond, Black, 4);
            Generator.Generate(98, Diamond, Gold, 6, Diamond, Black, 6);
            Generator.Generate(63, Diamond, Gold, 8, Diamond, Black, 4);
            //Right Set 3 (5x5) Nerf to 5x4?
            Generator.Generate(140, Diamond, Gold, 6, Diamond, Black, 6, Diamond, Blue, 6);
            Generator.Generate(21, Diamond, Gold, 8, Diamond, Black, 8, Diamond, Blue, 6);
            //Right Shortcut
            Generator.Generate(462, Diamond, White, 6, Diamond, Gold, 4, Diamond, Purple, 4);
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
            //Flowers Tutorial 1 (3x3)
            //Flowers Tutorial 2 (3x3)
            //Flowers Set 1 (3x3)
            //Flowers Set 2 (4x4)
            Generator.Generate(233, Flower, Black, 4);
            Generator.Generate(100, Flower, Black, 5);
            Generator.Generate(531, Flower, Black, 6);
            Generator.Generate(526, Flower, Black, 8);
            Generator.Generate(538, Flower, Black, 10);
            //Flowers + Snake (5x5)
            //Shortcut to Hub (6x3)
            Generator.Generate(66, Flower, Black, 10);
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
