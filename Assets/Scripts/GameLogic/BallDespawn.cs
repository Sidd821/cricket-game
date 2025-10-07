using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDespawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Debug.Log("Ball despawned: " + other.name);
            Destroy(other.gameObject); // Remove the ball when it crosses boundary
        }
    }
}
