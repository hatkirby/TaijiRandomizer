using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System.Text;

namespace TaijiRandomizer
{
    public class Randomizer : MelonMod
    {
        public const string kVersion = "0.1.0";

        private static Randomizer? _instance = null;

        public static Randomizer? Instance
        {
            get { return _instance; }
        }

        [HarmonyPatch(typeof(PuzzlePanelStartTile), "ToggleTile")]
        static class ToggleTilePatch
        {
            public static bool Prefix(PuzzlePanelStartTile __instance)
            {
                if (Randomizer.Instance != null) {
                    Randomizer.Instance?.LoggerInstance.Msg($"Panel {__instance.panelToControl.id} is {__instance.panelToControl.width}x{__instance.panelToControl.height}");
                }
                
                return true;
            }
        }

        public override void OnInitializeMelon()
        {
            _instance = this;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            Puzzle hello = new();
            hello.Load(46);
            hello.SetSymbol(0, 0, Puzzle.Symbol.OnePetal, Puzzle.Color.Black);
            hello.SetSymbol(1, 0, Puzzle.Symbol.Diamond, Puzzle.Color.PetalPurple);
            hello.SetSymbol(3, 0, Puzzle.Symbol.OnePip, Puzzle.Color.Gray);
            hello.SetSymbol(1, 2, Puzzle.Symbol.Diamond, Puzzle.Color.Black);
            hello.SetSymbol(0, 3, Puzzle.Symbol.OneAntiPip, Puzzle.Color.Gray);
            hello.SetSymbol(3, 3, Puzzle.Symbol.Diamond, Puzzle.Color.Black);
            hello.Save(46);
        }

        public override void OnDeinitializeMelon()
        {
            _instance = null;
        }
    }
}