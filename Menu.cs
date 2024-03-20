using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using UnityEngine;

namespace TaijiRandomizer
{
    [HarmonyPatch(typeof(PauseMenu), "InitializeMenus")]
    static class Menu
    {
        public static PauseMenu? pauseMenu = null;
        public static PauseMenu.SubMenu randomizerMenu;

        private static PauseMenu.MenuItem? _seedMenuItem = null;

        private static bool _setSeed = false;
        private static int _setSeedValue = 1000000;

        public static bool SetSeed
        {
            get { return _setSeed; }

            set
            {
                _setSeed = value;

                if (_seedMenuItem != null)
                {
                    if (_setSeed)
                    {
                        _seedMenuItem.hidden = false;
                        _seedMenuItem.obj.active = true;
                    }
                    else
                    {
                        _seedMenuItem.hidden = true;
                        _seedMenuItem.obj.active = false;
                    }
                }
            }
        }

        public static int SetSeedValue
        {
            get { return _setSeedValue; }
        }

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
            GameObject textEntry = GameObject.Instantiate(pauseMenu.optionPrefab);
            textEntry.transform.parent = pauseMenu.menuDisableGroup.transform;

            TextMeshPro textObject = textEntry.GetComponent<TextMeshPro>();
            textObject.horizontalAlignment = HorizontalAlignmentOptions.Left;
            textObject.enableWordWrapping = true;

            RectTransform textTransform = textEntry.GetComponent<RectTransform>();
            textTransform.offsetMin = new(0.5F, -0.75F);
            textTransform.offsetMax = new(8, 0.3422F);
            textTransform.pivot = new(0.04F, 0.46F);

            PauseMenu.MenuItem subMenuItem = new()
            {
                obj = textEntry,
                locString = "MENU_OFF",
                type = PauseMenu.WidgetType.subMenu,
                text = textObject,
                func = null,
                belowMenu = null,
                hidden = false
            };

            TextEntry menuInputHandler = textEntry.AddComponent<TextEntry>();
            menuInputHandler.MenuItem = subMenuItem;
            menuInputHandler.Text = getStringFunc();
            menuInputHandler.OnInputFinished = (bool success) =>
            {
                pauseMenu.GoToAboveMenu();

                if (success)
                {
                    setStringFunc(menuInputHandler.Text);
                }
                
                menuInputHandler.Text = getStringFunc();
            };

            PauseMenu.SubMenu subMenu = new()
            {
                isBottomMenu = true,
                depth = 1
            };
            subMenu.items.Add(subMenuItem);

            pauseMenu.menus.Add(subMenu);

            PauseMenu.MenuItem menuItem = CreateSubMenuItem(text, subMenu);
            menuItem.func = DelegateSupport.ConvertDelegate<PauseMenu.menuFunctionDelegate>(new System.Action(() =>
            {
                pauseMenu.GoToBelowMenu();
                textObject.alpha = 1.0F;
                menuInputHandler.ActivateInputField();
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
            randomizerMenu.items.Add(CreateActionMenuItem("start new game", new System.Action(() =>
            {
                Randomizer.Instance.ShouldRandomize = true;

                pauseMenu.StartNewGame();
            })));

#if DEBUG
            randomizerMenu.items.Add(CreateActionMenuItem("DEBUG: re-randomize", new System.Action(() =>
            {
                Randomizer.Instance?.GeneratePuzzles();
                pauseMenu.ResumeGame();
            })));
#endif

            _seedMenuItem = CreateTextEntryMenuItem(
                "seed number",
                () => _setSeedValue.ToString().PadLeft(7, '0'),
                (string value) =>
                {
                    if (!int.TryParse(value, out _setSeedValue) || _setSeedValue < 1 || _setSeedValue >= 10000000)
                    {
                        _setSeedValue = 1;
                    }
                });

            randomizerMenu.items.Add(CreateChoiceMenuItem(
                "seed type",
                new() { "random seed", "set seed" },
                () => _setSeed ? 1 : 0,
                (int choice) => SetSeed = (choice == 1)));
            randomizerMenu.items.Add(_seedMenuItem);

            SetSeed = SetSeed;
        }
    }
}
