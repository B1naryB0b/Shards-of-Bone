using UnityEngine;

public class ShotgunJump : MonoBehaviour
{
    [SerializeField] private float force;
    [SerializeField] private float minimumDownwardAngle = 45f; // Minimum angle to allow shotgun jump
    [SerializeField] private float jumpTimeWindow = 0.2f; // Time window in seconds to allow shotgun jump after leaving the ground
    [SerializeField] private AudioClip shotgunJumpSFX;

    private ShootBones _shootBones;
    private CPMPlayer _cpmPlayer;

    private Camera _mainCamera;
    
    private bool _canShotgunJump;
    public bool isShotgunJumping;
    private float _timeSinceGrounded;

    private void Start()
    {
        _shootBones = GetComponent<ShootBones>();
        _cpmPlayer = GetComponent<CPMPlayer>();

        _shootBones.shotgunJumpDelay = jumpTimeWindow;
        _mainCamera = Camera.main;
        _canShotgunJump = true;
    }

    private void Update()
    {
        if (_cpmPlayer.Controller.isGrounded)
        {
            _canShotgunJump = true;
            isShotgunJumping = false;
            _timeSinceGrounded = 0f;
        }
        else
        {
            _timeSinceGrounded += Time.deltaTime;
        }
        
        if (!_canShotgunJump) return;

        if (!_cpmPlayer.Controller.isGrounded && _timeSinceGrounded <= jumpTimeWindow && _shootBones.isShotgunFired && IsCameraFacingDownward())
        {
            Debug.Log("Shotgun jump");
            AudioController.Instance.PlaySound(shotgunJumpSFX);
            _cpmPlayer.AddExternalVelocity((-_mainCamera.transform.forward * force) + (_cpmPlayer.Controller.velocity.normalized * force * 0.1f));
            _shootBones.isShotgunFired = false;
            _canShotgunJump = false;
            isShotgunJumping = true;
        }
    }

    private bool IsCameraFacingDownward()
    {
        float angle = Vector3.Angle(-_mainCamera.transform.forward, Vector3.up);
        return angle <= minimumDownwardAngle;
    }
}