using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;

public class BossScript : MonoBehaviour
{
    //private Animator animator;
    public CinemachineImpulseSource impulseSource;
    public Animator animator;
    public CombatScript playerCombat;
    public EnemyScript enemyScript;
    public Slider BossHPSlider;
    public BossStageManager stageManager;
    public EnemyManager theEM;

    public ParticleSystem wave;

    public AudioSource[] SFX;

    [Header("패턴1 오브젝트")]
    public Transform blockPosition1;
    public Transform blockPosition2;
    public Transform blockPosition3;
    public Transform blockPosition4;
    public GameObject block;
    public GameObject Fakeblock;
    public GameObject punchLineParticle1;
    public GameObject punchLineParticle2;
    public GameObject punchLineParticle3;
    public GameObject punchLineParticle4;

    [Header("패턴2 오브젝트")]
    public GameObject BossLandPosition;
    public GameObject BossLandingRange;


    [Header("패턴3 오브젝트")]
    [SerializeField] Rigidbody Rock;
    [SerializeField] Transform RockPos;
    Rigidbody ThrowingRock;
    Vector3 vo;
    Vector3 position;
    private Vector3 posUp;
    


    [Header("패턴4 오브젝트")]
    public GameObject BeamCharging;
    bool Beaming = false;
    public Transform BeamStartPoint;
    public Transform BeamEndPoint;
    public GameObject beamLineRendererPrefab;
    public GameObject beamStartPrefab;
    public GameObject beamEndPrefab;
    private GameObject beamStart;
    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture
    private float beamDamage;
    private int prevcount = 0;

    // Start is called before the first frame update
    void Start()
    {
        enemyScript = FindObjectOfType<EnemyScript>();
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        animator = GetComponent<Animator>();
        playerCombat = FindObjectOfType<CombatScript>();
        //enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();
        //playerCombat.OnHit.AddListener((x) => OnPlayerHit(x));
        StartCoroutine(Pattern());
    }

    private void Update()
    {
        BossHPSlider.value = enemyScript.health;
        if (Beaming)
        {
            beamDamage += Time.deltaTime;
            //Debug.Log(Mathf.RoundToInt(beamDamage));
            //Ray ray = Camera.main.ScreenPointToRay(playerCombat.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(BeamEndPoint.position, BeamEndPoint.position, out hit))
            {
                Vector3 tdir = hit.point - BeamStartPoint.position;
                
                ShootBeamInDir(BeamStartPoint.position, tdir);
            }
        }


 
    }


    IEnumerator Pattern()
    {
        yield return new WaitForSeconds(0.1f);
        //int randomPattern = 0;
        int randomPattern;

        if(theEM.aliveEnemyCount > 1)
        {
            randomPattern = Random.Range(1, 4);
        }
        else randomPattern = Random.Range(0, 4);
        Debug.Log(randomPattern);

        switch (randomPattern)
        {
            case 0:
                StartCoroutine(BossPunch()); // 펀치
                break;
            case 1:
                StartCoroutine(RockShooting()); //돌 던지기
                break;
            case 2:
                StartCoroutine(JumpAttack()); //점프 공격
                break;
            case 3:
                StartCoroutine(Beam()); // 레이저 빔
                break;

        }
    }

    IEnumerator BossPunch()
    {
        animator.SetTrigger("BossPunch");       
        yield return new WaitForSeconds(2.2f);
        SFX[0].Play();
        impulseSource.GenerateImpulse();
        punchLineParticle1.transform.LookAt(blockPosition1);
        punchLineParticle1.GetComponent<ParticleSystem>().Play();
        punchLineParticle2.transform.LookAt(blockPosition2);
        punchLineParticle2.GetComponent<ParticleSystem>().Play();
        punchLineParticle3.transform.LookAt(blockPosition3);
        punchLineParticle3.GetComponent<ParticleSystem>().Play();
        punchLineParticle4.transform.LookAt(blockPosition4);
        punchLineParticle4.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(.3f);      
        Instantiate(block, blockPosition1);
        Instantiate(Fakeblock, blockPosition2);
        Instantiate(Fakeblock, blockPosition3);
        Instantiate(Fakeblock, blockPosition4);
        yield return new WaitForSeconds(6f);
        StartCoroutine(Pattern());
    }
    IEnumerator RockShooting()
    {
        animator.SetTrigger("RockShooting");
        yield return new WaitForSeconds(.5f);
        ThrowingRock = Instantiate(Rock, RockPos.position, Quaternion.identity);
        ThrowingRock.transform.DOScale(new Vector3(.75f, .75f, .75f), 0.1f);
        yield return new WaitForSeconds(.5f);
        ThrowingRock.transform.DOScale(new Vector3(1.25f, 1.25f, 1.25f), 0.1f);
        yield return new WaitForSeconds(1f);
        transform.LookAt(playerCombat.transform);
        vo = CalculateVelocty(playerCombat.transform.position, RockPos.position, 1f);
        ThrowingRock.transform.position = RockPos.position;
        ThrowingRock.velocity = vo; // 돌을 던진다
        yield return new WaitForSeconds(5f);

        StartCoroutine(Pattern());
    }

