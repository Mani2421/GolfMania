using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class ShotCharger : MonoBehaviour
    {
        [Header("Charge Settings")]
        public float maxCharge = 40f;
        public float chargeRate = 20f;
        public Slider chargeBarUI;

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
                if (aimSystem)
                    aimSystem.SetChargePreview(charge, maxCharge);
            }

            if (charging && Input.GetKey(KeyCode.E))
            {
                charge += chargeRate * Time.deltaTime;
                charge = Mathf.Clamp(charge, 0f, maxCharge);

                if (chargeBarUI)
                {
                    chargeBarUI.value = charge / maxCharge;

                    Image fillImage = chargeBarUI.fillRect ? chargeBarUI.fillRect.GetComponent<Image>() : null;
                    if (fillImage)
                        fillImage.color = Color.Lerp(Color.green, Color.red, charge / maxCharge);
                }

                if (aimSystem)
                    aimSystem.SetChargePreview(charge, maxCharge);
            }

            if (charging && (Input.GetKeyUp(KeyCode.E) || charge >= maxCharge))
            {
                charging = false;
                FireShot();
            }

            if (chargeBarUI)
                chargeBarUI.gameObject.SetActive(charge > 0);
        }

        private void FireShot()
        {
            // Get the forward vector of AimSystem pivot
            Vector3 aimDir = aimSystem.AimDirection;

            ball.Shoot(aimDir, charge);
            OnShotFired?.Invoke(aimDir, charge);

            charge = 0f;
            if (chargeBarUI)
            {
                chargeBarUI.value = 0f;

                Image fillImage = chargeBarUI.fillRect ? chargeBarUI.fillRect.GetComponent<Image>() : null;
                if (fillImage)
                    fillImage.color = Color.green;
            }

            if (aimSystem)
                aimSystem.SetChargePreview(0f, maxCharge);
            
            var impulse = ball.GetComponent<CinemachineImpulseSource>();
            if (impulse != null) 
            {
                // Shake strength based on how high the charge was
                impulse.GenerateImpulse(charge / maxCharge); 
            }
        }
    }
}