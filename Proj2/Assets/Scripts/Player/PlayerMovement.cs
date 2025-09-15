using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    FMOD.Studio.EventInstance fmod_run;
    FMOD.Studio.PLAYBACK_STATE state;

    Vector3 verticalVelocity;
    bool groundedPlayer = false;
    bool isCrouching = false;
    float canJump = -1;
    float pleaseJump = -1;
    int currentJumpCount = 0;
    Vector3 hitNormal;

    private float movementPower = 0f;
    private int jumpBoy = 0;
    private float airBoy = 0;
    private float dashDownBoy = 0f;
    private float dashSpeedBoy = 0f;

    public Transform cameraTransform;

    [Header("Ground")]
    public float walkSpeed = 2.0f;
    public float walkDrag = 0.9f;
    public float slopeFriction = 0.7f;

    [Header("Air")]
    public float airSpeed = 1.8f;
    public float airDrag = 0.99f;

    [Header("Crouch")]
    public float crouchSpeed = 1.8f;
    public float crouchDrag = 0.99f;

    [Header("Jump")]
    public float jumpHeight = 1.0f;
    public float jumpCrouchHeight = 0.7f;
    public float gravity = -9.81f;
    public float coyoteTime = 0.5f;
    int jumpPhase = 1; // 0 = pending in air; -1 = landing

    [Header("Dash")]
    public ParticleSystem dashParticles;
    public float dashSpeed = 8f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2.0f;
    float dashTimer = 10000;
    bool isDashing = false;
    bool canDash = true;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.5f;
    private bool isKnockedback = false;
    private Vector3 knockbackVelocity = Vector3.zero;
    private float knockbackTimer = 0f;

    [Header("Other")]
    public int numberOfJumps = 1;
    public float stepOffset = 0.3f;
    public float standHeight = 1.8f;
    public float standCenter = 0f;
    public float crouchHeight = 1f;
    public float crouchCenter = 0.3f;
    public bool toggleCrouchMode = false;

    [Header("Weapons & Crosshair")]
    public bool isInMeleeMode = false;
    public GameObject pistol;
    public GameObject rifle;
    public GameObject sword;

    public GameObject crosshair;

    private float walkSpeedPowPow = 0f;
    private float airSpeedPowPow = 0f;

    private float walkSpeedMoMo = 0f;
    private float airSpeedMoMo = 0f;
    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        numberOfJumps = PlayerPrefs.GetInt("JumpLevel", 1) + PlayerPrefs.GetInt("jumpBoy", 0);
        int dashLevel = PlayerPrefs.GetInt("DashLevel", 1);
        dashCooldown = dashCooldown / dashLevel;

        int MovementLevel = PlayerPrefs.GetInt("MovementLevel", 1);
        float MovementPower = PlayerPrefs.GetFloat("MovementPower", 0f);
        airSpeed += PlayerPrefs.GetFloat("airBoy", 0f) - PlayerPrefs.GetFloat("DoubleHealthHalveSpeedAirPart", 0f) + PlayerPrefs.GetFloat("DoubleMovementHalvePowerAirPart", 0f);
        walkSpeed = walkSpeed + (MovementLevel - 1) * 0.50f + MovementPower - PlayerPrefs.GetFloat("DoubleHealthHalveSpeedWalkPart", 0f) + PlayerPrefs.GetFloat("DoubleMovementHalvePowerAirPart", 0f);
        dashCooldown -= PlayerPrefs.GetFloat("dashDownBoy", 0f);
        dashSpeed += PlayerPrefs.GetFloat("dashSpeedBoy", 0f);

        fmod_run = FMODUnity.RuntimeManager.CreateInstance("event:/High Priority/Passos/Correr grande");
        fmod_run.setPitch(1.3f);
    }

    void Update()
    {
        if (transform.position.y < -200f)
        {
            Die();
            return;
        }

        SwitchPlayerMode();

        // Handle knockback first - if knocked back, skip normal movement
        if (isKnockedback)
        {
            HandleKnockback();
            return;
        }

        groundedPlayer = controller.isGrounded && (Vector3.Angle(Vector3.up, hitNormal) <= controller.slopeLimit); ;

        if (groundedPlayer)
        {
            canJump = coyoteTime;
            currentJumpCount = 0;
        }
        else
        {
            canJump -= Time.deltaTime;
        }

        HandleCrouch();

        controller.Move(MoveXZ() + MoveY());

        if (!isDashing && canDash && !isCrouching)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Vector3 dashDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

                if (dashDir.sqrMagnitude > 0.1f)
                {
                    dashDir = cameraTransform.forward * dashDir.z + cameraTransform.right * dashDir.x;
                    dashDir.y = 0f;
                    dashDir.Normalize();
                    StartCoroutine(Dash(dashDir));
                }
            }
        }
    }

    public void ApplyKnockback(Vector3 knockbackVector)
    {
        knockbackVelocity = knockbackVector;
        knockbackTimer = knockbackDuration;
        isKnockedback = true;
    }

    void HandleKnockback()
    {
        controller.Move(knockbackVelocity * Time.deltaTime);

        knockbackTimer -= Time.deltaTime;
        if (knockbackTimer <= 0f)
        {
            isKnockedback = false;
            knockbackVelocity = Vector3.zero;
        }
    }

    Vector3 MoveXZ()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 move = (forward * moveInput.z + right * moveInput.x).normalized;

        Vector3 currentVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        if (isCrouching && isInMeleeMode)
        {
            return Vector3.zero;
        }

        if (groundedPlayer)
        {
            fmod_run.getPlaybackState(out state);
            if (move != Vector3.zero && state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                fmod_run.start();
            else if (move == Vector3.zero && state == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                fmod_run.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            Vector3 velocity;
            if (!isCrouching)
                velocity = (currentVelocity * walkDrag + move * walkSpeed) * Time.deltaTime;
            else
                velocity = (currentVelocity * crouchDrag + move * crouchSpeed) * Time.deltaTime;

            return AdjustVelocityToSlope(velocity);
        }
        else
        {
            fmod_run.getPlaybackState(out state);
            if (state == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                fmod_run.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            return (currentVelocity * airDrag + move * airSpeed) * Time.deltaTime;
        }
    }

    Vector3 MoveY()
    {
        if (Input.GetButtonDown("Jump")) pleaseJump = coyoteTime;
        else pleaseJump -= Time.deltaTime;

        if (groundedPlayer && verticalVelocity.y < 0) verticalVelocity.y = 0f;

        if (isDashing)
        {
            verticalVelocity.y = 0;
            return verticalVelocity * Time.deltaTime;
        }

        verticalVelocity.x = 0;
        verticalVelocity.z = 0;

        if ((canJump > 0 || currentJumpCount < numberOfJumps) && pleaseJump > 0)
        {
            pleaseJump = -1;
            canJump = -1;

            currentJumpCount++;
            verticalVelocity.y = Mathf.Sqrt((isCrouching ? jumpCrouchHeight : jumpHeight) * -3.0f * gravity);

            if (currentJumpCount >= 1) StartCoroutine(HandleDoubleAirJump());

            FMODUnity.RuntimeManager.PlayOneShot("event:/High Priority/Saltar e Aterrar/Saltar 2");
        }
        else if (groundedPlayer && jumpPhase == 0)
        {
            jumpPhase = -1;
        }
        else if (!groundedPlayer)
        {
            jumpPhase = 0;

            verticalVelocity.x = (1f - hitNormal.y) * hitNormal.x * slopeFriction;
            verticalVelocity.z = (1f - hitNormal.y) * hitNormal.z * slopeFriction;
        }
        hitNormal = Vector3.zero;

        verticalVelocity.y += gravity * Time.deltaTime;

        return verticalVelocity * Time.deltaTime;
    }

    Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.2f))
        {
            var ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, controller.height / 2 + 0.35f))
            {
                var slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                var adjustedVelocity = slopeRotation * velocity;

                if (adjustedVelocity.y < 0)
                {
                    return adjustedVelocity;
                }
            }
        }
        return velocity;
    }

    IEnumerator HandleDoubleAirJump()
    {
        jumpPhase = -1;
        yield return new WaitForSeconds(0.66667f);
        jumpPhase = 1;
        yield return null;
    }

    public int GetJumpPhase()
    {
        return jumpPhase;
    }

    void SwitchPlayerMode()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isInMeleeMode = !isInMeleeMode;

            if (pistol != null & rifle != null & crosshair != null)
            {
                rifle.SetActive(!isInMeleeMode);
                pistol.SetActive(!isInMeleeMode);
                crosshair.SetActive(!isInMeleeMode);
            }
            if (sword != null)
            {
                sword.SetActive(isInMeleeMode);
            }
        }
    }

    public bool GetIsInMeleeMode()
    {
        return isInMeleeMode;
    }

    void HandleCrouch()
    {
        if (toggleCrouchMode)
        {
            if (Input.GetButtonDown("Crouch"))
            {
                if (isCrouching) Uncrouch();
                else Crouch();

                isCrouching = !isCrouching;
            }
        }
        else
        {
            if (Input.GetButtonDown("Crouch")) Crouch();
            else if (Input.GetButtonUp("Crouch")) Uncrouch();
        }
    }

    void Crouch()
    {
        isCrouching = true;
        canDash = false;
        controller.height = crouchHeight;
        controller.center = new Vector3(0, crouchCenter, 0);
        controller.Move(controller.velocity * Time.deltaTime + Vector3.down * 0.5f);

        fmod_run.setPitch(0.7f);
    }

    void Uncrouch()
    {
        controller.Move(controller.velocity * Time.deltaTime + Vector3.up * 0.7f);

        isCrouching = false;
        canDash = true;
        controller.height = standHeight;
        controller.center = new Vector3(0, standCenter, 0);

        fmod_run.setPitch(1.3f);
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }

    IEnumerator Dash(Vector3 direction)
    {
        isDashing = true;
        canDash = false;

        direction.y = 0f;
        direction.Normalize();

        dashTimer = 0f;

        dashParticles.Play();

        while (dashTimer < dashDuration)
        {
            controller.Move(direction * dashSpeed * Time.deltaTime);
            dashTimer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        while (dashTimer < dashCooldown)
        {
            dashTimer += Time.deltaTime;
            yield return null;
        }

        canDash = true;
        yield return null;
    }

    public float GetCurrentDashPercentage()
    {
        return dashTimer * 100 / dashCooldown;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    // power up functions
    public void IncreaseMovementSpeed(float value)
    {
        walkSpeed += value;
        movementPower += value;
        PlayerPrefs.SetFloat("MovementPower", movementPower);
    }

    public void ReduceInHalfMovementSpeed()
    {
        walkSpeed /= 2;
        airSpeed /= 2;

        walkSpeedPowPow += walkSpeed;
        airSpeedPowPow += airSpeed;

        PlayerPrefs.SetFloat("DoubleHealthHalveSpeedWalkPart", walkSpeedPowPow);  
        PlayerPrefs.SetFloat("DoubleHealthHalveSpeedAirPart", airSpeedPowPow);  
    }

    public void DoubleMovementSpeed()
    {
        walkSpeedMoMo += walkSpeed;
        airSpeedMoMo += airSpeed;

        PlayerPrefs.SetFloat("DoubleMovementHalvePowerWalkPart", walkSpeedMoMo);  
        PlayerPrefs.SetFloat("DoubleMovementHalvePowerAirPart", airSpeedMoMo);  

        walkSpeed *= 2;
        airSpeed *= 2;
    }

    public void IncreaseJumpNumber(int value)
    {
        numberOfJumps += value;
        jumpBoy += value;
        PlayerPrefs.SetInt("jumpBoy", jumpBoy);
    }

    public void IncreaseAirSpeed(float value)
    {
        airSpeed += value;
        airBoy += value;
        PlayerPrefs.SetFloat("airBoy", airBoy);
    }

    public void IncreaseDashDistance(float value)
    {
        dashSpeed += value;
        dashSpeedBoy += value;
        PlayerPrefs.SetFloat("dashSpeedBoy", dashSpeedBoy);
    }

    public void ReduceDashCooldown(float value)
    {
        if (dashCooldown > 0.1f)
        {
            dashCooldown -= value;
        }
        else
        {
            dashCooldown = value;
        }

        dashDownBoy += value;
        PlayerPrefs.SetFloat("dashDownBoy", dashDownBoy);
    }
    
    public void Die()
    {
        // Change scene when player dies.
        if (gameObject.name == "Player")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Player died, Game over screen!");
            SceneManager.LoadScene("Scenes/GameOver");
        }
    }

    private void OnDestroy()
    {
        fmod_run.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        fmod_run.release();
    }
}