using Cysharp.Threading.Tasks;
using SunTemple;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Play
{
    public class GameController : MonoBehaviour
    {
        [Tooltip("The slider to update based on the evaluation of the current board")]
        [SerializeField] private Slider evaluation;
        
        [Tooltip("The UI to display when the game is paused")]
        [SerializeField] private GameObject pauseMenu;

        [Tooltip("The UI to select the difficulty of the game")]
        [SerializeField] private GameObject difficultyMenu;

        [Tooltip("The background image to display when the game is paused")]
        [SerializeField] private Image background;

        [Tooltip("The audio source to play when opening/closing a panel")]
        [SerializeField] private AudioSource panelSound;

        [Tooltip("The panel to display when the game throws an error")]
        [SerializeField] private GameObject errorPanel;

        [Tooltip("The text to display whe the game is over")]
        [SerializeField] private TMP_Text gameResult;

        private FirstPersonController _fpController;
        private CursorManager _cursorManager;
        private Board _board;
        private BoardGrid _boardGrid;
        private Bot _bot;
        
        private bool _isPaused;
        private bool _canPause;
        public void CanPause(bool value)
        {
            _canPause = value;
        }

        private void Awake()
        {
            _fpController = FindObjectOfType<FirstPersonController>();
            _cursorManager = FindObjectOfType<CursorManager>();
            _board = FindObjectOfType<Board>(true);
            _boardGrid = _board.GetComponentInChildren<BoardGrid>(true);
            _bot = FindObjectOfType<Bot>();
        }

        private void Start()
        {
            _board.OnGameOver += OnGameOver;
            
            difficultyMenu.SetActive(true);
            background.gameObject.SetActive(true);
            ShowCursor(true);

            _board.OnGridUpdate += Lambda;
            return;
            
            async UniTask Lambda()
            {
                evaluation.value = 1f - await _bot.Evaluate(_boardGrid);
            }
        }
        
        private void OnEnable()
        {
            Application.logMessageReceived += LogCallback;
        }
        
        private void OnDisable()
        {
            Application.logMessageReceived -= LogCallback;
        }

        private void Update()
        {
            if (!_canPause || !Input.GetKeyDown(KeyCode.Q))
            {
                return;
            }
            
            if (!_isPaused)
            {
                OnPaused();
            }
            else if (!difficultyMenu.activeSelf)
            {
                OnResumed();
            }
        }

        public void OnResumed()
        {
            _isPaused = false;
            pauseMenu.SetActive(false);
            background.gameObject.SetActive(false);
            panelSound.Play();

            ShowCursor(false);
        }

        public void OnNewGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnPaused()
        {
            _isPaused = true;
            for (var i = 0; i < pauseMenu.transform.childCount; i++)
            {
                pauseMenu.transform.GetChild(i).gameObject.SetActive(true);
            }
            pauseMenu.SetActive(true);
            background.gameObject.SetActive(true);
            panelSound.Play();
            
           ShowCursor(true);
        }

        private void ShowCursor(bool value)
        {
            _fpController.cameraCanMove = !value;
            _fpController.playerCanMove = !value;
            _fpController.crosshair = !value;
            _fpController.enableZoom = !value;
            
            _cursorManager.gameObject.SetActive(!value);
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            _isPaused = !hasFocus;
        }

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception)
            {
                return;
            }
            
            errorPanel.SetActive(true);
            ShowCursor(true);
            panelSound.Play();
        }

        private UniTask OnGameOver(Player? winner)
        {
            gameResult.text = winner switch
            {
                Player.Bot => "You Lose!",
                Player.Human => "You Win!",
                var _ => "Draw",
            };

            evaluation.value = winner switch
            {
                Player.Bot => 0f,
                Player.Human => 1f,
                var _ => 0.5f,
            };
            
            gameResult.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public void OnInitialDifficultySelected()
        {
            ShowCursor(false);
            _canPause = true;
        }
    }
}