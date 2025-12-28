using UnityEngine;
using UnityEngine.UI;

public class GolfShotController : MonoBehaviour
{
    [Header("References")]
    public GolfAimController aimController;
    public Rigidbody ballRb;
    public Image chargeFill;

    [Header("Shot Settings")]
    public float maxCharge = 40f;
    public float chargeSpeed = 25f;

    private float charge = 0f;
    private bool charging = false;

    void Update()
    {
        HandleCharging();
    }

    void HandleCharging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            charge = 0f;
        }

        if (charging && Input.GetMouseButton(0))
        {
            charge += chargeSpeed * Time.deltaTime;
            charge = Mathf.Clamp(charge, 0f, maxCharge);

            if (chargeFill)
                chargeFill.fillAmount = charge / maxCharge;
        }

        if (charging && Input.GetMouseButtonUp(0))
        {
            charging = false;
            Shoot();
        }
    }

    void Shoot()
    {
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        Vector3 dir = aimController.GetAimDirection();
        ballRb.AddForce(dir * charge, ForceMode.Impulse);

        charge = 0f;
        if (chargeFill)
            chargeFill.fillAmount = 0f;
    }
}
