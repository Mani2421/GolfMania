using UnityEngine;

public class ClubAnimationEvents : MonoBehaviour
{
    [Header("References")]
    public ShotCharger shotCharger;
    public BallController ball;
    public ClubRailAimer clubRailAimer;

    // Called from animation event at the moment of impact
    public void OnClubImpact()
    {
        if (shotCharger == null || ball == null || clubRailAimer == null)
        {
            Debug.LogWarning("ClubAnimationEvents: Missing references!");
            return;
        }

        // Get the current aim direction from the rail
        Vector3 aimDir = clubRailAimer.GetAimDirection();

        // Use the charge that was built up
        float charge = shotCharger.charge;

        // Fire the ball
        ball.Shoot(aimDir, charge);

        // Reset charge after shot
        shotCharger.charge = 0f;
        if (shotCharger.chargeBarUI)
            shotCharger.chargeBarUI.fillAmount = 0f;
    }
}
