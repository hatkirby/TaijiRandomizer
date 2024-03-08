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

            // We are leaving the first puzzle vanilla for now because of the alt solution.

            // Second floating island.
            tutorialGenerator = new(415, 0, 0, "3rdIsland/GraphicsRoot/StartingArea_HintPillarBase (1)", (float)-0.5, 1);
            tutorialGenerator.Generate();

            // Third floating island.
            tutorialGenerator = new(419, 1, 1, "4thIsland/NewGraphicsRoot/StartingArea_HintPillarBase (2)", (float)-0.5, 1);
            tutorialGenerator.Generate();

            // Fourth floating island.
            tutorialGenerator = new(428, 2, 3, "5thIsland/NewGraphicsRoot/StartingArea_HintPillarBase (3)", (float)-0.5, 1);
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

            // TODO: The final four.
        }
    }
}
