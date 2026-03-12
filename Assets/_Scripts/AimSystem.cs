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
        public float indicatorLength = 12f;
        public int indicatorSegments = 24;
        public float indicatorHeightOffset = 0.06f;
        public float raycastHeight = 3f;
        public LayerMask groundMask = ~0;

        [Header("Charge Preview")]
        public float maxPreviewForce = 40f;
        public Color lowChargeColor = new Color(0.2f, 0.85f, 1f, 0.95f);
        public Color highChargeColor = new Color(1f, 0.55f, 0.15f, 1f);
        public float widthStart = 0.12f;
        public float widthEnd = 0.03f;
        public bool pulseWidth = true;
        public float pulseAmplitude = 0.1f;
        public float pulseSpeed = 3f;

        private float currentAngle;
        private float currentCharge;
        private LineRenderer lineRenderer;

        public Vector3 AimDirection => transform.forward;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCapVertices = 8;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.positionCount = Mathf.Max(2, indicatorSegments);
            lineRenderer.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        }

        void Update()
        {
            UpdateIndicator();
        }

        // Handle camera stuff in late update to avoid jitter
        void LateUpdate()
        {
            HandleRotation();
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

            int segments = Mathf.Max(2, indicatorSegments);
            if (lineRenderer.positionCount != segments)
            {
                lineRenderer.positionCount = segments;
            }

            float charge01 = maxPreviewForce > 0f
                ? Mathf.Clamp01(currentCharge / maxPreviewForce)
                : 1f;

            float previewDistance = Mathf.Lerp(indicatorLength * 0.35f, indicatorLength, charge01);
            float yFallback = transform.position.y + indicatorHeightOffset;

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                Vector3 sample = transform.position + transform.forward * (previewDistance * t);
                Vector3 rayOrigin = sample + Vector3.up * raycastHeight;
                bool hasGroundHit = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 3f, groundMask, QueryTriggerInteraction.Ignore);

                if (hasGroundHit)
                {
                    sample.y = hit.point.y + indicatorHeightOffset;
                }
                else
                {
                    sample.y = yFallback;
                }

                lineRenderer.SetPosition(i, sample);
            }

            float pulse = 1f;
            if (pulseWidth)
            {
                pulse += Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            }

            lineRenderer.startWidth = widthStart * pulse;
            lineRenderer.endWidth = widthEnd * pulse;

            Color baseColor = Color.Lerp(lowChargeColor, highChargeColor, charge01);
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(baseColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(baseColor.a, 0f),
                    new GradientAlphaKey(Mathf.Clamp01(baseColor.a * 0.2f), 1f)
                }
            );

            
            lineRenderer.colorGradient = gradient;
        }

        public void SetIndicatorVisible(bool visible)
        {
            lineRenderer.enabled = visible;
        }

        public void SetChargePreview(float charge, float maxCharge)
        {
            currentCharge = Mathf.Max(0f, charge);
            if (maxCharge > 0f)
            {
                maxPreviewForce = maxCharge;
            }
        }

        public void ResetAim()
        {
            currentAngle = 0f;
            currentCharge = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}