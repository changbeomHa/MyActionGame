using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;

public class CombatScript : MonoBehaviour
{
    private ScoreManager heartbeatRate;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private MovementInput movementInput;
    private Animator animator;
    public CinemachineImpulseSource impulseSource;
    public HitBox[] hitBox;

    [Header("Target")]
    private EnemyScript lockedTarget;

    [Header("����")]
    [SerializeField] private float attackCooldown;
    public int health = 10;
    
    [Header("����")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;
    public bool nowPowerful = false;
    public bool Attackable = true;
    public bool death = false;

    [Header("���۷���")]
    [SerializeField] public Transform punchPosition;
    [SerializeField] public ParticleSystemScript punchParticle;
    [SerializeField] public GameObject StunParticle;
    [SerializeField] public Image AwakeningOK;
    [SerializeField] private GameObject criticalHitCamera;
    [SerializeField] private Transform criticalHitFocusObject;
    [SerializeField] private GameObject lastHitCamera;
    [SerializeField] private Transform lastHitFocusObject;

    //�ڷ�ƾ
    private Coroutine counterCoroutine;
    private Coroutine attackCoroutine;
    private Coroutine damageCoroutine;

    [Space]

    //�̺�Ʈ
    public UnityEvent<EnemyScript> OnTrajectory;
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;

    // �ִϸ��̼� ����
    public int animationCount = 0;
    string[] attackMotions;



    void Start()
    {
        heartbeatRate = FindObjectOfType<ScoreManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
    }

    // ���� Ű�� ȣ���� �� ����
    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;

        // �� ���� �ý��ۿ� ���� �����Ǿ��ִ°�
        if (enemyDetection.CurrentTarget() == null)
        {
            if (enemyManager.AliveEnemyCount() == 0)
            {
                Attack(null, 0);
                return;
            }
            else
            {
                lockedTarget = enemyManager.RandomEnemy();
            }
        }

        // �÷��̾ �̵��ϴ� ��� �̵������� �����Ͽ� Ÿ���� �����Ѵ�.
        if (enemyDetection.InputMagnitude() > .2f)
            lockedTarget = enemyDetection.CurrentTarget();

        // ���� Ÿ���� ���ٸ� �������� ���� ����
        if(lockedTarget == null)
            lockedTarget = enemyManager.RandomEnemy();

        Attack(lockedTarget, TargetDistance(lockedTarget));
    }