    // 생성지점(origin)부터 목표지점(target)까지 계산
    Vector3 CalculateVelocty(Vector3 target, Vector3 origin, float time)
    {
        Vector3 distance = target - origin;
        Vector3 distanceXz = distance;
        distanceXz.y = 0f;

        float sY = distance.y;
        float sXz = distanceXz.magnitude;

        float Vxz = sXz * time;
        float Vy = (sY / time) + (0.5f * Mathf.Abs(Physics.gravity.y) * time);

        Vector3 result = distanceXz.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    IEnumerator JumpAttack()
    {
        animator.SetTrigger("Jump");
        // 플레이어 위치를 찾고 낙하
        //animator.SetTrigger("Down");
        yield return new WaitForSeconds(3.5f);
        BossLandPosition.transform.position = playerCombat.transform.position;
        BossLandPosition.transform.DOScale(new Vector3(.15f, .15f, .15f), .5f);
        transform.position = playerCombat.transform.position;
        BossLandPosition.SetActive(true);
        yield return new WaitForSeconds(1.1f);
        BossLandPosition.transform.DOScale(new Vector3(0f, 0f, 0f), 0.1f);
        BossLandPosition.SetActive(false);
        BossLandingRange.SetActive(true);
        SFX[1].Play();
        wave.Play();
        impulseSource.GenerateImpulse();
        yield return new WaitForSeconds(.2f);
        BossLandingRange.SetActive(false);
        yield return new WaitForSeconds(2.8f);

        StartCoroutine(Pattern());
    }
    IEnumerator Beam()
    {
        beamDamage = 0;
        transform.LookAt(playerCombat.transform);
        animator.SetTrigger("Beam");
        //SFX[2].Play();
        BeamCharging.SetActive(true);
        yield return new WaitForSeconds(2f);
        BeamCharging.SetActive(false);
        transform.LookAt(playerCombat.transform);
        //BeamEndPoint.position = playerCombat.transform.position;
        beamStart = Instantiate(beamStartPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beamEnd = Instantiate(beamEndPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beam = Instantiate(beamLineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();
        Beaming = true;
        float timer = 0.0f;
        while (timer <= 5)
        {
            timer += Time.deltaTime;
            yield return null;
            Quaternion targetRotation = Quaternion.LookRotation(playerCombat.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
        }
        yield return new WaitForSeconds(.1f);
        Destroy(beamStart);
        Destroy(beamEnd);
        Destroy(beam);
        Beaming = false;
        StartCoroutine(Pattern());
    }
    void ShootBeamInDir(Vector3 start, Vector3 dir)
    {
        line.positionCount = 2;
        line.SetPosition(0, start);
        beamStart.transform.position = start;

        Vector3 end = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit))
        {
            end = hit.point - (dir.normalized * beamEndOffset);
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log(Mathf.RoundToInt(beamDamage));
                if(prevcount != Mathf.RoundToInt(beamDamage))
                {
                    playerCombat.health -= Mathf.RoundToInt(beamDamage) / Mathf.RoundToInt(beamDamage);
                    prevcount = Mathf.RoundToInt(beamDamage);
                }               
            }
        }           
        else
            end = transform.position + (dir * 100);

        beamEnd.transform.position = end;
        line.SetPosition(1, end);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, end);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }

}
