using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
