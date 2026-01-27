using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class GolfGameController : MonoBehaviour
{
    public BallAimer aimer;
    public AimIndicator aimIndicator;

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
        //HandleCameraReset();
        
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ball.ResetBall();
        }
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
        ball.Shoot(aimer.GetAimDirection(), charge);
        chargeFill.fillAmount = 0;
        shotFired = true;

        aimer.enabled = false;
        aimIndicator.SetVisible(false);

        aimCam.Priority = 0;
        followCam.Priority = 10;
    }


    public void HandleCameraReset()
    {
        if (!shotFired) return;

        if (!ball.IsMoving)
        {
            Invoke(nameof(ResetTurn), 5f);
        }
    }

    public void ResetTurn()
    {
        shotFired = false;

        aimer.ResetAim();
        aimer.enabled = true;
        aimIndicator.SetVisible(true);

        aimCam.Priority = 10;
        followCam.Priority = 0;
    }

}
