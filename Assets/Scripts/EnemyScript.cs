using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemyScript : MonoBehaviour
{
    //Declarations
    private Animator animator;
    private CombatScript playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private CharacterController characterController;
    private TimeStop timestop;

    [Header("스텟")]
    public int health = 3;
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("상태")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    [Header("파티클, 오브젝트")]
    [SerializeField] private ParticleSystem counterParticle;


    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;

    //Events
    public UnityEvent<EnemyScript> OnDamage;
    public UnityEvent<EnemyScript> OnStopMoving;
    public UnityEvent<EnemyScript> OnRetreat;

    void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        timestop = FindObjectOfType<TimeStop>();

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindObjectOfType<CombatScript>();
        enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        playerCombat.OnHit.AddListener((x) => OnPlayerHit(x));
        playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
        playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));

        MovementCoroutine = StartCoroutine(EnemyMovement());

    }

    IEnumerator EnemyMovement()
    {
        // 행동이 할당되지 않을 때 까지 대기
        yield return new WaitUntil(() => isWaiting == true);

        int randomValue = Random.Range(0, 2);

        if (randomValue == 1)
        {
            int randomDir = Random.Range(0, 2);
            // 랜덤방향이 1이면 우측, 아니면 좌측으로 이동
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        }
        else
        {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        MovementCoroutine = StartCoroutine(EnemyMovement());
    }

    void Update()
    {
        // 적은 항상 플레이어를 바라본다
        transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));
        // 방향이 설정되어 있을 때만 이동한다
        MoveEnemy(moveDirection);
    }


    // 피격
    void OnPlayerHit(EnemyScript target)
    {
        if (target == this)
        {
            playerCombat.punchParticle.PlayParticleAtPosition(playerCombat.punchPosition.position);

            StopEnemyCoroutines();
            DamageCoroutine = StartCoroutine(HitCoroutine());

            enemyDetection.SetCurrentTarget(null);
            isLockedTarget = false;
            OnDamage.Invoke(this);

            health--;

            if (health <= 0)
            {
                Death();
                return;
            }

            StartCoroutine(HitStop());

            // boss가 아닌 일반 몹
            if(playerCombat.nowPowerful)
                animator.SetTrigger("CriticalHit");
            else
            {
                if (playerCombat.animationCount == 0 )
                    animator.SetTrigger("RightHit");
                else if (playerCombat.animationCount == 2)
                    animator.SetTrigger("LeftHit");
                else animator.SetTrigger("Hit"); // 발차기는 머리 휘청
            }


            transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);

            StopMoving();
        }
        IEnumerator HitStop()
        {
            yield return new WaitForSeconds(0.3f);
            //timestop.StopTime(0.1f, 10, 0.5f);

        }

        IEnumerator HitCoroutine()
        {
            isStunned = true;
            yield return new WaitForSeconds(.5f);
            isStunned = false;
        }
    }

    void OnPlayerCounter(EnemyScript target)
    {
        if (target == this)
        {
            PrepareAttack(false);
        }
    }

    void OnPlayerTrajectory(EnemyScript target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            isLockedTarget = true;
            PrepareAttack(false);
            StopMoving();
        }
    }

    void Death()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
    }

    // 뒤로 슬금슬금 후퇴한다
    public void SetRetreat()
    {
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            isRetreating = true;
            moveDirection = -Vector3.forward;
            isMoving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, playerCombat.transform.position) > 4);
            isRetreating = false;
            StopMoving();

            //Free 
            isWaiting = true;
            MovementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    public void SetAttack()
    {
        isWaiting = false;

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());

        IEnumerator PrepAttack()
        {
            PrepareAttack(true);
            yield return new WaitForSeconds(.2f);
            moveDirection = Vector3.forward;
            isMoving = true;
        }
    }


    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;

        if (active)
        {
            counterParticle.Play();
        }
        else
        {
            StopMoving();
            counterParticle.Clear();
            counterParticle.Stop();
        }
    }

    void MoveEnemy(Vector3 direction)
    {
        moveSpeed = 1;

        if (direction == Vector3.forward)
            moveSpeed = 5;
        if (direction == -Vector3.forward)
            moveSpeed = 2;

        animator.SetFloat("InputMagnitude", (characterController.velocity.normalized.magnitude * direction.z) / (5 / moveSpeed), .2f, Time.deltaTime);
        animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        if (!isMoving)
            return;

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; // 현재 direction에 수직인 벡터
        Vector3 movedir = Vector3.zero;

        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward)
            finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left)
            finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward)
            finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left)
            moveSpeed /= 1.5f;

        movedir += finalDirection * moveSpeed * Time.deltaTime;

        characterController.Move(movedir);

        if (!isPreparingAttack)
            return;

        if(Vector3.Distance(transform.position, playerCombat.transform.position) < 1 && !playerCombat.death)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
    }
    public int ran;
    private void Attack()
    {
        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        ran = Random.Range(0, 5);
        playerCombat.DamageNumber = ran;
        if(ran == 1)
            animator.SetTrigger("LowKick");
        else animator.SetTrigger("Punch");
    }

    public void HitEvent()
    {
        if(!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
            playerCombat.DamageEvent();

        PrepareAttack(false);
    }

    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        if(characterController.enabled)
            characterController.Move(moveDirection);
    }

    void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if(DamageCoroutine != null)
            StopCoroutine(DamageCoroutine);

        if (MovementCoroutine != null)
            StopCoroutine(MovementCoroutine);
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return health > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return isRetreating;
    }

    public bool IsLockedTarget()
    {
        return isLockedTarget;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    #endregion
}
