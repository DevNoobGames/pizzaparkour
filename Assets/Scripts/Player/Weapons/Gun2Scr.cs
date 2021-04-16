using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun2Scr : MonoBehaviour
{
    public Animation shootAnim;
    public AudioSource gunShotAudio;
    public ParticleSystem particles;
    public bool reloaded;

    private void Start()
    {
        reloaded = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && reloaded)
        {
            reloaded = false;
            StartCoroutine(reloadNum());
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
                    hit.transform.SendMessage("Deduct", 35, SendMessageOptions.DontRequireReceiver);
                }
                if (hit.transform.CompareTag("Olive"))
                {
                    hit.transform.SendMessage("Deduct", 35, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }

    IEnumerator reloadNum()
    {
        yield return new WaitForSeconds(0.15f);
        reloaded = true;
    }

    private void OnDisable()
    {
        reloaded = true;
    }
}
