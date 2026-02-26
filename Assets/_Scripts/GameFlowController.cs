using System;
using Cinemachine;
using Unity.VisualScripting;
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

        public ProceduralTerrainGolf proceduralTerrainGolf;

        private bool shotFired;


        private void Update()
        {
            // TODO: MAYBE MAKE A DEBUG MANAGER FOR TESTING
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTurn();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                ball.transform.position = proceduralTerrainGolf.GetStartWorldPosition();
            }
        }

        private void Start()
        {
            // TODO: On level start teleport the ball to start position.
            proceduralTerrainGolf = FindFirstObjectByType<ProceduralTerrainGolf>();
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

            aimSystem.enabled = false;
            aimSystem.SetIndicatorVisible(false);

            aimCam.Priority = 0;
            followCam.Priority = 1;
        }

        private void HandleBallStopped()
        {
            if (!shotFired) return;

            ResetTurn();
        }
        private void HandleHoleEntered()
        {
            // TODO: HANDLE PLAYER SCORE HERE.
            Debug.Log("Player scored!");
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
    }
}