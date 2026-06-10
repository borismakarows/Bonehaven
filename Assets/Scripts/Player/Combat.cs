using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Combat : MonoBehaviour
{
#region Conditions
    private bool isEquippedSword;
    private bool isDrawedSword;
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
    public static event Action<bool> OnDrawing;
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
        StarterAssetsInputs.OnSlashFired += Slash;
        StarterAssetsInputs.OnWithdrawFired += Draw;
        StarterAssetsInputs.OnShootFired += Shoot;
        StarterAssetsInputs.OnGuardFired += Guard;
    }

    private void UnsubscireEvents()
    {
        StarterAssetsInputs.OnSlashFired -= Slash;
        StarterAssetsInputs.OnWithdrawFired -= Draw;
        StarterAssetsInputs.OnShootFired -= Shoot;
        StarterAssetsInputs.OnGuardFired -= Guard;
    }
#endregion

#region Sword
    private void Slash()
    {
        if (CheckSwordConditions()) {OnSlashing?.Invoke();} 
    }

    private bool CheckSwordConditions()
    {
        if (isEquippedSword && !isDrawedSword) {Draw(); return false;}
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
            OnDrawing?.Invoke(isDrawedSword);
            isDrawedSword = !isDrawedSword;
        }
    }

    //Guard Functionality
    private void Guard(bool _isGuarding)
    {
        if (isDrawedSword)
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
