using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegen;
    [SerializeField] private float energySpend;
    [SerializeField] private float shootEnergyCost; 
    
    [SerializeField] private Slider slider;
    
    [SerializeField] private List<AbilitySO> abilities;

    private float _currentEnergy;
    private bool _abilityActive;
    
    [SerializeField] private VolumeProfile defaultVolumeProfile;
    [SerializeField] private VolumeProfile abilityActiveVolumeProfile;
    
    private Volume _currentVolume;

    private void Start()
    {
        _currentVolume = FindObjectOfType<Volume>();
        
        _currentEnergy = maxEnergy;
        slider.value = _currentEnergy;
        slider.maxValue = maxEnergy;

        foreach (var ability in abilities)
        {
            ability.isUnlocked = true;
        }
    }

    private void Update()
    {
        HandleInputs();
        CalculateEnergy();
        
        _currentVolume.profile = _abilityActive ? abilityActiveVolumeProfile : defaultVolumeProfile;
    }

    private void CalculateEnergy()
    {
        if (_abilityActive && (_currentEnergy > 0f))
        {
            _currentEnergy -= energySpend * Time.unscaledDeltaTime;
        }
        else
        {
            _abilityActive = false;
            TriggerAbilityEvents(ability => ability.Deactivate());
            
            //This is to prevent the player gaining energy when they shotgun fire
            if (Input.GetButtonDown("Fire1")) _currentEnergy -= shootEnergyCost;
            
            if (!Input.GetButton("Fire1")) _currentEnergy += energyRegen * Time.unscaledDeltaTime;
        }
        
        
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, maxEnergy);
        slider.value = _currentEnergy;
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Math.Abs(_currentEnergy - maxEnergy) < 0.1f)
        {
            _abilityActive = !_abilityActive;
            Action<AbilitySO> action = _abilityActive ? (ability => ability.Activate()) : (ability => ability.Deactivate());
            TriggerAbilityEvents(action);
        }
    }

    private void TriggerAbilityEvents(Action<AbilitySO> abilityAction)
    {
        foreach (var ability in abilities)
        {
            if (ability.isUnlocked) abilityAction(ability);
        }
    }
}
