using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
public class PlayerCharacterController : MonoBehaviour
{
    //DEVNOOB EDIt
    public AudioSource stepSound;
    public float slideSpeed = 10;
    Vector3 loc;
    public float Health = 10;
    public Slider healthSlider;
    public List<GameObject> weaponList = new List<GameObject>();
    public int activeWeapon = 0;

    public float timer;
    public bool timerActive;
    public TextMeshProUGUI timerText;
    public float score;
    public TextMeshProUGUI scoreText;

    public bool hasHandgun = false;
    public GameObject handGun;
    public bool hasAutoGun = false;
    public GameObject AutoGun;
    public AudioSource backgroundMusic;
    public AudioSource gunPickUpAudio;
    public AudioSource wonAudio;
    public AudioSource jumpAudio;
    public List<Vector3> checkpoints = new List<Vector3>();

    public PostProcessVolume v;
    private LensDistortion lensdis;
    private ChromaticAberration chrome;
    private Vignette vignett;

    public GameObject[] toDisableWhenWin;
    public GameObject[] toDisableWhenDed;
    public GameObject[] toEnableWhenWin;
    public TextMeshProUGUI winText;
    public GameObject deadPanel;

    [Header("References")]
    [Tooltip("Reference to the main camera used for the player")]
    public Camera playerCamera;
    [Tooltip("Audio source for footsteps, jump, etc...")]
    public AudioSource audioSource;

