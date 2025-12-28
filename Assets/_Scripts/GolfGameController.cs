using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class GolfGameController : MonoBehaviour
{
    public GolfAimController aim;
    public BallController ball;
    public Image chargeFill;

    public CinemachineVirtualCamera aimCam;
    public CinemachineVirtualCamera followCam;

    public float maxCharge = 40f;
    public float chargeSpeed = 25f;

    private float charge;
    private bool charging;
    private bool shotFired;

    void Update()
    {
        HandleCharge();
        HandleCameraReset();
    }

    void HandleCharge()
    {
        if (shotFired) return;

        if (Input.GetMouseButtonDown(0))
        {
            charging = true;
            charge = 0;
        }

        if (charging && Input.GetMouseButton(0))
        {
            charge += chargeSpeed * Time.deltaTime;
            charge = Mathf.Clamp(charge, 0, maxCharge);
            chargeFill.fillAmount = charge / maxCharge;
        }

        if (charging && Input.GetMouseButtonUp(0))
        {
            charging = false;
            FireShot();
        }
    }

    void FireShot()
    {
        ball.Shoot(aim.GetAimDirection(), charge);
        chargeFill.fillAmount = 0;
        shotFired = true;

        // Switch cameras
        aimCam.Priority = 0;
        followCam.Priority = 10;
    }

    void HandleCameraReset()
    {
        if (!shotFired) return;

        if (!ball.IsMoving)
        {
            Invoke(nameof(ResetTurn), 1.0f);
        }
    }

    void ResetTurn()
    {
        shotFired = false;
        aim.ResetAim();

        aimCam.Priority = 10;
        followCam.Priority = 0;
    }
}
