using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateManager : MonoBehaviour
{
    [HideInInspector] public MovementController movementController;
    [HideInInspector] public GrappleController grappleController;
    
    public PlayerInputActions playerInputActions;
    private List<InputAction> _inputActions;
    private PlayerState _currentState;

    private List<PlayerState> _playerStates;

    [SerializeField] private float coyoteTime;
    private float _coyoteTimeCounter;

    [SerializeField] private float jumpBufferTime;
    private float _jumpBufferCounter;

    [HideInInspector] public bool canJump;
    
    private void Awake()
    {
        canJump = false;
        
        movementController = GetComponent<MovementController>();
        grappleController = GetComponent<GrappleController>();
        playerInputActions = new PlayerInputActions();
        _inputActions = new List<InputAction>();
        _currentState = new IdleState(this);

        var playerActions = playerInputActions.Player;

        _inputActions.Add(playerActions.Jump);
        _inputActions.Add(playerActions.Move);
        _inputActions.Add(playerActions.Fire);
        _inputActions.Add(playerActions.Ability);
        _inputActions.Add(playerActions.Look);
        _inputActions.Add(playerActions.Grapple);
        _inputActions.Add(playerActions.WallRun);
        _inputActions.Add(playerActions.Slide);

        _playerStates = InitializePlayerStates();

        _currentState = _playerStates.OfType<IdleState>().FirstOrDefault();
    }


    private List<PlayerState> InitializePlayerStates()
    {
        var playerStateType = typeof(PlayerState);

        // Get the assembly where PlayerState is defined
        var assembly = Assembly.GetAssembly(playerStateType);

        // Find all types that inherit from PlayerState
        var stateTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(playerStateType));

        // Instantiate each state and pass the PlayerStateManager instance to the constructor
        var states = new List<PlayerState>();
        foreach (var type in stateTypes)
        {
            // Find the constructor that takes a PlayerStateManager parameter
            var constructor = type.GetConstructor(new[] { typeof(PlayerStateManager) });
            if (constructor != null)
            {
                var state = (PlayerState)constructor.Invoke(new object[] { this });
                states.Add(state);
            }
            else
            {
                Debug.LogError($"No suitable constructor found for {type.Name}");
            }
        }

        return states;
    }
    
    private void OnEnable()
    {
        foreach (var action in _inputActions)
        {
            action.Enable();
        }

        playerInputActions.Player.Jump.canceled += OnJumpCanceled;
    }


    private void OnDisable()
    {
        foreach (var action in _inputActions)
        {
            action.Disable();
        }

        playerInputActions.Player.Jump.canceled -= OnJumpCanceled;
    }
    private void OnJumpCanceled(InputAction.CallbackContext obj)
    {
        _coyoteTimeCounter = 0f;
    }
    
    private void Update()
    {
        JumpCheck();
        HandleStateTransition();
        _currentState?.Execute();
    }

    private void JumpCheck()
    {
        if (canJump)
        {
            canJump = !canJump;
            _jumpBufferCounter = 0f;
        }
        
        // Handle coyote time
        if (movementController.IsGrounded())
        {
            _coyoteTimeCounter = coyoteTime;
            
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }
        _coyoteTimeCounter = Mathf.Max(_coyoteTimeCounter, 0f);

        // Handle jump buffer
        if (playerInputActions.Player.Jump.triggered)
        {
            _jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
        _jumpBufferCounter = Mathf.Max(_jumpBufferCounter, 0f);

        // Check if the player can jump
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f && !(_currentState.GetType() == typeof(JumpState)))
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        Debug.Log($"Can jump: {canJump}, coyote time: {_coyoteTimeCounter}, jump buffer: {_jumpBufferCounter}");
    }


    // Check Miro board for state transitions
    private void HandleStateTransition()
    {
        foreach (var state in _playerStates)
        {
            if (_currentState.CanTransitionTo(state))
            {
                ChangeState(state);
                return;
            }
        }
    }

    public void ChangeState(PlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
        //Debug.Log(_currentState);
    }
}

public abstract class PlayerState
{
    protected PlayerStateManager manager;