    [Header("General")]
    [Tooltip("Force applied downward when in the air")]
    public float gravityDownForce = 20f;
    [Tooltip("Physic layers checked to consider the player grounded")]
    public LayerMask groundCheckLayers = -1;
    [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
    public float groundCheckDistance = 0.05f;

    [Header("Movement")]
    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float maxSpeedOnGround = 10f;
    [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
    public float movementSharpnessOnGround = 15;
    [Tooltip("Max movement speed when crouching")]
    [Range(0, 1)]
    public float maxSpeedCrouchedRatio = 0.5f;
    [Tooltip("Max movement speed when not grounded")]
    public float maxSpeedInAir = 10f;
    [Tooltip("Acceleration speed when in the air")]
    public float accelerationSpeedInAir = 25f;
    [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
    public float sprintSpeedModifier = 2f;
    [Tooltip("Height at which the player dies instantly when falling off the map")]
    public float killHeight = -50f;

    [Header("Rotation")]
    [Tooltip("Rotation speed for moving the camera")]
    public float rotationSpeed = 200f;
    [Range(0.1f, 1f)]
    [Tooltip("Rotation speed multiplier when aiming")]
    public float aimingRotationMultiplier = 0.4f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    public float jumpForce = 9f;

    [Header("Stance")]
    [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
    public float cameraHeightRatio = 0.9f;
    [Tooltip("Height of character when standing")]
    public float capsuleHeightStanding = 1.8f;
    [Tooltip("Height of character when crouching")]
    public float capsuleHeightCrouching = 0.9f;
    [Tooltip("Speed of crouching transitions")]
    public float crouchingSharpness = 10f;

    public UnityAction<bool> onStanceChanged;

    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isCrouching { get; private set; }
    


    PlayerInputHandler m_InputHandler;
    CharacterController m_Controller;
    Vector3 m_GroundNormal;
    Vector3 m_CharacterVelocity;
    Vector3 m_LatestImpactSpeed;
    float m_LastTimeJumped = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_footstepDistanceCounter;
    float m_TargetCharacterHeight;

    const float k_JumpGroundingPreventionTime = 0.2f;
    const float k_GroundCheckDistanceInAir = 0.07f;

    WallRun wallRunComponent;

    void Start()
    {
        timerActive = true;

        checkpoints.Add(transform.position);

        v.profile.TryGetSettings(out lensdis);
        v.profile.TryGetSettings(out vignett);
        v.profile.TryGetSettings(out chrome);

        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        DebugUtility.HandleErrorIfNullGetComponent<CharacterController, PlayerCharacterController>(m_Controller, this, gameObject);

        m_InputHandler = GetComponent<PlayerInputHandler>();
        DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, PlayerCharacterController>(m_InputHandler, this, gameObject);

        wallRunComponent = GetComponent<WallRun>();

        m_Controller.enableOverlapRecovery = true;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);
    }

    void Update()
    {
        if (timerActive)
        {
            timer += Time.deltaTime;

            int timeInSecondsInt = (int)timer;  //We don't care about fractions of a second, so easy to drop them by just converting to an int
            int minutes = timeInSecondsInt / 60;  //Get total minutes
            int seconds = timeInSecondsInt - (minutes * 60);  //Get seconds for display alongside minutes
            timerText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2"); 
        }

        hasJumpedThisFrame = false;

        bool wasGrounded = isGrounded;
        GroundCheck();

        // landing
        if (isGrounded && !wasGrounded)
        {
            //Can add fall damage
            //play landing sound
            lensdis.intensity.value = 0;
            vignett.intensity.value = 0;
            chrome.intensity.value = 0;
        }

        // crouching
        if (m_InputHandler.GetCrouchInputDown())
        {
            addforce(transform.forward, slideSpeed);
            //lensdis.intensity.value = -60;
            chrome.intensity.value = 1;
            //vignett.intensity.value = 0.5f;
            SetCrouchingState(true, false);
        }
        if (m_InputHandler.GetCrouchInputReleased())
        {
            lensdis.intensity.value = 0;
            chrome.intensity.value = 0;
            vignett.intensity.value = 0;
            SetCrouchingState(false, false);
        }

        UpdateCharacterHeight(false);

        HandleCharacterMovement();

        if (isCrouching)
        {
            addforce(transform.forward, slideSpeed);

        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            if (activeWeapon >= 1 && weaponList.Count > 1)
            {
                weaponList[activeWeapon].SetActive(false);
                activeWeapon -= 1;
                weaponList[activeWeapon].SetActive(true);
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            if (activeWeapon < (weaponList.Count - 1) && weaponList.Count > 1)
            {
                weaponList[activeWeapon].SetActive(false);
                activeWeapon += 1;
                weaponList[activeWeapon].SetActive(true);
            }
        }

        if (transform.position.y < -30)
        {
            transform.position = checkpoints[checkpoints.Count - 1];
        }
    }

    public void addforce(Vector3 direction, float forcespeed)
    {
        characterVelocity += direction * forcespeed;
    }


    void GroundCheck()
    {
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    void HandleCharacterMovement()
    {
        if (!isCrouching)
        {

        
        // horizontal character rotation
        {
            transform.Rotate(new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * rotationSpeed), 0f), Space.Self);
        }

        // vertical camera rotation
        {
            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * rotationSpeed;

            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

            if (wallRunComponent != null)
            {
                playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, wallRunComponent.GetCameraRoll());
            }
            else
            {
                playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }
        }
        }
        // character movement handling
        bool isSprinting = false;
        if (!isCrouching)
        {
            isSprinting = m_InputHandler.GetSprintInputHeld();
        }
        {

            if (isSprinting)
            {
                isSprinting = SetCrouchingState(false, false);
            }

            float speedModifier = isSprinting ? sprintSpeedModifier : 1f;

            // converts move input to a worldspace vector based on our character's transform orientation
            Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());

            // handle grounded movement
            if (isGrounded || (wallRunComponent != null && wallRunComponent.IsWallRunning()))
            {
                //if (isGrounded)
                {
                    // calculate the desired velocity from inputs, max speed, and current slope
                    Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * 1.5f;
                    // reduce speed if crouching by crouch speed ratio
                    if (isCrouching)
                        targetVelocity *= maxSpeedCrouchedRatio;
                    targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;

                    // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                    characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);
                }

                // jumping
                if ((isGrounded || (wallRunComponent != null && wallRunComponent.IsWallRunning())) && m_InputHandler.GetJumpInputDown())
                {
                    // force the crouch state to false
                    if (SetCrouchingState(false, false))
                    {
                        if (isGrounded)
                        {
                            // start by canceling out the vertical component of our velocity
                            characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                            // then, add the jumpSpeed value upwards
                            characterVelocity += Vector3.up * jumpForce;
                        }
                        else
                        {
                            characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                            // then, add the jumpSpeed value upwards
                            characterVelocity += wallRunComponent.GetWallJumpDirection() * jumpForce;
                        }

                        // remember last time we jumped because we need to prevent snapping to ground for a short time
                        m_LastTimeJumped = Time.time;
                        hasJumpedThisFrame = true;

                        // Force grounding to false
                        isGrounded = false;
                        m_GroundNormal = Vector3.up;
                    }
                }

            }
            // handle air movement
            else
            {
                if (wallRunComponent == null || (wallRunComponent != null && !wallRunComponent.IsWallRunning()))
                {
                    // add air acceleration
                    characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                    // limit air speed to a maximum, but only horizontally
                    float verticalVelocity = characterVelocity.y;
                    Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * 1.5f);
                    characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                    // apply the gravity to the velocity
                    characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
                }
            }
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        m_Controller.Move(characterVelocity * Time.deltaTime);

        // detect obstructions to adjust velocity accordingly
        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
        {
            // We remember the last impact speed because the fall damage logic might need it
            m_LatestImpactSpeed = characterVelocity;

            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
        }
    }

    public void addScore(float points)
    {
        score += points;
        scoreText.text = score.ToString("F0");
    }

    // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    // Gets a reoriented direction that is tangent to a given slope
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void UpdateCharacterHeight(bool force)
    {
        // Update height instantly
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * cameraHeightRatio;
            //m_Actor.aimPoint.transform.localPosition = m_Controller.center;
        }
        // Update smooth height
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            // resize the capsule and adjust camera position
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, crouchingSharpness * Time.deltaTime);
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
            //m_Actor.aimPoint.transform.localPosition = m_Controller.center;
        }
    }

    // returns false if there was an obstruction
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // set appropriate heights
        if (crouched)
        {
            m_TargetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            // Detect obstructions
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(capsuleHeightStanding),
                    m_Controller.radius,
                    -1,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = capsuleHeightStanding;
        }

        if (onStanceChanged != null)
        {
            onStanceChanged.Invoke(crouched);
        }

        isCrouching = crouched;
        return true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Slider"))
        {
            Debug.Log("slider");
            Vector3 closetp = hit.transform.GetChild(0).GetComponent<Collider>().ClosestPoint(loc);
            Vector3 direction = (closetp - transform.position).normalized;
            addforce(direction, slideSpeed);
        }

        if (hit.gameObject.CompareTag("Wall"))
        {
            //lensdis.intensity.value = -60;
            chrome.intensity.value = 1;
            //vignett.intensity.value = 0.5f;

        }

        if (hit.gameObject.CompareTag("Trampoline"))
        {
            // jumping
            //if ((isGrounded || (wallRunComponent != null && wallRunComponent.IsWallRunning())) && m_InputHandler.GetJumpInputDown())
            {
                // force the crouch state to false
                SetCrouchingState(false, false);
                {
                    //if (isGrounded)
                    {
                        // start by canceling out the vertical component of our velocity
                        characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                        // then, add the jumpSpeed value upwards
                        characterVelocity += Vector3.up * (jumpForce * 3);
                        //lensdis.intensity.value = -60;
                        chrome.intensity.value = 1;
                        jumpAudio.Play();

                        //vignett.intensity.value = 0.5f;

                    }
                    /*else
                    {
                        characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                        // then, add the jumpSpeed value upwards
                        characterVelocity += wallRunComponent.GetWallJumpDirection() * (jumpForce * 3);
                    }*/

                    // remember last time we jumped because we need to prevent snapping to ground for a short time
                    m_LastTimeJumped = Time.time;
                    hasJumpedThisFrame = true;

                    // Force grounding to false
                    isGrounded = false;
                    m_GroundNormal = Vector3.up;
                }
            }
        }

        if (hit.gameObject.CompareTag("Olive"))
        {
            Destroy(hit.gameObject);
            gotHit(1);
        }

        if (hit.gameObject.CompareTag("EnemyBullet"))
        {
            CameraShake shake = Camera.main.GetComponent<CameraShake>();
            shake.shakeDuration = 0.2f;
        }

        if (hit.gameObject.CompareTag("gun1") && !hasHandgun)
        {
            if (!hasHandgun)
            {
                hasHandgun = true;
                Destroy(hit.gameObject);
                weaponList.Add(handGun);
                weaponList[activeWeapon].SetActive(false);
                activeWeapon = weaponList.Count - 1;
                weaponList[activeWeapon].SetActive(true);
                gunPickUpAudio.Play();
            }
            else
            {
                Destroy(hit.gameObject);
            }
        }
        if (hit.gameObject.CompareTag("gun2"))
        {
            if (!hasAutoGun)
            {
                hasAutoGun = true;
                Destroy(hit.gameObject);
                weaponList.Add(AutoGun);
                weaponList[activeWeapon].SetActive(false);
                activeWeapon = weaponList.Count - 1;
                weaponList[activeWeapon].SetActive(true);
                gunPickUpAudio.Play();
            }
            else
            {
                Destroy(hit.gameObject);
            }
        }

    }

    void gotHit(float damage)
    {
        CameraShake shake = Camera.main.GetComponent<CameraShake>();
        shake.shakeDuration = 0.2f;
        Health -= damage;
        healthSlider.value = Health;
        if (Health <= 0)
        {
            Debug.Log("dead"); 
            foreach (GameObject dis in toDisableWhenDed)
            {
                dis.SetActive(false);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            deadPanel.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyBullet"))
        {
            gotHit(1);
        }
        if (other.gameObject.CompareTag("checkPoint1"))
        {
            checkpoints.Add(other.gameObject.transform.position);
            Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("goal"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            timerActive = false;
            backgroundMusic.Stop();
            wonAudio.Play();
            Destroy(other.gameObject);

            foreach(GameObject en in toEnableWhenWin)
            {
                en.SetActive(true);
            }
            foreach (GameObject dis in toDisableWhenWin)
            {
                dis.SetActive(false); 
            }
            int timeInSecondsInt = (int)timer;  //We don't care about fractions of a second, so easy to drop them by just converting to an int
            int minutes = timeInSecondsInt / 60;  //Get total minutes
            int seconds = timeInSecondsInt - (minutes * 60);  //Get seconds for display alongside minutes
            winText.text = "YOU GOT THE GOLDEN PIZZA!" + "\n" + "SCORE: " + score.ToString() + "\n" + "TIME: " + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }
    }
}
