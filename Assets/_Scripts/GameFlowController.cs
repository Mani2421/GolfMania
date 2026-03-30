using System;
using Cinemachine;
using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace _Scripts
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        public AimSystem aimSystem;
        public BallController ball;
        public ShotCharger shotCharger;
        public TextMeshProUGUI strokeText; // Assign your UI text here

        [Header("Cameras")]
        public CinemachineVirtualCamera aimCam;
        public CinemachineVirtualCamera followCam;

        private bool shotFired;
        private int strokeCount = 0; // The counter

        private void Start()
        {
            MoveBallToStart();
            UpdateStrokeUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTurn();
            }

            if (ball != null)
            {
                aimSystem.transform.position = ball.transform.position;
            }
        }

        private void MoveBallToStart()
        {
            GameObject startPoint = GameObject.Find("StartPoint");
            if (startPoint != null && ball != null)
            {
                ball.transform.position = startPoint.transform.position + Vector3.up * 0.5f;
                ball.SetNewStartPosition(ball.transform.position); 
            }
        }

        private void OnEnable()
        {
            shotCharger.OnShotFired += HandleShotFired;
            ball.OnStopped += HandleBallStopped;
        }

        private void OnDisable()
        {
            shotCharger.OnShotFired -= HandleShotFired;
            ball.OnStopped -= HandleBallStopped;
        }

        private void HandleShotFired(Vector3 dir, float force)
        {
            shotFired = true;
            
            // Increment and Update UI
            strokeCount++;
            UpdateStrokeUI();

            aimSystem.SetIndicatorVisible(false);
            aimCam.Priority = 0;
            followCam.Priority = 1;
        }

        private void HandleBallStopped()
        {
            if (!shotFired) return;
            ball.SetNewStartPosition(ball.transform.position);
            ResetTurn();
        }
        
        private void HandleHoleEntered()
        {
            Debug.Log("Player scored!");
            // Call our global manager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.levelWinSound);
            }
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
            {
                strokeText.text = $"Strokes: {strokeCount}";
            }
        }
    }
}