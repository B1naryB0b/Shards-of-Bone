/*
 * - Edited by PrzemyslawNowaczyk (11.10.17)
 *   -----------------------------
 *   Deleting unused variables
 *   Changing obsolete methods
 *   Changing used input methods for consistency
 *   -----------------------------
 *
 * - Edited by NovaSurfer (31.01.17).
 *   -----------------------------
 *   Rewriting from JS to C#
 *   Deleting "Spawn" and "Explode" methods, deleting unused varibles
 *   -----------------------------
 * Just some side notes here.
 *
 * - Should keep in mind that idTech's cartisian plane is different to Unity's:
 *    Z axis in idTech is "up/down" but in Unity Z is the local equivalent to
 *    "forward/backward" and Y in Unity is considered "up/down".
 *
 * - Code's mostly ported on a 1 to 1 basis, so some naming convensions are a
 *   bit fucked up right now.
 *
 * - UPS is measured in Unity units, the idTech units DO NOT scale right now.
 *
 * - Default values are accurate and emulates Quake 3's feel with CPM(A) physics.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Contains the command the user wishes upon the character
struct Cmd
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

public class CPMPlayer : MonoBehaviour
{
    //Input
    private PlayerInputActions _playerControls;
    private InputAction _lookInput;
    private Vector2 _lookDirection;
    private InputAction _moveInput;
    private Vector2 _moveDirection;
    private InputAction _jumpInput;
    
    [Header ("Camera")]
    public Transform playerView;     // Camera
    public float playerViewYOffset = 0.6f; // The height at which the camera is bound to
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;
    [SerializeField] private float tiltAngle = 10f;
    [SerializeField] private float tiltSmooth = 5f;
    private float currentTilt = 0f;
    //
    [Header ("Physics")]
    /*Frame occuring factors*/
    public float gravity = 20.0f;
    public float bonusGravityStrength = 20.0f;
    
    [SerializeField] private float bonusGravityTime;

    public float friction = 6; //Ground friction

    /* Movement stuff */
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
    
    private CharacterController _controller;
    public CharacterController Controller => _controller;

    // Camera rotations
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    public Vector3 PlayerVelocity
    {
        get { return playerVelocity; }
    }
    private float playerTopVelocity = 0.0f;

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time fricton values
    private float playerFriction = 0.0f;

    // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private Cmd _cmd;
    
    [SerializeField] private AudioClip bHopSFX;

    private float appliedGravity;
    private ShotgunJump shotgunJump;
    private float airTime;


    private void OnEnable()
    {
        _moveInput = _playerControls.Player.Move;
        _moveInput.Enable();
        
        _lookInput = _playerControls.Player.Look;
        _lookInput.Enable();
        
        _jumpInput = _playerControls.Player.Jump;
        _jumpInput.Enable();
        _jumpInput.performed += context => OnJumpPerformed();
        _jumpInput.canceled += context => OnJumpCanceled();
    }

    private void OnDisable()
    {
        _moveInput.Disable();
        
        _lookInput.Disable();
        
        _jumpInput.Disable();
        _jumpInput.performed -= context => OnJumpPerformed();
        _jumpInput.canceled -= context => OnJumpCanceled();
    }

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
    }
    
    private void OnJumpPerformed()
    {
        if (holdJumpToBhop)
        {
            wishJump = true;
        }
        else
        {
            if (!wishJump)
            {
                wishJump = true;
            }
        }
    }

    private void OnJumpCanceled()
    {
        wishJump = false;
    }

    private void Start()
    {
        airTime = 0f;
        appliedGravity = gravity;
        // Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetupCamera();

        _controller = GetComponent<CharacterController>();
        shotgunJump = GetComponent<ShotgunJump>();
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

    private void Update()
    {
        Vector3 velocityLimit = new Vector3(terminalVelocityX, terminalVelocityY, terminalVelocityX);
        playerVelocity = Vector3.Min(playerVelocity, velocityLimit);
        
        if (!_controller.isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            airTime = 0f;
        }
        
        //int applied = !Input.GetKey(KeyCode.Space) && shotgunJump.isShotgunJumping ? 1 : 0;
        gravity = appliedGravity + Mathf.Lerp(0, bonusGravityStrength, airTime/bonusGravityTime);
        
        LockCursor();

        _lookDirection = _lookInput.ReadValue<Vector2>();
        /* Camera rotation stuff, mouse controls this shit */
        rotX -= _lookDirection.y * xMouseSensitivity * Time.deltaTime;
        rotY += _lookDirection.x * yMouseSensitivity * Time.deltaTime;

        // Clamp the X rotation
        if(rotX < -90)
            rotX = -90;
        else if(rotX > 90)
            rotX = 90;

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider

        if (_controller.isGrounded)
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
            playerView.rotation = Quaternion.Euler(rotX, rotY, currentTilt);
        }
        else
        {
            // Reset the tilt when not grounded
            currentTilt = Mathf.Lerp(currentTilt, 0f, tiltSmooth * Time.deltaTime);
            playerView.rotation = Quaternion.Euler(rotX, rotY, currentTilt);
        }


        /* Movement, here's the important part */
        if(_controller.isGrounded)
            GroundMove();
        else if(!_controller.isGrounded)
            AirMove();

        // Apply the ceiling collision check here
        CeilingCollisionCheck();
        // Move the controller
        _controller.Move(playerVelocity * Time.deltaTime);

        /* Calculate top velocity */
        Vector3 udp = playerVelocity;
        udp.y = 0.0f;
        if(udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;

        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
        // Set the camera's position to the transform
        playerView.position = new Vector3(
            transform.position.x,
            transform.position.y + playerViewYOffset,
            transform.position.z);
    }

    private void CeilingCollisionCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, playerViewYOffset + 0.1f))
        {
            if (hit.normal.y < 0 && playerVelocity.y > 0) // Only adjust if hitting ceiling and moving upwards
            {
                playerVelocity.y = 0;
            }
        }
    }
    public void SetGrapplingState(bool grappling)
    {
        _isGrappling = grappling;
        UpdateAirControl();
    }

    private void UpdateAirControl()
    {
        if (_isGrappling)
        {
            sideStrafeAcceleration = grapplingSideStrafeAcceleration;
            sideStrafeSpeed = 7f;
            //airControl = grappleAirControl;
        }
        else
        {
            sideStrafeAcceleration = 50f;
            sideStrafeSpeed = 14f;
            //airControl = 1000f;  // Reset to default when not grappling
        }
    }

    private static void LockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked) 
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /*******************************************************************************************************\
    |* MOVEMENT
    \*******************************************************************************************************/

    /**
     * Sets the movement direction based on player input
     */
    private void SetMovementDir()
    {
        _moveDirection = _moveInput.ReadValue<Vector2>();

        _cmd.forwardMove = _moveDirection.y;
        _cmd.rightMove   = _moveDirection.x;
    }

    /**
     * Execs when the player is in the air
    */
    private void AirMove()
    {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;
        
        SetMovementDir();

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

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
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

    /**
     * Called every frame when the engine detects that the player is on the ground
     */
    private void GroundMove()
    {
        Vector3 wishdir;

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        SetMovementDir();

        wishdir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime;

        if(wishJump)
        {
            AudioController.Instance.PlaySound(bHopSFX);
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    /**
     * Applies friction to the player, called in both the air and on the ground
     */
    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();

        vec.y = 0.0f;
        float speed = vec.magnitude;
        float drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if(_controller.isGrounded)
        {
            float control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
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

    public void AddExternalVelocity(Vector3 velocity)
    {
        playerVelocity += velocity;
    }

    public void SetVelocity(Vector3 velocity)
    {
        playerVelocity = velocity;
    }
    
}