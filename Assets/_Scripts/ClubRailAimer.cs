using UnityEngine;

public class ClubRailAimer : MonoBehaviour
{
    [Header("Aiming")]
    public float rotationSpeed = 120f;
    public float minAngle = -70f;
    public float maxAngle = 70f;

    [Header("Auto-Alignment")]
    public Transform ball;
    public float distanceBehindBall = 0.4f; // how far behind the ball the rail sits

    private float currentAngle = 0f;

    void Start()
    {
        // Initialize angle
        currentAngle = transform.localEulerAngles.y;
        if (currentAngle > 180) currentAngle -= 360;

        // Align behind the ball
        if (ball != null)
        {
            Vector3 offset = -ball.forward * distanceBehindBall;
            transform.position = ball.position + offset + Vector3.up * 0.05f;
        }
    }

    void Update()
    {
        // Horizontal rotation with mouse
        float mouseX = Input.GetAxis("Mouse X");
        currentAngle += mouseX * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    // Returns the forward direction of the club (used for shooting)
    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }
}
