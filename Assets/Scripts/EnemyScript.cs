using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemyScript : MonoBehaviour
{
    //Declarations
    protected Animator animator;
    protected CombatScript playerCombat;
    protected EnemyManager enemyManager;
    protected EnemyDetection enemyDetection;
    private CharacterController characterController;
    protected TimeStop timestop;
    public GameObject shield;
    public bool hasShield = false;
    public AudioSource enemySFX;

    [Header("����")]
    public int health = 10;
    private float moveSpeed = 1;
    private Vector3 moveDirection;
    public bool theBOSS;
    public bool mini_boss;

    [Header("����")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    [Header("��ƼŬ, ������Ʈ")]
    [SerializeField] private ParticleSystem counterParticle;


    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;

    //Events
    public UnityEvent<EnemyScript> OnDamage;
    public UnityEvent<EnemyScript> OnStopMoving;
    public UnityEvent<EnemyScript> OnRetreat;

    float timecheck = 0;
    float stuncheck = 0;

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
        if(!theBOSS)
            MovementCoroutine = StartCoroutine(EnemyMovement());

    }

    IEnumerator EnemyMovement()
    {
        // �ൿ�� �Ҵ���� ���� �� ���� ���
        yield return new WaitUntil(() => isWaiting == true);

        int randomValue = Random.Range(0, 2);

        if (randomValue == 1)
        {
            int randomDir = Random.Range(0, 2);
            // ���������� 1�̸� ����, �ƴϸ� �������� �̵�
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
        // ���� �׻� �÷��̾ �ٶ󺻴�
        if(!theBOSS)
            transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));
        // ������ �����Ǿ� ���� ���� �̵��Ѵ�
        if(!theBOSS)
            MoveEnemy(moveDirection);
        if (!theBOSS && isWaiting || isRetreating)
        {
            timecheck += Time.deltaTime;
            if (timecheck > 3)
            {
                SetAttack();
                timecheck = 0;
            }
        }
        else timecheck = 0;

        if(!theBOSS && isStunned)
        {
            stuncheck += Time.deltaTime;
            if(stuncheck > 2) {
                Attack();
                stuncheck = 0;
            }
        }
    }


    // �ǰ�
    protected void OnPlayerHit(EnemyScript target)
    {
        if (target == this)
        {
            Debug.Log("hit");
            playerCombat.punchParticle.PlayParticleAtPosition(playerCombat.punchPosition.position);
            if(!theBOSS)
                StopEnemyCoroutines();
            DamageCoroutine = StartCoroutine(HitCoroutine());

            enemyDetection.SetCurrentTarget(null);
            //isLockedTarget = false;
            OnDamage.Invoke(this);

            if (!hasShield)
            {
                if (playerCombat.nowPowerful)
                    health -= 2;
                else
                    health--;
            }
            else
            {
                if (playerCombat.nowPowerful)
                {
                    hasShield = false;
                    shield.SetActive(false);
                    health--;
                }
            }

            if(health % 5 == 0 && health != 0)
            {
                hasShield = true;
                shield.SetActive(true);
            }

            if (health <= 0)
            {
                StartCoroutine(Death());
                return;
            }

            

            // boss�� �ƴ� �Ϲ� ��
            if (!theBOSS && !hasShield)
            {
                if (playerCombat.nowPowerful)
                    animator.SetTrigger("CriticalHit");
                else
                {
                    if (playerCombat.animationCount == 0)
                        animator.SetTrigger("RightHit");
                    else if (playerCombat.animationCount == 2)
                        animator.SetTrigger("LeftHit");
                    else animator.SetTrigger("Hit"); // ������� �Ӹ� ��û
                }

                transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);
            }
            if(!playerCombat.nowPowerful)
                StartCoroutine(HitStop());
            if (!theBOSS)
                StopMoving();
        }
        IEnumerator HitStop()
        {
            //Time.timeScale = 0.1f;
            yield return new WaitForSeconds(0.2f);
            //Time.timeScale = 1f;
            timestop.StopTime(0.1f, 10, 1f);

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

    IEnumerator Death()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        characterController.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
        yield return new WaitForSeconds(3f);
        if (mini_boss)
        {
            this.gameObject.SetActive(false);
            //enemyManager.AliveEnemyCount();
            //enemyManager.EnemySpawn();
        }
        else Destroy(gameObject);
    }

    // �ڷ� ���ݽ��� �����Ѵ�
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
        if(!playerCombat.death)
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
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; // ���� direction�� ������ ����
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
            if (!playerCombat.isAttackingEnemy)
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
        enemySFX.Play();
    }

    public void HitEvent()
    {
        //if(!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
            playerCombat.DamageEvent();

        PrepareAttack(false);
    }

    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        if(characterController.enabled)
            characterController.Move(moveDirection);
        // �ٽ� �����̴� �ڵ带 �ִ´�.
        //StartCoroutine(EnemyMovement()); // �̰� ������ ���ϰɸ�
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
