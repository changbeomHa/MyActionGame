using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class AttackController : MonoBehaviour
{
    private Animator animator;
    private CinemachineImpulseSource impulseSource;
    private EnemyDetection enemyDetection;


    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private int damage = 10;

    [Header("Public References")]
    [SerializeField] private Transform punchPosition;
    [SerializeField] private ParticleSystemScript punchParticle;
    [SerializeField] private GameObject lastHitCamera;
    [SerializeField] private Transform lastHitFocusObject;

    [Header("Target")]
    private EnemyScript lockedTarget;
    string[] attacks;

    // Start is called before the first frame update
    void Start()
    {
        enemyDetection = GetComponentInChildren<EnemyDetection>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
        }
    }
    public void Attack(EnemyScript target, float distance)
    {
        //Types of attack animation
        // attacks = new string[] { "GroundPunch", "Left jab", "CrescentKick", "MmaKick" }; // animationController Jammo의 경우 AirKick1, 3 추가 
        attacks = new string[] { "Left jab" };



        //Change impulse
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = Mathf.Max(3, 1 * distance);

    }
    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }
}
