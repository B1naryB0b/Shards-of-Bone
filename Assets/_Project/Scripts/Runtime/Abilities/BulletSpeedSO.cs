using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class BulletSpeedSO : AbilitySO
{
    private ShootBones _shootBones;

    private float _initialFireRate;
    private float _initialShootingForce;
    private float _initialShotgunFireRate;
    private float _initialShotgunShootingForce;

    [SerializeField] private float bulletTimeScaleFactor = 2f;

    public override void Initialise()
    {
        _shootBones = FindObjectOfType<ShootBones>();

        _initialFireRate = _shootBones.FireRate;
        _initialShootingForce = _shootBones.ShootingForce;
        _initialShotgunFireRate = _shootBones.ShotgunFireRate;
        _initialShotgunShootingForce = _shootBones.ShotgunShootingForce;
    }

    public override void Activate()
    {
        _shootBones.FireRate *= bulletTimeScaleFactor;
        _shootBones.ShootingForce *= bulletTimeScaleFactor;
        _shootBones.ShotgunFireRate *= bulletTimeScaleFactor;
        _shootBones.ShotgunShootingForce *= bulletTimeScaleFactor;
    }

    public override void Deactivate()
    {
        _shootBones.FireRate = _initialFireRate;
        _shootBones.ShootingForce = _initialShootingForce;
        _shootBones.ShotgunFireRate = _initialShotgunFireRate;
        _shootBones.ShotgunShootingForce = _initialShotgunShootingForce;
    }
}