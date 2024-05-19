using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    private PlayerInputActions _playerControls;
    private InputAction _abilityInput;
    
    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyRegen;
    [SerializeField] private float energySpend;
    [SerializeField] private float shootEnergyCost; 
    [SerializeField] private float shootCooldown;
    
    [SerializeField] private Slider slider;
    
    [SerializeField] private List<AbilitySO> abilities;

    private float _currentEnergy;
    private bool _abilityActive;
    public bool AbilityActive => _abilityActive;
    
    private float _lastShotTime;

    private void Awake()
    {
        _playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _abilityInput = _playerControls.Player.Ability;
        _abilityInput.Enable();
        _abilityInput.performed += OnAbilityPerformed;
    }

    private void OnDisable()
    {
        _abilityInput.Disable();
        _abilityInput.performed -= OnAbilityPerformed;
    }

    private void OnAbilityPerformed(InputAction.CallbackContext obj)
    {
        if (_abilityActive || Mathf.Abs(_currentEnergy - maxEnergy) < shootEnergyCost)
        {
            _abilityActive = !_abilityActive;
            Action<AbilitySO> action = _abilityActive ? (ability => ability.Activate()) : (ability => ability.Deactivate());
            TriggerAbilityAction(action);
        }
    }

    private void Start()
    {
        _currentEnergy = maxEnergy;
        slider.value = _currentEnergy;
        slider.maxValue = maxEnergy;
        
        _lastShotTime = -shootCooldown;

        TriggerAbilityAction(ability => ability.Initialise());
        
        foreach (var ability in abilities)
        {
            ability.isUnlocked = true;
        }
    }

    private void Update()
    {
        //HandleInputs();
        CalculateEnergy();
    }

    private void CalculateEnergy()
    {
        bool shouldRegenEnergy = true;

        if (_abilityActive)
        {
            if (_currentEnergy > 0f)
            {
                _currentEnergy -= energySpend * Time.unscaledDeltaTime;
                shouldRegenEnergy = false;
            }
            else
            {
                _abilityActive = false;
                TriggerAbilityAction(ability => ability.Deactivate());
            }
        }

        if (Input.GetButtonDown("Fire1") && Time.time - _lastShotTime >= shootCooldown)
        {
            _currentEnergy -= shootEnergyCost;
            _lastShotTime = Time.time;
            shouldRegenEnergy = false;
        }

        if (shouldRegenEnergy && !Input.GetButton("Fire1"))
        {
            _currentEnergy += energyRegen * Time.unscaledDeltaTime;
        }

        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, maxEnergy);
        slider.value = _currentEnergy;
    }

    private void TriggerAbilityAction(Action<AbilitySO> abilityAction)
    {
        foreach (var ability in abilities)
        {
            if (ability.isUnlocked) abilityAction(ability);
        }
    }
}
