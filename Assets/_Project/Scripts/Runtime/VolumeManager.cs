using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private VolumeProfile defaultVolumeProfile;
    [SerializeField] private VolumeProfile abilityActiveVolumeProfile;
    [SerializeField] private VolumeProfile playerDeathVolumeProfile; 
    
    private Volume _currentVolume;

    private AbilityManager _abilityManager;
    private PlayerDeath _playerDeath;
    
    // Start is called before the first frame update
    void Start()
    {
        _currentVolume = FindObjectOfType<Volume>();
        _abilityManager = FindObjectOfType<AbilityManager>();
        _playerDeath = FindObjectOfType<PlayerDeath>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerDeath.IsDead)
        {
            _currentVolume.profile = playerDeathVolumeProfile;
        }
        else
        {
            _currentVolume.profile = _abilityManager.AbilityActive ? abilityActiveVolumeProfile : defaultVolumeProfile;
        }
    }
}
