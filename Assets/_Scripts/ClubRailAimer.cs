using UnityEngine;

public class ClubRailAimer : MonoBehaviour
{
    [Header("Aiming")]
    public float rotationSpeed = 120f;
    public float minAngle = -70f;
    public float maxAngle = 70f;

    private float currentAngle = 0f;

    void Start()
    {
        // Initialize correct angle
        currentAngle = transform.localEulerAngles.y;
        if (currentAngle > 180) currentAngle -= 360;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");

        currentAngle += mouseX * rotationSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    // Used by ShotCharger
    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }
}
