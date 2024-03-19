using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.InputSystem;
using UnityEngine;

namespace TaijiRandomizer
{
    [RegisterTypeInIl2Cpp]
    class TextEntry : MonoBehaviour
    {
        public delegate void InputFinishedHandler(bool success);

        private TextMeshPro? _inputField = null;
        private GameManager? _gameManager = null;

        private InputFinishedHandler? _inputFinishedHandler = null;
        private PauseMenu.MenuItem? _menuItem = null;

        private bool _inputActivated = false;
        private bool _firstFrame = false;
        private bool _shouldDeactivate = false;

        private string _text = "";
        private int _position = 0;

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
            }
        }
    }
}