    public void Attack(EnemyScript target, float distance)
    {
        // ���⿡ ���� �ִϸ��̼� �������� ���´�.
        attackMotions = new string[] { "Left jab", "CrescentKick", "Right Punch", "MmaKick" };

        if (distance < 4 && Attackable)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attackMotions.Length);
            string attackString = isLastHit()&&distance<2 ? attackMotions[Random.Range(0, attackMotions.Length)] : attackMotions[animationCount];
            AttackType(attackString, attackCooldown, target, .65f);
        }

        if (lockedTarget.IsPreparingAttack())
            Debug.Log("counter");

        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = Mathf.Max(3, 1 * distance);

    }

    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(isLastHit() ? 1.5f : cooldown));

        // ��Ʈ ���� üũ
        if (isLastHit())
            StartCoroutine(FinalBlowCoroutine());


        if (target == null)
            return;

        target.StopMoving();
        MoveTorwardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration)
        {
            movementInput.acceleration = 0;
            isAttackingEnemy = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }

        IEnumerator FinalBlowCoroutine()
        {
            Time.timeScale = .5f;
            lastHitCamera.SetActive(true);
            lastHitFocusObject.position = lockedTarget.transform.position;
            yield return new WaitForSecondsRealtime(2);
            lastHitCamera.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // ���ݽ� �׻� ���� �ٶ󺻴�.
    void MoveTorwardsTarget(EnemyScript target, float duration)
    {
        OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .2f);
        ControlManager.targetVector = (target.transform.position - transform.position).normalized;
        ControlManager.targetVector -= new Vector3(0, ControlManager.targetVector.y, 0);
    }

    // ī���� ����
    void CounterCheck()
    {   
        //Initial check
        if (isCountering || isAttackingEnemy || !enemyManager.AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack.Invoke(lockedTarget);

        if (TargetDistance(lockedTarget) > 2)
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        float duration = .2f;
        animator.SetTrigger("Dodge");
        Debug.Log("Counter");
        transform.DOLookAt(lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if (counterCoroutine != null)
            StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float duration)
        {
            isCountering = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;

        }
    }

    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    public void HitEvent()
    {
        if (lockedTarget == null || enemyManager.AliveEnemyCount() == 0)
            return;

        if(hitBox[0].triggerCheck || hitBox[1].triggerCheck || hitBox[2].triggerCheck || hitBox[3].triggerCheck)
        {
            if(!nowPowerful)
                OnHit.Invoke(lockedTarget);
            else
            {
                OnHit.Invoke(lockedTarget);
                StartCoroutine(CriticalBlowCoroutine());
            }
        }


    }
    IEnumerator CriticalBlowCoroutine()
    {
        Time.timeScale = .1f;
        criticalHitCamera.SetActive(true);
        criticalHitFocusObject.position = lockedTarget.transform.position;
        yield return new WaitForSecondsRealtime(1);
        criticalHitCamera.SetActive(false);
        nowPowerful = false;
        AwakeningOK.color = Color.white;
        heartbeatRate.awakeningCombo = 0;
        heartbeatRate.currentHeartRate = 73;
        heartbeatRate.txtScore.text = heartbeatRate.currentHeartRate.ToString();
        Time.timeScale = 1f;
    }
    public int DamageNumber;
    // �ǰ�
    public void DamageEvent()
    {
        if(DamageNumber == 1)
            animator.SetTrigger("FallDown");
        else
            animator.SetTrigger("Hit");

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());
        health--;

        if (health <= 0)
        {
            death = true;
            Death();
            return;
        }
        IEnumerator DamageCoroutine()
        {
            if (!Attackable) // ���ϻ��¶��
            {
                // �׳� �������� ����
            }
            else
            {
                movementInput.enabled = false;
                yield return new WaitForSeconds(.5f);
                movementInput.enabled = true;
                LerpCharacterAcceleration();
            }
            
        }
    }
    public void Death()
    {
        Attackable = false;
        movementInput.enabled = false;
        animator.SetTrigger("Death");
    }

    public void Stunned()
    {
    
        StartCoroutine(StunCoroutine());
        
        IEnumerator StunCoroutine()
        {
            Attackable = false;
            StunParticle.SetActive(true);
            animator.SetTrigger("Stunned");
            movementInput.enabled = false;
            yield return new WaitForSeconds(2f);
            StunParticle.SetActive(false);
            yield return new WaitForSeconds(1f);
            movementInput.enabled = true;
            Attackable = true;
            LerpCharacterAcceleration();
        }
    }

    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = 100;
        int finalIndex = 0;

        for (int i = 0; i < enemyManager.allEnemies.Length; i++)
        {
            EnemyScript enemy = enemyManager.allEnemies[i].enemyScript;

            if (enemy.IsPreparingAttack())
            {

                if (Vector3.Distance(transform.position, enemy.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return enemyManager.allEnemies[finalIndex].enemyScript;

    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

    bool isLastHit()
    {
        if (lockedTarget == null)
            return false;

        return enemyManager.AliveEnemyCount() == 1 && lockedTarget.health <= 1;
    }

    bool isCriticalHit()
    {
        if (lockedTarget == null)
            return false;

        // �ɹڼ� ����
        return true;
    }

    #region Input

    private void OnCounter()
    {
        CounterCheck();
    }

    private void OnAttack()
    {
        AttackCheck();
    }

    #endregion

}
