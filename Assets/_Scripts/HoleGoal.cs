using UnityEngine;

namespace _Scripts
{
    public class HoleGoal : MonoBehaviour
    {
        [Header("Settings")]
        public float captureSuctionForce = 15f; // How hard the hole pulls the ball
        public float snapToCenterDistance = 0.2f; // Distance at which ball snaps to middle
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb == null) return;

                // 1. Calculate direction to the center of the hole
                Vector3 directionToCenter = transform.position - other.transform.position;
                directionToCenter.y = 0; // Only pull horizontally

                // 2. Apply suction force to pull the ball in
                rb.AddForce(directionToCenter.normalized * captureSuctionForce, ForceMode.Acceleration);

                // 3. If very close to center, kill horizontal velocity so it drops straight down
                if (directionToCenter.magnitude < snapToCenterDistance)
                {
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    other.transform.position = Vector3.Lerp(other.transform.position, 
                        new Vector3(transform.position.x, other.transform.position.y, transform.position.z), 
                        Time.deltaTime * 5f);
                }
            }
        }
    }
}