using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimIndicator : MonoBehaviour
{
    public float length = 3f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    void Update()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, transform.position + transform.forward * length);
    }

    public void SetVisible(bool visible)
    {
        lr.enabled = visible;
    }
}