using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScrp : MonoBehaviour
{
    public Animation shootAnim;
    public ParticleSystem particles;
    public AudioSource gunShotAudio;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            shootAnim.Play();
            particles.Play();
            gunShotAudio.Play();

            CameraShake shake = Camera.main.GetComponent<CameraShake>();
            shake.shakeDuration = 0.1f;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    hit.transform.SendMessage("Deduct", 80, SendMessageOptions.DontRequireReceiver);
                }
                if (hit.transform.CompareTag("Olive"))
                {
                    hit.transform.SendMessage("Deduct", 80, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
