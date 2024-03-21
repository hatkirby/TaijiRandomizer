using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.InputSystem;
using UnityEngine;

namespace TaijiRandomizer
{
    [RegisterTypeInIl2Cpp]
    class Caret : MonoBehaviour
    {
        public Caret(System.IntPtr ptr) : base(ptr) { }

        private TextEntry? _textEntry = null;
        private TextMeshPro? _textMeshPro = null;
        private SpriteRenderer? _sprite = null;

        void Start()
        {
            GameObject parentObject = gameObject.transform.parent.gameObject;
            _textEntry = parentObject.GetComponent<TextEntry>();
            _textMeshPro = parentObject.GetComponent<TextMeshPro>();

            _sprite = gameObject.AddComponent<SpriteRenderer>();
            _sprite.sprite = GameObject.Find("New Pause Menu").GetComponent<PauseMenu>().sliderPrefab.GetComponent<SpriteRenderer>().sprite;
            _sprite.sortingLayerName = "UI/Cursor";
            _sprite.sortingOrder = 4;

            Bounds bounds = new();
            bounds.center = new(0, 0, 0);
            bounds.extents = new(0.25F, 1);
            _sprite.bounds = bounds;

            transform.set_localScale_Injected(new(0.01F, 0.45F, 1));
        }

        void Update()
        {
            if (_textEntry.InputActivated)
            {
                _sprite.enabled = true;

                if (_textEntry.Position <= _textMeshPro.textInfo.characterCount)
                {
                    float x = 0;
                    float y = 0;
                    TMP_LineInfo lineInfo;

                    if (_textEntry.Position > 0)
                    {
                        TMP_CharacterInfo charInfo = _textMeshPro.textInfo.characterInfo[_textEntry.Position - 1];
                        lineInfo = _textMeshPro.textInfo.lineInfo[charInfo.lineNumber];
                        x = charInfo.topRight.x + 0.05F;
                    }
                    else
                    {
                        TMP_CharacterInfo charInfo = _textMeshPro.textInfo.characterInfo[0];
                        lineInfo = _textMeshPro.textInfo.lineInfo[charInfo.lineNumber];

                        x = charInfo.topLeft.x - 0.05F;
                    }

                    y = lineInfo.baseline + 0.2F;

                    gameObject.transform.set_localPosition_Injected(new(x, y, 0));
                }
            }
            else
            {
                _sprite.enabled = false;
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    class TextEntry : MonoBehaviour
    {
        public delegate void InputFinishedHandler(bool success);

        private TextMeshPro? _inputField = null;
        private GameManager? _gameManager = null;

        private InputFinishedHandler? _inputFinishedHandler = null;
        private PauseMenu.MenuItem? _menuItem = null;
        private Caret? _caret = null;

        private bool _inputActivated = false;
        private bool _firstFrame = false;
        private bool _shouldDeactivate = false;

        private string _text = "";
        private int _position = 0;

        public bool InputActivated
        {
            get { return _inputActivated; }
        }

        public int Position
        {
            get { return _position; }
        }

        public string Text
        {
            get { return _text; }

            set
            {
                if (!_text.Equals(value))
                {
                    _position = value.Length;

                    SetText(value);
                }
            }
        }

        private void SetText(string value)
        {
            Randomizer.Instance?.LoggerInstance.Msg($"Text is changed from \"{_text}\" to \"{value}\"");

            _text = value;

            if (_inputField != null)
            {
                _inputField.text = _text;
            }
        }

        public InputFinishedHandler OnInputFinished
        {
            set { _inputFinishedHandler = value; }
        }

        public PauseMenu.MenuItem MenuItem
        {
            set { _menuItem = value; }
        }

        public TextEntry(System.IntPtr ptr) : base(ptr) { }

        void Start()
        {
            _inputField = gameObject.GetComponent<TextMeshPro>();
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            _inputField.text = _text;
            _position = _text.Length;

            Keyboard.current.add_onTextInput(new System.Action<char>(ProcessInput));

            if (_caret == null)
            {
                GameObject caretObject = new("Caret");
                caretObject.transform.parent = gameObject.transform;
                caretObject.layer = LayerMask.NameToLayer("OffScreen_Buffer");

                _caret = caretObject.AddComponent<Caret>();
            }
        }

        public void ActivateInputField()
        {
            _inputActivated = true;
            _firstFrame = true;
        }

        public void DeactivateInputField()
        {
            _inputActivated = false;

            if (_gameManager != null)
            {
                _gameManager.enabled = true;
            }
        }

        void ProcessInput(char ch)
        {
            if (_inputActivated && !_firstFrame)
            {
                if (ch == '\b')
                {
                    if (_position > 0)
                    {
                        _position--;
                        SetText(_text.Remove(_position, 1));
                    }
                }
                else if (ch == '\n' || ch == '\r')
                {
                    _shouldDeactivate = true;
                }
                else if (!Char.IsControl(ch))
                {
                    SetText(_text.Insert(_position, ch.ToString()));
                    _position++;
                }
            }
        }

        void Update()
        {
            if (_menuItem != null)
            {
                _menuItem.textWidth = 0.3F;
            }

            if (_firstFrame)
            {
                _firstFrame = false;

                if (_gameManager != null)
                {
                    _gameManager.enabled = false;
                }

                return;
            }

            if (_shouldDeactivate)
            {
                DeactivateInputField();

                if (_inputFinishedHandler != null)
                {
                    _inputFinishedHandler(true);
                }

                _shouldDeactivate = false;

                return;
            }

            if (_inputActivated)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    DeactivateInputField();

                    if (_inputFinishedHandler != null)
                    {
                        _inputFinishedHandler(false);
                    }

                    return;
                }

                if (Keyboard.current.vKey.wasPressedThisFrame && Keyboard.current.ctrlKey.isPressed)
                {
                    SetText(_text.Insert(_position, GUIUtility.systemCopyBuffer));
                    _position += GUIUtility.systemCopyBuffer.Length;
                }

                if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
                {
                    if (_position > 0)
                    {
                        _position--;
                    }
                }

                if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    if (_position < _text.Length)
                    {
                        _position++;
                    }
                }

                if (Keyboard.current.deleteKey.wasPressedThisFrame)
                {
                    if (_position < _text.Length)
                    {
                        SetText(_text.Remove(_position, 1));
                    }
                }
            }
        }
    }
}
