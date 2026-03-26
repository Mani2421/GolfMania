using System;
using TMPro;
using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        public float stopVelocityThreshold = 0.05f;

        private Rigidbody rb;
        private Vector3 startPos;
        private Quaternion startRot;

        public event Action OnStopped;      // Event triggered when ball stops
        public event Action OnHoleEntered;  // Event triggered when ball hits the hole

        public bool IsMoving => rb.velocity.magnitude > stopVelocityThreshold;

        public TextMeshProUGUI velocityText;
        
        [Header("Stopping Logic")]
        public float dragMultiplier = 0.95f; // How quickly to bleed velocity when slow
        public float slowSpeedThreshold = 1.0f; // Velocity at which extra friction applies

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = 20f;

            startPos = transform.position;
            startRot = transform.rotation;
        }

        void Update()
        {
            if (velocityText)
            {
                velocityText.text = $"Velocity: {rb.velocity.magnitude:F2}";
            }
        }

        private void FixedUpdate()
        {
            // 1. Manually bleed velocity if the ball is rolling too slowly
            if (rb.velocity.magnitude < slowSpeedThreshold && rb.velocity.magnitude > 0)
            {
                // Reduce velocity by a percentage every fixed frame
                rb.velocity *= dragMultiplier;
                rb.angularVelocity *= dragMultiplier;

                // 2. Force a hard stop if it's below your threshold
                if (rb.velocity.magnitude < stopVelocityThreshold)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep(); // Force the physics engine to stop calculating
                }
            }

            // 3. Trigger the stopped event
            if (!IsMoving && rb.IsSleeping())
            {
                OnStopped?.Invoke();
            }
        }

        public void Shoot(Vector3 direction, float force)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        public void ResetBall()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Returns to the position stored when the game started
            transform.position = startPos;
            transform.rotation = startRot;
        }
        
        public void SetNewStartPosition(Vector3 newPos)
        {
            startPos = newPos;
            startRot = transform.rotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hole"))
            {
                OnHoleEntered?.Invoke();
                Debug.Log("Ball entered the hole!");
            }
        }
    }
}