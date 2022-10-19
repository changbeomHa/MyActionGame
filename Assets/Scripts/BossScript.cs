using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BossScript : MonoBehaviour
{
    private Animator animator;
    private CombatScript playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private CharacterController characterController;
    private TimeStop timestop;

    public ParticleSystem wave;

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

    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;
    private Coroutine MovementCoroutine;

    //Events
    public UnityEvent<BossScript> OnDamage;
    public UnityEvent<BossScript> OnStopMoving;
    public UnityEvent<BossScript> OnRetreat;


    // Start is called before the first frame update
    void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        timestop = FindObjectOfType<TimeStop>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCombat = FindObjectOfType<CombatScript>();
        //enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        StartCoroutine(Pattern());
    }

    private void Update()
    {
        
    }

    IEnumerator Pattern()
    {
        yield return new WaitForSeconds(0.1f);
        int randomPattern = Random.Range(0, 4);
        Debug.Log(randomPattern);

        switch (randomPattern)
        {
            case 0:
                StartCoroutine(BossPunch()); // 펀치
                break;
            case 1:
                StartCoroutine(RockShooting()); //점프 공격
                break;
            case 2:
                StartCoroutine(JumpAttack()); //돌 떨구기
                break;
            case 3:
                StartCoroutine(Breath()); // 브레스
                break;

        }
    }

    IEnumerator BossPunch()
    {
        animator.SetTrigger("BossPunch");
        yield return new WaitForSeconds(5f);
        StartCoroutine(Pattern());
    }
    IEnumerator RockShooting()
    {
        animator.SetTrigger("RockShooting");
        yield return new WaitForSeconds(5f);
        StartCoroutine(Pattern());
    }
    IEnumerator JumpAttack()
    {
        animator.SetTrigger("Jump");
        // 플레이어 위치를 찾고 낙하
        //animator.SetTrigger("Down");
        yield return new WaitForSeconds(4.6f);
        wave.Play();
        yield return new WaitForSeconds(3f);

        StartCoroutine(Pattern());
    }
    IEnumerator Breath()
    {
        animator.SetTrigger("Breath");
        yield return new WaitForSeconds(5f);
        StartCoroutine(Pattern());
    }

}
