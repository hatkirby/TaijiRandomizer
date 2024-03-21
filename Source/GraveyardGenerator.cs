using Il2Cpp;
using UnityEngine;

namespace TaijiRandomizer
{
    internal class GraveyardGenerator
    {
        private readonly uint _id;
        private readonly Puzzle _puzzle = new();
        private readonly int _length;

        private GameObject? _gameObject = null;
        private readonly List<BinaryString> _binaryStrings = new();

        public bool Palindrome { get; set; }

        public bool Inside { get; set; }

        private static Sprite? _zeroSprite = null;
        private static Sprite? _oneSprite = null;
        private static Sprite? _dotSprite = null;
        private static Sprite? _whiteFillSprite = null;

        public string GetSortingLayer()
        {
            if (Inside)
            {
                return "GraveyardArea/Tomb";
            }
            else
            {
                return "GraveyardArea/MountainPath";
            }
        }

        public static void Initialize()
        {
            GameObject binaryObject = GameObject.Find("Graveyard_GraveyardRoot/Graveyard_Gravestones/BinaryString (Powered) (55)");
            BinaryString binaryString = binaryObject.GetComponent<BinaryString>();
            _zeroSprite = binaryString.zero;
            _oneSprite = binaryString.one;
            _dotSprite = binaryString.dot;

            GameObject pauseMenu = GameObject.Find("New Pause Menu");
            _whiteFillSprite = pauseMenu.GetComponent<PauseMenu>().sliderPrefab.GetComponent<SpriteRenderer>().sprite;
        }

        public GraveyardGenerator(uint id)
        {
            _id = id;
            _puzzle.Load(_id);
            _length = _puzzle.Width;
        }

        public void LoadExistingStrings(string path)
        {
            _gameObject = GameObject.Find(path);

            for (int i = 0; i < _gameObject.transform.childCount; i++)
            {
                GameObject child = _gameObject.transform.GetChild(i).gameObject;
                BinaryString binaryString = child.GetComponent<BinaryString>();

                if (binaryString != null)
                {
                    _binaryStrings.Add(binaryString);
                }
            }
        }

        public void CreateBackground(string parent, Color background, Vector3 position, Vector2 size)
        {
            _gameObject = new("NewBinaryString");
            _gameObject.layer = LayerMask.NameToLayer("Colliders");
            _gameObject.transform.SetParent(GameObject.Find(parent).transform);
            _gameObject.transform.set_position_Injected(position);

            SpriteRenderer bgSprite = _gameObject.AddComponent<SpriteRenderer>();
            bgSprite.sprite = _whiteFillSprite;
            bgSprite.color = background;
            bgSprite.sortingLayerName = GetSortingLayer();
            bgSprite.sortingOrder = 1;
            if (Inside)
            {
                bgSprite.bounds = new(new(4.0625F, 297.75F, 5), Vector3.zero);
            }
            else
            {
                bgSprite.bounds = new(new(4.0625F, 281.125F, 0F), Vector3.zero);
            }
            bgSprite.size = size;
            bgSprite.drawMode = SpriteDrawMode.Sliced;

            _gameObject.transform.set_localScale_Injected(Vector3.one);
        }

        public void AddString(Vector3 localPosition, bool reverse = false)
        {
            GameObject stringObject = new("BinaryString");
            stringObject.transform.SetParent(_gameObject.transform);
            stringObject.transform.set_localPosition_Injected(localPosition);

            BinaryString binaryString = stringObject.AddComponent<BinaryString>();
            binaryString.onesAndZeros = new(_length);
            binaryString.sprites = new(_length + 1);
            binaryString.zero = _zeroSprite;
            binaryString.one = _oneSprite;

            binaryString.sprites[0] = null;
            for (int i = 0; i < _length; i++)
            {
                GameObject spriteObject = new($"sprite{i + 1}");
                spriteObject.transform.parent = stringObject.transform;
                spriteObject.transform.set_localPosition_Injected(new(0.5F * i, 0, 0));
                spriteObject.layer = LayerMask.NameToLayer("Colliders");

                SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingLayerName = GetSortingLayer();
                spriteRenderer.sortingOrder = 2;
                spriteRenderer.color = new(0.75F, 0.75F, 0.75F, 1);

                if (reverse)
                {
                    binaryString.sprites[_length - i] = spriteObject;
                }
                else
                {
                    binaryString.sprites[i + 1] = spriteObject;
                }
            }

            _binaryStrings.Add(binaryString);
        }

        public void AddDot(Vector3 localPosition)
        {
            GameObject spriteObject = new("dot");
            spriteObject.transform.parent = _gameObject.transform;
            spriteObject.transform.set_localPosition_Injected(localPosition);
            spriteObject.layer = LayerMask.NameToLayer("Colliders");

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = GetSortingLayer();
            spriteRenderer.sortingOrder = 2;
            spriteRenderer.sprite = _dotSprite;
            spriteRenderer.color = new(0.75F, 0.75F, 0.75F, 1);
        }

        public void Generate()
        {
            for (int i = 0; i < _binaryStrings.Count; i++)
            {
                BinaryString binaryString = _binaryStrings[i];

                for (int j = 0; j < binaryString.onesAndZeros.Length; j++)
                {
                    if (binaryString.no_dot && j >= (int)Math.Ceiling((double)_length / 2.0))
                    {
                        break;
                    }

                    bool value = ((Randomizer.Instance?.Rng?.Next(0, 2) ?? 0) == 0);
                    binaryString.onesAndZeros[j] = value;

                    int spriteIndex = j;
                    if (!binaryString.no_dot)
                    {
                        spriteIndex++;
                    }

                    if (value)
                    {
                        binaryString.sprites[j + 1].GetComponent<SpriteRenderer>().sprite = binaryString.one;
                    }
                    else
                    {
                        binaryString.sprites[j + 1].GetComponent<SpriteRenderer>().sprite = binaryString.zero;
                    }

                    if (binaryString.no_dot)
                    {
                        int inverted = binaryString.onesAndZeros.Length - j - 1;

                        binaryString.onesAndZeros[inverted] = value;

                        if (value)
                        {
                            binaryString.sprites[inverted + 1].GetComponent<SpriteRenderer>().sprite = binaryString.one;
                        }
                        else
                        {
                            binaryString.sprites[inverted + 1].GetComponent<SpriteRenderer>().sprite = binaryString.zero;
                        }
                    }
                }
            }

            Puzzle puzzle = new Puzzle();
            puzzle.Load(_id);
            puzzle.Clear();

            for (int j = 0; j < _length; j++)
            {
                int total = 0;

                for (int i = 0; i < _binaryStrings.Count; i++)
                {
                    if (_binaryStrings[i].onesAndZeros[j])
                    {
                        total++;
                    }
                }

                puzzle.SetSolution(j, 0, total % 2 == 1);
            }

            if (Palindrome)
            {
                for (int j = 0; j < (int)(Math.Ceiling((double)_length / 2.0)); j++)
                {
                    bool value = (puzzle.IsInSolution(j, 0) != puzzle.IsInSolution(_length - j - 1, 0));
                    puzzle.SetSolution(j, 0, value);
                    puzzle.SetSolution(_length - j - 1, 0, value);
                }
            }

            puzzle.Save(_id);
            puzzle.WriteSolution(_id);
        }
    }
}
