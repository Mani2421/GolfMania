using UnityEngine;
using UnityEngine.UI;

public class ShotCharger : MonoBehaviour
{
    [Header("Charge Settings")]
    public float maxCharge = 40f;
    public float chargeRate = 20f;
    public Image chargeBarUI;

    [Header("References")]
    public ClubRailAimer clubRailAimer;
    public BallController ball;

    [HideInInspector] public float charge = 0f;
    public bool charging = false;

    void Update()
    {
        if (chargeBarUI) chargeBarUI.enabled = charge > 0;
        // Begin charging
        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            charge = 0f;
        }

        // Continue charging
        if (charging && Input.GetMouseButton(0))
        {
            charge += chargeRate * Time.deltaTime;
            charge = Mathf.Clamp(charge, 0f, maxCharge);

            if (chargeBarUI)
                chargeBarUI.fillAmount = charge / maxCharge;
        }

        // Release shot
        if ((charging && Input.GetMouseButtonUp(0)) || charge > 39f) // Automatically release charge once it's filled.
        {
            charging = false;

            // Get shot direction from rail pivot
            Vector3 aimDir = clubRailAimer.GetAimDirection();

            // Fire the ball
            ball.Shoot(aimDir, charge);

            // Reset UI
            charge = 0f;
            if (chargeBarUI)
                chargeBarUI.fillAmount = 0f;
        }
    }
}
