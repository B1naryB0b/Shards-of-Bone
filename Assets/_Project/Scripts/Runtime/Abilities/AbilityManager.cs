using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        _currentEnergy = 0f;
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
        
        if (_abilityActive && (_currentEnergy > 0f))
        {
            _currentEnergy -= energySpend * Time.unscaledDeltaTime;
        }
        else
        {
            _abilityActive = false;
            TriggerAbilityEvents(ability => ability.Deactivate());
            
            if (Input.GetButtonDown("Fire1"))
            {
                _currentEnergy -= shootEnergyCost;
            }
            
            if (!Input.GetButton("Fire1"))
            {
                _currentEnergy += energyRegen * Time.unscaledDeltaTime;
            }
        }

        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, maxEnergy);
        slider.value = _currentEnergy;
    }

    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
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
