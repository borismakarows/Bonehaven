using System;
using UnityEngine;

public class Combat : MonoBehaviour
{
#region Conditions
    private bool isEquippedSword;
    private bool isArmed;
    private bool isEquippedPistol;
#endregion

#region Sword 
    [Header("Sword")]
    [SerializeField] private float attackCoolDown = 0.5f;
    private float lastAttackTime;
    private bool isGuarding;
#endregion

#region Pistol
    [Header("Pistol")]
    [SerializeField] private float shootRotationSpeed = 10.0f;
    [SerializeField] private float RotateDegreeY = 805f;

#endregion

#region  Events
    public static event Action OnSlashing;
    public static event Action<bool> OnArmed;
    public static event Action<float,float> OnShooting;
    public static event Action<bool> OnGuarding;
#endregion

#region Unity Funcs.
    void Start()
    {
        isEquippedSword = true;
        isEquippedPistol = true;
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscireEvents();
    }
#endregion

#region Event Subs.
    private void SubscribeEvents()
    {
        PlayerInputManager.OnSlashFired += Slash;
        PlayerInputManager.OnWithdrawFired += Draw;
        PlayerInputManager.OnShootFired += Shoot;
        PlayerInputManager.OnGuardFired += Guard;
    }

    private void UnsubscireEvents()
    {
        PlayerInputManager.OnSlashFired -= Slash;
        PlayerInputManager.OnWithdrawFired -= Draw;
        PlayerInputManager.OnShootFired -= Shoot;
        PlayerInputManager.OnGuardFired -= Guard;
    }
#endregion

#region Sword
    private void Slash()
    {
        if (CheckSwordConditions()) {OnSlashing?.Invoke();} 
    }

    private bool CheckSwordConditions()
    {
        if (isEquippedSword && !isArmed) {Draw(); return false;}
        else if (!isEquippedSword) {return false;}
        else
        {
            if (Time.time >= lastAttackTime + attackCoolDown)
            {
                lastAttackTime = Time.time;
                return true;
            }
            else
            {return false;}
        }
    }

    private void Draw()
    {
        if (isEquippedSword) 
        {
            isArmed = !isArmed;
            OnArmed?.Invoke(isArmed);
        }
    }


    //Guard Functionality
    private void Guard(bool _isGuarding)
    {
        if (isArmed)
        {
            isGuarding = _isGuarding;
            OnGuarding?.Invoke(isGuarding);
        }
    }
#endregion


#region Pistol
    private void Shoot()
    {
        if (isEquippedPistol)
        {
            OnShooting?.Invoke(shootRotationSpeed,RotateDegreeY);
        }
    }
#endregion



}
