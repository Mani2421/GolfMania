using System;
using Cinemachine;
using UnityEngine;
using TMPro; // Required for Stroke and Win UI
using UnityEngine.SceneManagement; // Required for Restarting

namespace _Scripts
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        public AimSystem aimSystem;
        public BallController ball;
        public ShotCharger shotCharger;

        [Header("Cameras")]
        public CinemachineVirtualCamera aimCam;
        public CinemachineVirtualCamera followCam;

        [Header("UI - Stroke Counter")]
        public TextMeshProUGUI strokeText;
        private int strokeCount = 0;

        [Header("UI - Win State")]
        public GameObject winPanel;
        public TextMeshProUGUI finalStrokeText;
        public TextMeshProUGUI victoryTitleText;

        [Header("Cinematic Settings")]
        public float winCameraRotationSpeed = 15f; 
        private bool isLevelWon = false;
        private bool shotFired;

        private void Start()
        {
            // Initial setup
            MoveBallToStart();
            UpdateStrokeUI();
            
            if (winPanel != null) winPanel.SetActive(false);
        }

        private void Update()
        {
            // 1. Manual Reset
            if (Input.GetKeyDown(KeyCode.R) && !isLevelWon)
            {
                ResetTurn();
            }

            // 2. Out of Bounds Check
            if (ball != null && ball.transform.position.y < -10f)
            {
                ResetTurn();
            }

            // 3. Sync Aim Pivot to Ball Position
            if (ball != null)
            {
                aimSystem.transform.position = ball.transform.position;
            }

            // 4. Cinematic Win Camera
            if (isLevelWon)
            {
                aimSystem.transform.Rotate(Vector3.up, winCameraRotationSpeed * Time.deltaTime);
            }
        }

        private void MoveBallToStart()
        {
            GameObject startPoint = GameObject.Find("StartPoint");
            if (startPoint != null && ball != null)
            {
                ball.transform.position = startPoint.transform.position + Vector3.up * 0.5f;
                // Save the very first position
                ball.SetNewStartPosition(ball.transform.position); 
            }
        }

        private void OnEnable()
        {
            shotCharger.OnShotFired += HandleShotFired;
            ball.OnStopped += HandleBallStopped;
            ball.OnHoleEntered += HandleHoleEntered;
        }

        private void OnDisable()
        {
            shotCharger.OnShotFired -= HandleShotFired;
            ball.OnStopped -= HandleBallStopped;
            ball.OnHoleEntered -= HandleHoleEntered;
        }

        private void HandleShotFired(Vector3 dir, float force)
        {
            shotFired = true;
            
            // Increment strokes
            strokeCount++;
            UpdateStrokeUI();

            aimSystem.enabled = false;
            aimSystem.SetIndicatorVisible(false);

            aimCam.Priority = 0;
            followCam.Priority = 1;
        }

        private void HandleBallStopped()
        {
            if (!shotFired || isLevelWon) return;

            // Save the ball's position so the next shot is taken from here
            ball.SetNewStartPosition(ball.transform.position);

            ResetTurn();
        }

        private void HandleHoleEntered()
        {
            if (isLevelWon) return;

            isLevelWon = true;
            shotFired = false;

            // Stop the ball physics
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            // Switch to cinematic view (Aim Cam uses the pivot we are rotating)
            aimCam.Priority = 1;
            followCam.Priority = 0;

            // Update and Show Victory UI
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                
                if (strokeCount == 1 && victoryTitleText != null)
                    victoryTitleText.text = "HOLE IN ONE!";

                if (finalStrokeText != null)
                    finalStrokeText.text = $"Total Strokes: {strokeCount}";
            }

            // Unlock Cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResetTurn()
        {
            shotFired = false;

            ball.ResetBall();
            aimSystem.ResetAim();
            aimSystem.enabled = true;
            aimSystem.SetIndicatorVisible(true);

            aimCam.Priority = 1;
            followCam.Priority = 0;
        }

        private void UpdateStrokeUI()
        {
            if (strokeText != null)
                strokeText.text = $"Strokes: {strokeCount}";
        }
        
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}