using UnityEngine;

public class GolfObstacle : MonoBehaviour
{
    [Header("Visual Feedback")]
    public GameObject impactEffect; // Optional: Particle effect prefab
    public AudioClip bounceSound;   // Optional: Sound effect

    private AudioSource audioSource;

    void Awake()
    {
        // Add an AudioSource automatically if we have a sound
        if (bounceSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = bounceSound;
        }
    }

    // This handles the "Logic" of hitting an obstacle
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Feedback ?");
            PlayFeedback(collision.contacts[0].point);
        }
    }

    private void PlayFeedback(Vector3 point)
    {
        if (audioSource != null && bounceSound != null)
            audioSource.Play();

        if (impactEffect != null)
            Instantiate(impactEffect, point, Quaternion.identity);
    }
}