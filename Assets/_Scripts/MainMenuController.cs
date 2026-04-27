using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject hudPanel;
        
        [Header("References")]
        public GameFlowController gameFlow;
        public AimSystem aimSystem;

        private void Start()
        {
            ShowMenu();
        }

        public void PlayGame()
        {
            mainMenuPanel.SetActive(false);
            hudPanel.SetActive(true);
            
            // Enable game systems
            aimSystem.enabled = true;
            aimSystem.SetIndicatorVisible(true);
            
            // Lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ShowMenu()
        {
            mainMenuPanel.SetActive(true);
            hudPanel.SetActive(false);
            
            // Disable gameplay systems
            aimSystem.enabled = false;
            aimSystem.SetIndicatorVisible(false);
            
            // Unlock cursor for menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}