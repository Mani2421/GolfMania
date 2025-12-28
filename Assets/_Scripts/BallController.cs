using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float stopVelocityThreshold = 0.05f;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    public bool IsMoving => rb.velocity.magnitude > stopVelocityThreshold;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 20f; // Prevent camera chaos

        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void Shoot(Vector3 dir, float force)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    public void ResetBall()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
