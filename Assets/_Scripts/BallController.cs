using System;
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

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.maxAngularVelocity = 20f;

            startPos = transform.position;
            startRot = transform.rotation;
        }

        private void FixedUpdate()
        {
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

            transform.position = startPos;
            transform.rotation = startRot;
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