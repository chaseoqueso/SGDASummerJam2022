using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyLump : MonoBehaviour, IInteractible
{
    public Vector2Int numCandyRange;
    public GameObject candyPrefab;
    public float candyVelocity = 5f;
    public float candyAngle = 20f;

    public void OnInteract(GameObject interactor)
    {
        int numCandies = Random.Range(numCandyRange.x, numCandyRange.y + 1);
        for(int i = 0; i < numCandies; i++)
        {
            Vector3 velocity = Vector3.up * candyVelocity;
            velocity = Quaternion.AngleAxis(candyAngle, Vector3.right) * velocity;
            velocity = Quaternion.AngleAxis(360 * (1 + i) / numCandies, Vector3.up) * velocity;

            Vector3 spawnOffset = new Vector3(velocity.x, 0, velocity.z).normalized * 0.1f;

            GameObject candy = Instantiate(candyPrefab, transform.position + spawnOffset, transform.rotation);

            Rigidbody rigidbody;
            if(candy.TryGetComponent<Rigidbody>(out rigidbody))
            {
                rigidbody.velocity = velocity;
            }
        }

        Destroy(gameObject);
    }
}
