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
        
        [Header("Audio")]
        public AudioSource rollSource; // Loop this one!
        public AudioClip hitSound;
        public float minHitVelocity = 1f;
        
        [Header("Stopping Logic")]
        public float dragMultiplier = 0.8f;
        public float slowSpeedThreshold = 1.0f;
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
        
        private void OnCollisionEnter(Collision collision)
        {
            float impactVelocity = collision.relativeVelocity.magnitude;

            if (impactVelocity > minHitVelocity)
            {
                // Play hit sound at the point of impact
                AudioSource.PlayClipAtPoint(hitSound, collision.contacts[0].point, impactVelocity / 20f);
            }
        }

        private void FixedUpdate()
        {
            // Manually bleed velocity if the ball is rolling too slowly
            if (rb.velocity.magnitude < slowSpeedThreshold && rb.velocity.magnitude > 0)
            {
                // Reduce velocity by a percentage every fixed frame
                rb.velocity *= dragMultiplier;
                rb.angularVelocity *= dragMultiplier;

                // Force a hard stop if it's below your threshold
                if (rb.velocity.magnitude < stopVelocityThreshold)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep(); // Force the physics engine to stop calculating
                }
            }
            
            // Rolling Sound Logic
            if (IsMoving && rb.velocity.magnitude > 0.2f)
            {
                if (!rollSource.isPlaying) rollSource.Play();
                // Pitch shifts higher as the ball goes faster
                rollSource.pitch = Mathf.Lerp(0.5f, 1.5f, rb.velocity.magnitude / 15f);
                rollSource.volume = Mathf.Lerp(0f, 1f, rb.velocity.magnitude / 5f);
            }
            else
            {
                rollSource.Stop();
            }
            
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