    protected PlayerState(PlayerStateManager manager)
    {
        this.manager = manager;
    }

    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
    public virtual bool CanTransitionTo(PlayerState nextState) => false;
}

public class GroundMoveState : PlayerState
{
    public GroundMoveState(PlayerStateManager manager) : base(manager) { }
    
    public override void Enter() { }

    public override void Execute()
    {
        Vector2 direction = manager.playerInputActions.Player.Move.ReadValue<Vector2>();
        manager.movementController.GroundMove(direction);
    }
    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is JumpState)
        {
            return manager.canJump;
        }
        if (nextState is AirMoveState)
        {
            return !manager.movementController.IsGrounded();
        }
        if (nextState is SlideState)
        {
            return manager.playerInputActions.Player.Slide.inProgress && manager.movementController.IsGrounded();
        }
        if (nextState is GrappleState)
        {
            return manager.playerInputActions.Player.Grapple.inProgress;
        }
        return false;
    }
}

public class JumpState : PlayerState
{
    public JumpState(PlayerStateManager manager) : base(manager) { }

    public override void Enter()
    {
        manager.movementController.OnJumpPerformed();
    }

    public override void Execute() { }

    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is AirMoveState)
        {
            return true;
        }
        return false;
    }
}

public class AirMoveState : PlayerState
{
    public AirMoveState(PlayerStateManager manager) : base(manager) { }
    
    public override void Enter() { }

    public override void Execute()
    {
        Vector2 direction = manager.playerInputActions.Player.Move.ReadValue<Vector2>();
        manager.movementController.AirMove(direction);
    }

    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is GroundMoveState)
        {
            return manager.movementController.IsGrounded();
        }
        if (nextState is WallRunState)
        {
            return manager.playerInputActions.Player.WallRun.inProgress && manager.movementController.IsWalled();
        }
        if (nextState is GrappleState)
        {
            return manager.playerInputActions.Player.Grapple.inProgress;
        }
        if (nextState is JumpState)
        {
            return manager.canJump;
        }
        return false;
    }
}

public class GrappleState : PlayerState
{
    public GrappleState(PlayerStateManager manager) : base(manager) { }

    public override void Enter() { }
    public override void Execute()
    {
        manager.grappleController.OnGrapplePerformed();
    }

    public override void Exit()
    {
        manager.grappleController.OnGrappleCanceled();
    }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is AirMoveState)
        {
            return true;
        }
        if (nextState is GroundMoveState)
        {
            return manager.movementController.IsGrounded();
        }
        return false;
    }
}

public class SlideState : PlayerState
{
    public SlideState(PlayerStateManager manager) : base(manager) { }
    
    public override void Enter() { }
    public override void Execute() { }
    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is GroundMoveState)
        {
            return manager.movementController.IsGrounded();
        }
        if (nextState is JumpState)
        {
            return manager.canJump;
        }
        if (nextState is AirMoveState)
        {
            return !manager.movementController.IsGrounded();
        }
        if (nextState is GrappleState)
        {
            return manager.playerInputActions.Player.Grapple.inProgress;
        }
        return false;
    }
}

public class WallRunState : PlayerState
{
    public WallRunState(PlayerStateManager manager) : base(manager) { }
    
    public override void Enter() { }
    public override void Execute() { }
    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is AirMoveState)
        {
            return true;
        }
        if (nextState is WallJumpState)
        {
            return !manager.playerInputActions.Player.WallRun.inProgress;
        }
        if (nextState is GrappleState)
        {
            return manager.playerInputActions.Player.Grapple.inProgress;
        }
        return false;
    }
}

public class WallJumpState : PlayerState
{
    public WallJumpState(PlayerStateManager manager) : base(manager) { }
    
    public override void Enter() { }
    public override void Execute() { }
    public override void Exit() { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        if (nextState is AirMoveState)
        {
            return true;
        }
        return false;
    }
}

public class IdleState : PlayerState
{
    public IdleState(PlayerStateManager manager) : base(manager) { }

    public override bool CanTransitionTo(PlayerState nextState)
    {
        return true; // Idle state can always transition to other states
    }
}
