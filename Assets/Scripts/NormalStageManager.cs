using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NormalStageManager : MonoBehaviour
{
    [Header("������Ʈ")]
    public GameObject FadeObj;
    public GameObject StageStartObj;
    public GameObject AskRestart;
    public GameObject GameOver;
    public GameObject GameClear;
    public GameObject GameClearText;
    public Image FadeImg;
    bool nowAskYou;
    public GameObject BGM;

    [SerializeField] EnemyManager enemyManager;
    [SerializeField] GameObject EnemyObj;
    public bool stageStart;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GameStart());
        BGM = FindObjectOfType<SceneManagement>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AskRestart.SetActive(true);
            nowAskYou = true;
        }
        if (nowAskYou)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                nowAskYou = false;
                AskRestart.SetActive(false);
            }
        }

        if (stageStart)
        {
            stageStart = false;
            enemyManager.EnemySpawn();
        }

        if(enemyManager.AliveEnemyCount() == 0)
        {
            StartCoroutine(GameClearCoroutine());
        }
    }

    IEnumerator GameStart()
    {
        FadeImg.CrossFadeAlpha(0, 1f, false);
        yield return new WaitForSeconds(1f);
        StageStartObj.SetActive(true);
        yield return new WaitForSeconds(3f);
        StageStartObj.SetActive(false);
        FadeObj.SetActive(false);
    }

    IEnumerator GameClearCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameClear.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        GameClearText.SetActive(true);
        yield return new WaitForSeconds(3f);
        Destroy(BGM);
        SceneManager.LoadScene("BossRoomEnter");
    }
}
