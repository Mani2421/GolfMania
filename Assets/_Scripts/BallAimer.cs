using UnityEngine;

public class BallAimer : MonoBehaviour
{
    public float rotationSpeed = 120f;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    private float currentAngle;

    void Update()
    {
        if (!enabled) return;

        float mouseX = Input.GetAxis("Mouse X");
        currentAngle += mouseX * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }

    public void ResetAim()
    {
        currentAngle = 0f;
        transform.rotation = Quaternion.identity;
    }
}