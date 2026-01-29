using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class AimSystem : MonoBehaviour
    {
        [Header("Aim Settings")]
        public float rotationSpeed = 120f;
        public float minAngle = -90f;
        public float maxAngle = 90f;

        [Header("Indicator Settings")]
        public float indicatorLength = 3f;

        private float currentAngle;
        private LineRenderer lineRenderer;

        public Vector3 AimDirection => transform.forward;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
        }

        void Update()
        {
            if (!enabled) return;

            HandleRotation();
            UpdateIndicator();
        }

        void HandleRotation()
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentAngle += mouseX * rotationSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

            transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);
        }

        void UpdateIndicator()
        {
            if (!lineRenderer.enabled) return;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * indicatorLength);
        }

        public void SetIndicatorVisible(bool visible)
        {
            lineRenderer.enabled = visible;
        }

        public void ResetAim()
        {
            currentAngle = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}