using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float spinTorqueFactor = 0.05f;
    public float maxSpin = 200f;

    private Rigidbody rb;

    // Store initial position and rotation
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }


    // Launches the ball in a direction with a given power.
    public void Shoot(Vector3 direction, float power)
    {
        rb.isKinematic = false;
        rb.WakeUp();

        Vector3 impulse = direction.normalized * power;
        rb.AddForce(impulse, ForceMode.Impulse);

        float spin = Mathf.Clamp(power * 2f, -maxSpin, maxSpin);
        Vector3 torque = transform.right * -spin * spinTorqueFactor;
        rb.AddTorque(torque, ForceMode.Impulse);
    }

    // Resets the ball to its original starting position and stops all motion.
    public void ResetBall()
    {
        // Stop physics
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Reset position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Re-enable physics
        rb.isKinematic = false;
    }

    // Set a new start position
    public void SetStartPosition(Vector3 newStartPos)
    {
        startPosition = newStartPos;
        startRotation = transform.rotation;
    }

    void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ResetBall();
        }
    }
}
