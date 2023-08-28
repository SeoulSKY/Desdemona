using SunTemple;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Play
{
    public class GameController : MonoBehaviour
    {
        [Tooltip("The UI to display when the game is paused")]
        [SerializeField] private GameObject pauseMenu;

        [Tooltip("The background image to display when the game is paused")]
        [SerializeField] private Image background;

        [Tooltip("The audio source to play when opening/closing a panel")]
        [SerializeField] private AudioSource panelSound;

        [Tooltip("The panel to display when the game throws an error")]
        [SerializeField] private GameObject errorPanel;
        
        private FirstPersonController _fpController;
        private CursorManager _cursorManager;
        
        private bool _isPaused;

        private void Awake()
        {
            _fpController = FindObjectOfType<FirstPersonController>();
            _cursorManager = FindObjectOfType<CursorManager>();
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
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }
            
            if (!_isPaused)
            {
                OnPaused();
            }
            else
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

            _fpController.cameraCanMove = true;
            _fpController.playerCanMove = true;
            _fpController.crosshair = true;
            _fpController.enableZoom = true;
            
            _cursorManager.gameObject.SetActive(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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
            
            _fpController.cameraCanMove = false;
            _fpController.playerCanMove = false;
            _fpController.crosshair = false;
            _fpController.enableZoom = false;
            
            _cursorManager.gameObject.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
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
            Debug.LogError($"{condition}:\n{stackTrace}");
        }
    }
}