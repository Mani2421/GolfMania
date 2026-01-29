using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class ShotCharger : MonoBehaviour
    {
        [Header("Charge Settings")]
        public float maxCharge = 40f;
        public float chargeRate = 20f;
        public Image chargeBarUI;

        [Header("References")]
        public AimSystem aimSystem;
        public BallController ball;

        private float charge = 0f;
        private bool charging = false;

        public event Action<Vector3, float> OnShotFired; // Event for shooting

        private void Update()
        {
            HandleCharging();
        }

        private void HandleCharging()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                charging = true;
                charge = 0f;
            }

            if (charging && Input.GetKey(KeyCode.E))
            {
                charge += chargeRate * Time.deltaTime;
                charge = Mathf.Clamp(charge, 0f, maxCharge);

                if (chargeBarUI)
                    chargeBarUI.fillAmount = charge / maxCharge;
            }

            if (charging && (Input.GetKeyUp(KeyCode.E) || charge >= maxCharge))
            {
                charging = false;
                FireShot();
            }

            if (chargeBarUI)
                chargeBarUI.enabled = charge > 0;
        }

        private void FireShot()
        {
            Vector3 aimDir = aimSystem.AimDirection;

            ball.Shoot(aimDir, charge);
            OnShotFired?.Invoke(aimDir, charge);

            charge = 0f;
            if (chargeBarUI)
                chargeBarUI.fillAmount = 0f;
        }
    }
}