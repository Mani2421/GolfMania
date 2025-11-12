using UnityEngine;

public class ClubSwing : MonoBehaviour
{
    public Transform clubPivot;
    public float backswingAngle = -60f;
    public float swingAngle = 20f;
    public float swingSpeed = 8f;
    public BallController ball;
    public ShotCharger shotCharger;
    public ClubRailAimer clubRailAimer;

    private bool swinging = false;
    private float t = 0;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            swinging = true;
            t = 0;
        }

        if (swinging)
        {
            t += Time.deltaTime * swingSpeed;
            float angle = Mathf.SmoothStep(backswingAngle, swingAngle, t);
            clubPivot.localRotation = Quaternion.Euler(angle, 0, 0);

            if (t >= 1f)
            {
                swinging = false;
                ball.Shoot(clubRailAimer.GetAimDirection(), shotCharger.charge);
            }
        }
    }
}
