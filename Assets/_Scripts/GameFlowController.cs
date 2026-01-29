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

        public ProceduralTerrainGolf proceduralTerrainGolf;

        private bool shotFired;


        private void Update()
        {
            // DEBUG
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTurn();
            }
        }

        private void Start()
        {
            //ball.transform.position = proceduralTerrainGolf.ballStart.transform.position;
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
            followCam.Priority = 10;
        }

        private void HandleBallStopped()
        {
            if (!shotFired) return;

            ResetTurn();
        }

        private void HandleHoleEntered()
        {
            Debug.Log("Player scored!");
        }

        private void ResetTurn()
        {
            shotFired = false;

            ball.ResetBall();
            aimSystem.ResetAim();
            aimSystem.enabled = true;
            aimSystem.SetIndicatorVisible(true);

            aimCam.Priority = 10;
            followCam.Priority = 0;
        }
    }
}