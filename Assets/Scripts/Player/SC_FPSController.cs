using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SC_FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float WalljumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;
    public bool wallJump = false;
    public bool canMoveLeftRight = true;
    public GameObject wallJumpWall;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

        if (canMove)
        {
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        }

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded || Input.GetButton("Jump") && canMove && wallJump)
        {
            moveDirection.y = jumpSpeed;
            if (wallJump)
            {
                StartCoroutine(WallJump());
            }
            wallJump = false;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        //if (canMove)
        {
            rotationX += Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            Vector3 v = transform.rotation.eulerAngles;
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, v.z);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(1f, 0.25f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        if (!canMove)
        {
            Vector3 v = transform.rotation.eulerAngles;
            Quaternion finalRot = Quaternion.Euler(v.x, v.y, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
        }
    }

    IEnumerator WallJump()
    {
        canMove = false;

        if (wallJumpWall)
        {
            //Debug.Log("has wall");
            Vector3 dirVector = wallJumpWall.transform.parent.transform.position - transform.position;
            float dirNum = AngleDir(transform.forward, dirVector, transform.up);
            if (dirNum == 1)
            {
                moveDirection -= transform.right * WalljumpSpeed *2;                //wall is on the right
                moveDirection += transform.up * WalljumpSpeed / 2;
            }
            if (dirNum == -1)
            {
                moveDirection += transform.right * WalljumpSpeed *2;                //wall is on the left
                moveDirection += transform.up * WalljumpSpeed / 2;
            }

        }
        gravity = 20f;
        Vector3 v = transform.rotation.eulerAngles;
        Quaternion finalRot = Quaternion.Euler(v.x, v.y, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
        yield return new WaitForSeconds(0.4f);
        canMove = true;
    }

    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            gravity = 0;
            Debug.Log("enter wall");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("leaving wall");
            gravity = 20f;
            /*Vector3 v = transform.rotation.eulerAngles;
            Quaternion finalRot = Quaternion.Euler(v.x, v.y, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
            wallJump = false;
            canMove = true;*/
        }
    }

    /*void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == ("Wall"))
        {
            wallJumpWall = hit.gameObject;
            gravity = 1f; // was 6
            Vector3 dirVector = hit.transform.parent.transform.position - transform.position;
            float dirNum = AngleDir(transform.forward, dirVector, transform.up);
            if (dirNum == 1 )
            {
                Vector3 v = transform.rotation.eulerAngles;
                Quaternion finalRot = Quaternion.Euler(v.x, v.y, 18);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
            }
            else if (dirNum == -1)
            {
                Vector3 v = transform.rotation.eulerAngles;
                Quaternion finalRot = Quaternion.Euler(v.x, v.y, -18);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
            }

            wallJump = true;
        }
        else if (hit.gameObject.tag != ("Wall"))
        {
            gravity = 20f;
            Vector3 v = transform.rotation.eulerAngles;
            Quaternion finalRot = Quaternion.Euler(v.x, v.y, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, finalRot, 0.1f);
            wallJump = false;
            canMove = true;
        }
    }*/

}
