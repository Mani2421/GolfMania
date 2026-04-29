using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Scripts
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Settings")]
        public Slider sensitivitySlider;
        private const string SensKey = "MouseSensitivity";
        public float defaultSensitivity = 1.0f;

        void Start()
        {
            float savedSens = PlayerPrefs.GetFloat(SensKey, defaultSensitivity);
            
            if (sensitivitySlider != null)
            {
                sensitivitySlider.value = savedSens;
                
                sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            }
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
        

        public void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat(SensKey, value);
            PlayerPrefs.Save();
        }

        public void PlayGame()
        {
            SceneManager.LoadScene("Main");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}