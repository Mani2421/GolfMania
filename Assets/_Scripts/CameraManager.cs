using Cinemachine;
using UnityEngine;

namespace _Scripts
{
    public class CameraManager : MonoBehaviour
    {
        public CinemachineVirtualCamera followCam;
        public Rigidbody ballRigidbody;
    
        [Header("Dynamic FOV")]
        public float baseFOV = 40f;
        public float maxFOV = 60f;
        public float speedThreshold = 20f;

        void Update()
        {
            if (followCam.Priority > 0) // Only run when follow cam is active
            {
                // Increase FOV (zoom out) when the ball is flying fast
                float speed = ballRigidbody.velocity.magnitude;
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed / speedThreshold);
            
                // Smoothly transition the FOV
                followCam.m_Lens.FieldOfView = Mathf.Lerp(followCam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * 2f);
            }
        }
    }
}