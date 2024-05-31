using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FOV
{
    public float speedThreshold;
    [Range(0f, 2f)]
    public float fovScaler;
}

public class FOVManager : MonoBehaviour
{
    [SerializeField] private List<FOV> fovs;
    [SerializeField] private float lerpSpeed = 1f;

    private CPMPlayer _cpmPlayer;
    private Camera _playerCamera;
    private float _originalFOV;
    private float _targetFOV;

    void Start()
    {
        _cpmPlayer = GetComponent<CPMPlayer>();
        _playerCamera = Camera.main;
        if (_playerCamera != null && fovs.Count > 0)
        {
            _originalFOV = _playerCamera.fieldOfView;
            _targetFOV = _originalFOV;
        }
    }

    void Update()
    {
        float currentSpeed = _cpmPlayer.PlayerVelocity.magnitude;
        _targetFOV = _originalFOV;

        for (int i = fovs.Count - 1; i >= 0; i--)
        {
            if (currentSpeed >= fovs[i].speedThreshold)
            {
                _targetFOV = _originalFOV * fovs[i].fovScaler;
                break;
            }
        }

        if (_playerCamera != null)
        {
            _playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, _targetFOV, lerpSpeed * Time.deltaTime);
        }
    }
}