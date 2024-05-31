using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    
    private PlayerInputActions _playerInputActions;
    private Vector2 _lookDirection;

    private PlayerStateManager _playerStateManager;
    
    [Header ("Camera")]
    public Transform playerView;
    public float playerViewYOffset = 0.6f;
    [SerializeField] Vector2 mouseSensitivity;
    [SerializeField] private float tiltAngle = 10f;
    [SerializeField] private float tiltSmooth = 5f;
    private float currentTilt = 0f;
    
    [Header ("Physics")]
    [SerializeField] float gravity;
    [SerializeField] float groundFriction;
    [SerializeField] float wallFriction;
    
    [Header("Movement")] 
    //public float terminalVelocity = 40.0f;
    public float terminalVelocityX = 40.0f;
    public float terminalVelocityY = 40.0f;

    public float moveSpeed = 7.0f;                // Ground move speed
    public float runAcceleration = 14.0f;         // Ground accel
    public float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
    public float airAcceleration = 2.0f;          // Air accel
    public float airDecceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
    public float airControl = 0.3f;               // How precise air control is
    public float grappleAirControl = 0.1f;        // Air control during grappling (NEW)
    public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
    public float grapplingSideStrafeAcceleration = 5f;
    public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
    public float jumpSpeed = 8.0f;                // The speed at which the character's up axis gains when hitting jump
    public bool holdJumpToBhop = false;           // When enabled allows player to just hold jump button to keep on bhopping perfectly. Beware: smells like casual.
    
    private bool _isGrappling = false;             // Is the player grappling

    private Vector2 mouseCameraRotation = new Vector2(0f, 0f);

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    public Vector3 PlayerVelocity
    {
        get { return playerVelocity; }
    }
    private float playerTopVelocity = 0.0f;

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;
    public bool WishJump
    {
        get { return wishJump; }
    }

    // Used to display real time fricton values
    private float playerFriction = 0.0f;

    // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private Cmd _cmd;
    
    [SerializeField] private AudioClip bHopSFX;

    private float appliedGravity;
    private ShotgunJump shotgunJump;
    private float airTime;

    private void Awake()
    {
        _playerStateManager = GetComponent<PlayerStateManager>();
        _playerInputActions = _playerStateManager.playerInputActions;
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _characterController.minMoveDistance = 0f;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        SetupCamera();
    }

    private void SetupCamera()
    {
        if (playerView == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerView = mainCamera.gameObject.transform;
        }

        // Put the camera inside the capsule collider
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
    }
    
    void Update()
    {
        LockCursor();
        
        Look();
        
        EnforceTerminalVelocity();
        
        TiltCamera();

        _characterController.Move(playerVelocity * Time.deltaTime);
        
        //You must apply Gravity after moving the character controller because .isGround is stupid
        ApplyGravity();
        
        //JumpCheck();
        
        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
        // Set the camera's position to the transform
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
        
    }

    private void Look()
    {
        _lookDirection = _playerInputActions.Player.Look.ReadValue<Vector2>();
        
        mouseCameraRotation.x -= _lookDirection.y * mouseSensitivity.x * Time.deltaTime;
        mouseCameraRotation.y += _lookDirection.x * mouseSensitivity.y * Time.deltaTime;

        // Clamp the X rotation
        if (mouseCameraRotation.x < -90f)
            mouseCameraRotation.x = -90f;
        else if (mouseCameraRotation.x > 90f)
            mouseCameraRotation.x = 90f;

        transform.rotation = Quaternion.Euler(0, mouseCameraRotation.y, 0);
    }
    
    private static void LockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked) 
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void ApplyGravity()
    {
        if (IsGrounded()) 
            playerVelocity.y = -gravity * Time.deltaTime;
        else
            playerVelocity.y -=  gravity * Time.deltaTime;
    }

    private void JumpCheck()
    {
        if(wishJump && IsGrounded())
        {
            AudioController.Instance?.PlaySound(bHopSFX);
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    public bool IsGrounded()
    { 
        return _characterController.isGrounded;
    }

    public bool IsWalled()
    {
        return false;
    }
    

    public void OnJumpPerformed()
    {
        AudioController.Instance?.PlaySound(bHopSFX);
        playerVelocity.y = jumpSpeed;
        
        /*if (holdJumpToBhop)
        {
            wishJump = true;
        }
        else
        {
            if (!wishJump)
            {
                wishJump = true;
            }
        }*/
    }
    
    public void OnJumpCanceled()
    {
        wishJump = false;
    }

    public void AirMove(Vector2 direction)
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;
        
        SetMovementDir(direction);

        wishdir =  new Vector3(_cmd.rightMove, 0, _cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if(_cmd.forwardMove == 0 && _cmd.rightMove != 0)
        {
            if(wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if(airControl > 0)
            AirControl(wishdir, wishspeed2);
        // !CPM: Aircontrol
        
    }
    
    private void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
/*        if(Mathf.Abs(_cmd.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;*/
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }

    public void GroundMove(Vector2 direction)
    {
        Vector3 wishdir;

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
        {
            ApplyFriction(1.0f);
        }
        else
        {
            ApplyFriction(0);
            Debug.Log("friction not applied");
        }

        SetMovementDir(direction);

        wishdir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

    }

    private void TiltCamera()
    {
        if (IsGrounded())
        {
            float targetTilt = 0f;
            if (_cmd.rightMove < 0) // Moving left
            {
                targetTilt = tiltAngle;
            }
            else if (_cmd.rightMove > 0) // Moving right
            {
                targetTilt = -tiltAngle;
            }

            // Smoothly interpolate the current tilt towards the target tilt
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSmooth * Time.deltaTime);
            playerView.rotation = Quaternion.Euler(mouseCameraRotation.x, mouseCameraRotation.y, currentTilt);
        }
        else
        {
            // Reset the tilt when not grounded
            currentTilt = Mathf.Lerp(currentTilt, 0f, tiltSmooth * Time.deltaTime);
            playerView.rotation = Quaternion.Euler(mouseCameraRotation.x, mouseCameraRotation.y, currentTilt);
        }
    }
    
    private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        
        if(addSpeed <= 0)
            return;
        
        float accelSpeed = accel * Time.deltaTime * wishSpeed;
        
        if(accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        playerVelocity.x += accelSpeed * wishDir.x;
        playerVelocity.z += accelSpeed * wishDir.z;
    }

    private void EnforceTerminalVelocity()
    {
        Vector3 velocityLimit = new Vector3(terminalVelocityX, terminalVelocityY, terminalVelocityX);
        playerVelocity = Vector3.Min(playerVelocity, velocityLimit);
    }
    
    private void SetMovementDir(Vector2 moveDirection)
    {
        _cmd.forwardMove = moveDirection.y;
        _cmd.rightMove   = moveDirection.x;
    }
    
    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;

        vec.y = 0.0f;
        float speed = vec.magnitude;
        float drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if(IsGrounded())
        {
            float control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * groundFriction * Time.deltaTime * t;
        }

        float newSpeed = speed - drop;
        playerFriction = newSpeed;
        if(newSpeed < 0)
            newSpeed = 0;
        if(speed > 0)
            newSpeed /= speed;

        playerVelocity.x *= newSpeed;
        playerVelocity.z *= newSpeed;
    }
    
    public void AddExternalVelocity(Vector3 velocity)
    {
        playerVelocity += velocity;
    }

    public void SetVelocity(Vector3 velocity)
    {
        playerVelocity = velocity;
    }

}
