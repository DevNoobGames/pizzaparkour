using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OliveBox : MonoBehaviour
{
    public GameObject[] olives;
    public GameObject blockingObj;
    bool hasHit;

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (GameObject ol in olives)
            {
                ol.SetActive(true);
            }
        }
    }*/

    private void Start()
    {
        hasHit = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !hasHit)
        {
            hasHit = true;
            CameraShake shake = Camera.main.GetComponent<CameraShake>();
            shake.shakeDuration = 0.2f;
            if (blockingObj != null)
            {
                blockingObj.SetActive(true);
            }
            if (olives.Length > 0)
            {
                foreach (GameObject ol in olives)
                {
                    ol.SetActive(true);
                } 
            }
        }
    }
}
