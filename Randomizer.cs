using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using Il2CppTMPro;
using MelonLoader;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

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

        private Random? _rng = null;

        public Random? Rng
        {
            get { return _rng; }
        }

        private GameObject? _templateWhiteBlock = null;
        private GameObject? _templateBlackBlock = null;

        public GameObject? TemplateWhiteBlock { get { return _templateWhiteBlock; } }

        public GameObject? TemplateBlackBlock { get { return _templateBlackBlock; } }

        private GameObject? _templateMenuTextInput = null;

        internal delegate void PuzzlePanelInitializer(PuzzlePanel panel);

        private Dictionary<uint, PuzzlePanelInitializer> _puzzlePanelInitializers = new();

        internal void SetPuzzlePanelInitializer(uint id, PuzzlePanelInitializer initializer)
        {
            _puzzlePanelInitializers[id] = initializer;
        }

        private Dictionary<string, GameObject> _gameObjectCache = new();

        public GameObject LookupGameObject(string path)
        {
            if (!_gameObjectCache.ContainsKey(path))
            {
                _gameObjectCache[path] = GameObject.Find(path);
            }

            return _gameObjectCache[path];
        }

        private bool _setSeed = false;
        private int _setSeedValue = 0;

        [HarmonyPatch(typeof(PuzzlePanel), nameof(PuzzlePanel.Update))]
        static class UpdatePanelPatch
        {
            public static void Postfix(PuzzlePanel __instance)
            {
                // Here we can run a handler right after a puzzle has been properly initialized.
                if (__instance.isInitialized && _instance != null && _instance._puzzlePanelInitializers.ContainsKey(__instance.id))
                {
                    _instance._puzzlePanelInitializers[__instance.id](__instance);
                    _instance._puzzlePanelInitializers.Remove(__instance.id);
                }
            }
        }

        [HarmonyPatch(typeof(PuzzlePanelStartTile), "ToggleTile")]
        static class ToggleTilePatch
        {
            public static void Prefix(PuzzlePanelStartTile __instance)
            {
                if (Randomizer.Instance != null)
                {
                    Randomizer.Instance?.LoggerInstance.Msg($"Panel {__instance.panelToControl.id} is {__instance.panelToControl.width}x{__instance.panelToControl.height}");
                }
            }
        }

        [HarmonyPatch(typeof(PuzzlePanel), "stepOn")]
        static class SolvePuzzlePatch
        {
            private static HashSet<uint> _checked = new();

            public static void Prefix(PuzzlePanel __instance)
            {
                if (Randomizer.Instance != null && !_checked.Contains(__instance.id))
                {
                    Randomizer.Instance?.LoggerInstance.Msg($"Panel {__instance.id} is {__instance.width}x{__instance.height}");
                    _checked.Add(__instance.id);
                }
            }
        }

        [HarmonyPatch(typeof(PauseMenu), "InitializeMenus")]
        static class InitializeMenuPatch
        {
            public static PauseMenu? pauseMenu = null;
            public static PauseMenu.SubMenu randomizerMenu;

            public delegate int GetChoiceFunction();
            public delegate void SetChoiceFunction(int choice);

            public delegate string GetStringFunction();
            public delegate void SetStringFunction(string value);

            public static PauseMenu.MenuItem CreateMenuItem(string text)
            {
                GameObject menuDisableGroup = pauseMenu.menuDisableGroup;
                Transform transform = menuDisableGroup.transform;

                GameObject menuObject = GameObject.Instantiate(pauseMenu.optionPrefab);
                menuObject.transform.parent = transform;

                PauseMenu.MenuItem menuItem = new()
                {
                    obj = menuObject,
                    locString = "MENU_OFF",
                    type = PauseMenu.WidgetType.subMenu,
                    text = menuObject.GetComponent<TextMeshPro>(),
                    func = null,
                    belowMenu = null,
                    hidden = false
                };

                menuItem.text.m_HorizontalAlignment = HorizontalAlignmentOptions.Right;
                menuItem.text.m_VerticalAlignment = VerticalAlignmentOptions.Middle;
                menuItem.text.m_havePropertiesChanged = true;
                menuItem.text.SetVerticesDirty();
                menuItem.text.SetText(text);
                menuItem.text.color = Constants.WHITECLEAR_COLOR;

                return menuItem;
            }

            public static PauseMenu.MenuItem CreateActionMenuItem(string text, System.Action action)
            {
                PauseMenu.MenuItem menuItem = CreateMenuItem(text);
                menuItem.func = DelegateSupport.ConvertDelegate<PauseMenu.menuFunctionDelegate>(action);

                return menuItem;
            }

            public static PauseMenu.MenuItem CreateSubMenuItem(string text, PauseMenu.SubMenu subMenu)
            {
                PauseMenu.MenuItem menuItem = CreateMenuItem(text);
                menuItem.belowMenu = subMenu;
                menuItem.func = DelegateSupport.ConvertDelegate<PauseMenu.menuFunctionDelegate>(pauseMenu.GoToBelowMenu);

                return menuItem;
            }

            public static PauseMenu.MenuItem CreateReturnMenuItem()
            {
                PauseMenu.MenuItem menuItem = CreateMenuItem("<<");
                menuItem.func = DelegateSupport.ConvertDelegate<PauseMenu.menuFunctionDelegate>(pauseMenu.GoToAboveMenu);

                return menuItem;
            }

            public static PauseMenu.MenuItem CreateChoiceMenuItem(string text, List<string> choices, GetChoiceFunction getChoiceFunc, SetChoiceFunction setChoiceFunc)
            {
                PauseMenu.SubMenu subMenu = new()
                {
                    isBottomMenu = true,
                    depth = 1,
                    activeItem = getChoiceFunc()
                };

                foreach (string choice in choices)
                {
                    subMenu.items.Add(CreateActionMenuItem(choice, () =>
                    {
                        setChoiceFunc(subMenu.activeItem);
                        subMenu.activeItem = getChoiceFunc();
                        pauseMenu.GoToAboveMenu();
                    }));
                }

                pauseMenu.menus.Add(subMenu);

                PauseMenu.MenuItem menuItem = CreateSubMenuItem(text, subMenu);
                menuItem.type = PauseMenu.WidgetType.toggler;
                menuItem.currentSettingFunc = DelegateSupport.ConvertDelegate<PauseMenu.getCurrentSettingDelegate>(getChoiceFunc);

                return menuItem;
            }

            public static PauseMenu.MenuItem CreateTextEntryMenuItem(string text, GetStringFunction getStringFunc, SetStringFunction setStringFunc)
            {
                GameObject textEntry = new("MenuTextEntry");
                textEntry.transform.parent = pauseMenu.menuDisableGroup.transform;
                textEntry.AddComponent<LayoutElement>();
                RectTransform topTransform = textEntry.GetComponent<RectTransform>();
                topTransform.anchorMin = Vector2.zero;
                topTransform.anchorMax = Vector2.one;
                topTransform.offsetMin = Vector2.zero;
                topTransform.offsetMax = Vector2.zero;

                GameObject textViewport = new("Text Area");
                textViewport.transform.parent = textEntry.transform;
                textViewport.AddComponent<RectMask2D>();

                RectTransform inputTransform = textViewport.GetComponent<RectTransform>();
                inputTransform.anchorMin = Vector2.zero;
                inputTransform.anchorMax = Vector2.one;
                inputTransform.offsetMin = Vector2.zero;
                inputTransform.offsetMax = Vector2.zero;

                GameObject placeholder = new("Placeholder");
                placeholder.transform.parent = textViewport.transform;

                TextMeshPro placeholderText = placeholder.AddComponent<TextMeshPro>();
                placeholderText.text = "seed";
                placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

                RectTransform placeholderTransform = placeholder.GetComponent<RectTransform>();
                placeholderTransform.anchorMin = Vector2.zero;
                placeholderTransform.anchorMax = Vector2.one;
                placeholderTransform.offsetMin = Vector2.zero;
                placeholderTransform.offsetMax = Vector2.zero;

                GameObject textObject = GameObject.Instantiate(pauseMenu.optionPrefab);
                textObject.name = "Text";
                textObject.transform.parent = textViewport.transform;

                RectTransform textTransform = textObject.GetComponent<RectTransform>();
                textTransform.anchorMin = Vector2.zero;
                textTransform.anchorMax = Vector2.one;
                textTransform.offsetMin = Vector2.zero;
                textTransform.offsetMax = Vector2.zero;

                TMP_InputField inputField = textEntry.AddComponent<TMP_InputField>();
                inputField.interactable = true;
                inputField.placeholder = placeholderText;
                inputField.textComponent = textObject.GetComponent<TextMeshPro>();
                inputField.textViewport = inputTransform;
                inputField.richText = false;
                inputField.SetGlobalFontAsset(pauseMenu.optionPrefab.GetComponent<TextMeshPro>().font);
                inputField.SetGlobalPointSize(0.6F);
                inputField.SetText(getStringFunc());

                /*PauseMenu.MenuItem subMenuItem = new()
                {
                    obj = textObject,
                    locString = "MENU_OFF",
                    type = PauseMenu.WidgetType.subMenu,
                    text = textObject.GetComponent<TextMeshPro>(),
                    func = null,
                    belowMenu = null,
                    hidden = false
                };

                PauseMenu.SubMenu subMenu = new()
                {
                    isBottomMenu = true,
                    depth = 1
                };
                subMenu.items.Add(subMenuItem);

                pauseMenu.menus.Add(subMenu);*/

                PauseMenu.MenuItem menuItem = CreateMenuItem(text);
                menuItem.widgetObj = textEntry;
                menuItem.func = DelegateSupport.ConvertDelegate<PauseMenu.menuFunctionDelegate>(new System.Action(() =>
                {
                    //pauseMenu.GoToBelowMenu();
                    //EventSystem.current.SetSelectedGameObject(textEntry, null);
                    inputField.ActivateInputField();
                }));

                return menuItem;
            }

            public static void Postfix(PauseMenu __instance)
            {
                randomizerMenu = new();
                
                pauseMenu = __instance;
                pauseMenu.menus.Add(randomizerMenu);

                __instance.mainMenu.items.Insert(3, CreateSubMenuItem("randomizer", randomizerMenu));

                randomizerMenu.depth = 1;
                randomizerMenu.items.Add(CreateReturnMenuItem());
#if DEBUG
                randomizerMenu.items.Add(CreateActionMenuItem("DEBUG: re-randomize", new System.Action(() => Instance?.OnRandomizerMenuOpened(pauseMenu))));
#endif


                randomizerMenu.items.Add(CreateChoiceMenuItem(
                    "seed type",
                    new() { "random seed", "set seed" },
                    () => Instance._setSeed ? 1 : 0,
                    (int choice) => Instance._setSeed = (choice == 1)));

                randomizerMenu.items.Add(CreateTextEntryMenuItem("seed number", () => Instance._setSeedValue.ToString(), (string value) =>
                {
                    int.TryParse(value, out Instance._setSeedValue);
                }));
            }
        }

        public void OnRandomizerMenuOpened(PauseMenu pauseMenu)
        {
            GeneratePuzzles();
            pauseMenu.ResumeGame();
        }

        public override void OnInitializeMelon()
        {
            _instance = this;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _gameObjectCache.Clear();

            SaveSystem.GenerateInstanceMap();

            GameObject.Find("AreaRoot_BonusPuzzles/GraphicsRoot/BonusArea_FadeGroup/PuzzlePanel (234)").active = false;

            GameObject basedOn = GameObject.Find("AreaRoot_BonusPuzzles/GraphicsRoot/BonusArea_FadeGroup/PuzzlePanel (232)");
            Vector3 v3 = basedOn.transform.position;
            v3.y = 29.53F;

            Puzzle.Instantiate(3000, PuzzlePanel.PanelTypes.Snake, v3, 3, 4);

            // Create template blocks for the tutorial-style puzzles.
            _templateWhiteBlock = GameObject.Instantiate(GameObject.Find("StartingArea_HintPillarBase (7)/StartingArea_HintBlocks_0 (20)"));
            _templateWhiteBlock.active = false;

            _templateBlackBlock = GameObject.Instantiate(GameObject.Find("StartingArea_HintPillarBase (7)/StartingArea_HintBlocks_0 (25)"));
            _templateBlackBlock.active = false;

            // Create a text entry field for the pause menu.
            /*_templateMenuTextInput = GameObject.Instantiate(GameObject.Find("New Pause Menu").GetComponent<PauseMenu>().optionPrefab);
            _templateMenuTextInput.name = "MenuTextEntry";
            _templateMenuTextInput.hideFlags = HideFlags.HideAndDontSave;
            TMP_InputField inputField = _templateMenuTextInput.AddComponent<TMP_InputField>();

            GameObject textDisplay = new("Display");
            textDisplay.transform.parent = _templateMenuTextInput.transform;

            GameObject placeholder = new("Placeholder");
            placeholder.transform.parent = _templateMenuTextInput.transform;

            TextMeshPro placeholderText = placeholder.AddComponent<TextMeshPro>();
            placeholderText.text = "seed";

            inputField.placeholder = placeholderText;
            inputField.textComponent = textDisplay.AddComponent<TextMeshPro>();*/

            /*GameObject textEntry = new("MenuTextEntry");
            //textEntry.transform.parent = pauseMenu.menuDisableGroup.transform;
            Vector3 textVec = textEntry.transform.localPosition;
            //textVec.z = -10;
            textEntry.transform.set_localPosition_Injected(textVec);

            GameObject textViewport = new("Text Area");
            textViewport.transform.parent = textEntry.transform;
            textViewport.AddComponent<RectMask2D>();

            RectTransform inputTransform = textViewport.GetComponent<RectTransform>();
            inputTransform.anchorMin = Vector2.zero;
            inputTransform.anchorMax = Vector2.one;
            inputTransform.offsetMin = Vector2.zero;
            inputTransform.offsetMax = Vector2.zero;

            GameObject placeholder = new("Placeholder");
            placeholder.transform.parent = textViewport.transform;

            TextMeshPro placeholderText = placeholder.AddComponent<TextMeshPro>();
            placeholderText.text = "seed";
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            RectTransform placeholderTransform = placeholder.GetComponent<RectTransform>();
            placeholderTransform.anchorMin = Vector2.zero;
            placeholderTransform.anchorMax = Vector2.one;
            placeholderTransform.offsetMin = Vector2.zero;
            placeholderTransform.offsetMax = Vector2.zero;

            GameObject textObject = new("Text");// GameObject.Instantiate(pauseMenu.optionPrefab);
            //textObject.name = "Text";
            textObject.transform.parent = textViewport.transform;
            TextMeshPro hi = textObject.AddComponent<TextMeshPro>();
            hi.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;

            TMP_InputField inputField = textEntry.AddComponent<TMP_InputField>();
            inputField.interactable = true;
            inputField.placeholder = placeholderText;
            inputField.textComponent = textObject.GetComponent<TextMeshPro>();
            inputField.textViewport = inputTransform;
            inputField.richText = false;
            //inputField.SetGlobalFontAsset(pauseMenu.optionPrefab.GetComponent<TextMeshPro>().font);
            //inputField.SetGlobalPointSize(0.6F);
            //inputField.SetText(getStringFunc());
            inputField.SetText("Goodday");*/

        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            GeneratePuzzles();
        }

        public void GeneratePuzzles()
        {







            LoggerInstance.Msg("Start generation...");

            Random seedRng = new();
            int seed = seedRng.Next(100000, 1000000);
            LoggerInstance.Msg($"Seed: {seed}");

            _rng = new Random(seed);






            Generator gen3000 = new(3000);
            gen3000.Add(Puzzle.Symbol.Diamond, Puzzle.Color.White, 4);
            gen3000.SetWildcardFlowers(4);
            gen3000.Generate();





            // Generate some puzzles.
            Puzzle hello = new();
            hello.Load(46);
            hello.SetSymbol(0, 0, Puzzle.Symbol.OnePetal, Puzzle.Color.Black);
            hello.SetSymbol(1, 0, Puzzle.Symbol.Diamond, Puzzle.Color.PetalPurple);
            hello.SetSymbol(3, 0, Puzzle.Symbol.OnePip, Puzzle.Color.Gray);
            hello.SetSymbol(1, 2, Puzzle.Symbol.Diamond, Puzzle.Color.Black);
            hello.SetSymbol(0, 3, Puzzle.Symbol.OneAntiPip, Puzzle.Color.Gray);
            hello.SetSymbol(3, 3, Puzzle.Symbol.Diamond, Puzzle.Color.Black);
            hello.Save(46);

            Generator gen97 = new(97);
            gen97.SetLocks(6);
            gen97.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Black, 3);
            gen97.Add(Puzzle.Symbol.Diamond, Puzzle.Color.White, 4);
            gen97.SetFlowers(2, 1);
            gen97.SetFlowers(4, 1);
            gen97.SetFlowers(0, 1);
            gen97.SetWildcardFlowers(1);
            //gen97.Add(Puzzle.Symbol.Diamond, Puzzle.Color.Gold, 1);
            //gen97.Add(Puzzle.Symbol.Diamond, Puzzle.Color.PetalPurple, 1);
            gen97.Add(Puzzle.Symbol.Dice, Puzzle.Color.Black, 2);
            gen97.Generate();



            PanelList.Generate();

            // Scene currentScene = SceneManager.GetSceneByName("hi");

            LoggerInstance.Msg("Done!");
        }

        public override void OnDeinitializeMelon()
        {
            _instance = null;
        }
    }
}