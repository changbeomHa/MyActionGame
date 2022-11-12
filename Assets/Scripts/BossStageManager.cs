using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossStageManager : MonoBehaviour
{
    public GameObject FadeObj;
    public Image FadeImg;
    public GameObject StageStartObj;
    public GameObject AskRestart;
    public GameObject GameOver;
    public GameObject GameClear;
    bool playerDeath;
    bool nowAskYou;
    public CombatScript player;

    [SerializeField] EnemyScript theBoss;
    [SerializeField] EnemyManager enemyManager;

    public bool EnemySpawn;
    bool bossDeath;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GameStart());
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

        if (player.death && !playerDeath)
        {
            playerDeath = true;
            GameOver.SetActive(true);
        }

        if (EnemySpawn)
        {
            EnemySpawn = false;
            enemyManager.EnemySpawn();
        }

        if(theBoss.health <= 0 && !bossDeath)
        {
            bossDeath = true;
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
        yield return new WaitForSeconds(3f);
        GameClear.SetActive(true);
    }
}
