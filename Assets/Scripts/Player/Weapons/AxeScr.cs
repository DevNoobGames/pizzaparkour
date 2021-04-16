using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeScr : MonoBehaviour
{
    public float Reach;
    public float shakeDuration;
    public bool canAttack;
    public Animation axeHit;
    public AudioSource axeSwing;
    public GameObject particles;
    public Transform partPos;

    //values that will be set in the Inspector
    public Transform Target;
    public float RotationSpeed;

    //values for internal use
    private Quaternion _lookRotation;
    private Quaternion LookAtRotationOnly_Y;
    private Vector3 _direction;

    public Quaternion startRot;
    public Quaternion hitRot;

    private void Start()
    {
        startRot = Quaternion.Euler(0, 0, 0);
        hitRot = Quaternion.Euler(0, -75, 0);

        axeHit["axeHit"].time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Target)
        {
            //find the vector pointing from our position to the target
            /*_direction = (Target.position - transform.position).normalized;

            //create the rotation we need to be in to look at the target
            _lookRotation = Quaternion.LookRotation(_direction);
            LookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, _lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z);


            //rotate us over time according to speed until we are in the required rotation
            //transform.localRotation = Quaternion.Slerp(transform.rotation, LookAtRotationOnly_Y, Time.deltaTime * RotationSpeed);
            transform.localRotation = Quaternion.Slerp(transform.rotation, hitRot, Time.deltaTime * RotationSpeed);
        }
        else
        {
            transform.localRotation = startRot;
        }*/

        if (Input.GetMouseButton(0) && canAttack)
        {
            axeHit["axeHit"].speed = 1;
            axeHit.Play("axeHit");

            //axeHit.Play();
            canAttack = false;
            StartCoroutine(returnToStand());
            axeSwing.Play();


            GameObject ps = Instantiate(particles, partPos.position, partPos.rotation) as GameObject;
            ps.GetComponent<ParticleSystem>().Play();
            Destroy(ps, 1);
            //particles.Play();

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Enemy") && hit.distance < Reach)
                {
                    ps.GetComponent<ParticleSystem>().startColor = Color.red;
                    hit.transform.SendMessage("Deduct", 9999, SendMessageOptions.DontRequireReceiver);
                }
                else if (hit.transform.CompareTag("Olive"))
                {
                    ps.GetComponent<ParticleSystem>().startColor = Color.green;
                    hit.transform.SendMessage("Deduct", 90, SendMessageOptions.DontRequireReceiver);

                }
                else
                {
                    CameraShake shake = Camera.main.GetComponent<CameraShake>();
                    shake.shakeDuration = shakeDuration;
                }
            }
        }
    }

    IEnumerator returnToStand()
    {
        if (transform.rotation == LookAtRotationOnly_Y)
        {
            Target = null;
        }

        yield return new WaitForSeconds(1);

        canAttack = true;
        Target = null;
        
    }

    private void OnEnable()
    {
        axeHit["axeHit"].time = 0;
        axeHit["axeHit"].speed = 0;
        axeHit.Play("axeHit");
    }

    private void OnDisable()
    {
        canAttack = true;
        Target = null;
    }
}
