using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public enum EnemyType
{
    Melee,
    Ranger
}

[CreateAssetMenu(fileName = "AgentSettings", menuName = "AgentSettings/New")]
public class AgentSettings : ScriptableObject
{
    [Header("Agent Info")]
    public EnemyType enemyType;

    [Header("Parameters")]
    public int patrolStartRatio;
    [Range(4,7)]
    public float rangeVisDist;
    [Range(1,3)]
    public  float meleeVisDist;
    [Range(25,45)]
    public float visAng = 30f;
    [Range(2,5)]
    public float shootDist;
    [Range(1,2)]
    public float swordDist = 2f;
    public float rotationSpeed = 2f;

    public float shootAimAngle = 15f;

    [Header("Objects")] 
    public GameObject sword;
    public GameObject pistol;

    [Header("Conditions")]
    public bool isEquipped;
}
