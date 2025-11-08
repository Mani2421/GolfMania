using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float spinTorqueFactor = 0.05f;
    public float maxSpin = 200f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, float power)
    {
        rb.isKinematic = false;
        rb.WakeUp();

        // Apply impulse force
        Vector3 impulse = direction.normalized * power;
        rb.AddForce(impulse, ForceMode.Impulse);

        // Optional backspin torque
        float spin = Mathf.Clamp(power * 2f, -maxSpin, maxSpin);
        Vector3 torque = transform.right * -spin * spinTorqueFactor;
        rb.AddTorque(torque, ForceMode.Impulse);
    }
}
