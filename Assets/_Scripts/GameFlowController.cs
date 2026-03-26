using System;
using Cinemachine;
using UnityEngine;

namespace _Scripts
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        public AimSystem aimSystem;
        public BallController ball;
        public ShotCharger shotCharger;

        public CinemachineVirtualCamera aimCam;
        public CinemachineVirtualCamera followCam;

        private bool shotFired;

        private void Start()
        {
            // Initial teleport to the start prefab
            MoveBallToStart();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTurn();
            }

            // Keep the AimSystem (camera pivot) at the ball's position
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
                // Capture this as the very first 'save point'
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
            aimSystem.SetIndicatorVisible(false);
            aimCam.Priority = 0;
            followCam.Priority = 1;
        }

        private void HandleBallStopped()
        {
            if (!shotFired) return;

            // KEY CHANGE: Save the new position so we take the next shot from here
            ball.SetNewStartPosition(ball.transform.position);

            ResetTurn();
        }

        private void ResetTurn()
        {
            shotFired = false;
            
            // This now returns the ball to where it just stopped, not the start of the level
            ball.ResetBall(); 
            
            aimSystem.ResetAim();
            aimSystem.enabled = true;
            aimSystem.SetIndicatorVisible(true);

            aimCam.Priority = 1;
            followCam.Priority = 0;
        }
    }
}