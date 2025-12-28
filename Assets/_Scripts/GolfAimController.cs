using UnityEngine;

public class GolfAimController : MonoBehaviour
{
    public Transform ball;
    public float radius = 0.4f;
    public float rotationSpeed = 120f;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    private float angle;

    void Update()
    {
        if (!ball) return;

        angle += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0, angle, 0);
        transform.position = ball.position - transform.forward * radius;
    }

    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }

    public void ResetAim()
    {
        angle = 0;
    }
}
