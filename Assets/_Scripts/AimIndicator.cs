using UnityEngine;

public class AimIndicator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform ball;
    public ClubRailAimer clubRailAimer;
    public ShotCharger shotCharger;
    public float lineLength = 5f;

    void Update()
    {
        lineRenderer.enabled = shotCharger.charging;
        if (lineRenderer == null || clubRailAimer == null || ball == null)
            return;

        Vector3 startPos = ball.position + Vector3.up * 0.05f; // slightly above ball
        Vector3 endPos = startPos + clubRailAimer.GetAimDirection().normalized * lineLength;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
}
