using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class AimSystem : MonoBehaviour
    {
        [Header("Aim Settings")]
        public float rotationSpeed = 100f; // Increase this value now that we use DeltaTime
        public bool invertMouse = false;

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
        
        private float currentYRotation;
        private float currentCharge;
        private LineRenderer lineRenderer;
        public BallController ball;

        public Vector3 AimDirection => transform.forward;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            // Initialize rotation to current facing
            currentYRotation = transform.eulerAngles.y;
        }

        void Update()
        {
            // Sync position to the ball constantly so the "pivot" follows the ball
            // but the rotation stays where the player pointed it.
            UpdateIndicator();
        }
        
        void LateUpdate()
        {
            // Ensure the AimSystem itself is at the Ball's position
            transform.position = ball.transform.position;
    
            // Handle the Mouse Input
            float mouseX = Input.GetAxis("Mouse X");
            currentYRotation += mouseX * rotationSpeed * Time.unscaledDeltaTime;

            // IMPORTANT: Only apply Y rotation. This keeps the pivot 
            // from ever tilting or rolling like the ball does.
            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }

        void HandleRotation()
        {
            // Get raw mouse movement
            float mouseX = Input.GetAxis("Mouse X");
    
            if (invertMouse) mouseX *= -1;

            // Use unscaledDeltaTime so aiming feels smooth even if physics slows down
            currentYRotation += mouseX * rotationSpeed * Time.unscaledDeltaTime;
    
            // Apply rotation
            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }

        void UpdateIndicator()
        {
            if (!lineRenderer.enabled) return;

            float charge01 = maxPreviewForce > 0f ? Mathf.Clamp01(currentCharge / maxPreviewForce) : 1f;
            float previewDistance = Mathf.Lerp(indicatorLength * 0.35f, indicatorLength, charge01);

            lineRenderer.positionCount = indicatorSegments;

            for (int i = 0; i < indicatorSegments; i++)
            {
                float t = i / (float)(indicatorSegments - 1);
                Vector3 sample = transform.position + transform.forward * (previewDistance * t);
                
                // Stick the line to the terrain height
                if (Physics.Raycast(sample + Vector3.up * raycastHeight, Vector3.down, out RaycastHit hit, raycastHeight * 5f, groundMask))
                {
                    sample.y = hit.point.y + indicatorHeightOffset;
                }
                lineRenderer.SetPosition(i, sample);
            }

            // Coloring
            Color baseColor = Color.Lerp(lowChargeColor, highChargeColor, charge01);
            lineRenderer.startColor = baseColor;
            lineRenderer.endColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        }

        public void SetIndicatorVisible(bool visible) => lineRenderer.enabled = visible;

        public void SetChargePreview(float charge, float maxCharge)
        {
            currentCharge = charge;
            maxPreviewForce = maxCharge;
        }

        public void ResetAim()
        {
            // Don't reset rotation to 0, keep it where it is or point toward the hole
            currentCharge = 0f;
        }
    }
}