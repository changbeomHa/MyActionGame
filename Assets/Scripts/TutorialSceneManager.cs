using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSceneManager : MonoBehaviour
{
    public Animator anim;
    public string[] dialogue;
    public TextMeshProUGUI text;
    int i = 0;
    bool check = true;
    public GameObject PointUI;
    public GameObject LineUI;
    public GameObject GameStart;
    public GameObject wall;
    public DialogueManager theDM;
    public InteractionEvent interactionEvent;
    public AudioSource BGM;
    public EnemyScript enemy;
    public GameObject NoticeUI;
    public TextMeshProUGUI noticeText;
    public GameObject BackGroundImg;
    public GameObject CinematicCameraMove;
    public GameObject PlayerCamera;
    public GameObject UpFog;

    // Start is called before the first frame update
    void Start()
    {
        CinematicCameraMove.SetActive(true);
        BackGroundImg.GetComponent<Image>().rectTransform.SetAsFirstSibling();
        BGM.Play();
        LineUI.SetActive(true);
        
        theDM.ShowDIalogue(interactionEvent.GetDialogue());
    }

    // Update is called once per frame
    void Update()
    {

        if (i < 12)
        {
            //text.text = dialogue[i].ToString();
            if (Input.GetMouseButtonDown(0))
            {
                i++;
                //text.text = dialogue[i].ToString();
                if (i == 1)
                {
                    PointUI.SetActive(true);
                    anim.SetTrigger("1");
                }
                if (i == 2) anim.SetTrigger("2");
                if (i == 7) anim.SetTrigger("3");
                if (i == 11 && check == true)
                {
                    StartCoroutine(GameStartCoRoutine());
                }
            }
        }

        if (enemy.health == 5)
        {
            NoticeUI.SetActive(true);
            
        }
        else if (enemy.health == 4)
        {
            ShieldCheck = true;
        }
        else if (enemy.health < 4)
        {
            NoticeUI.SetActive(false);
        }
    }

    IEnumerator Fade()
    {
        BackGroundImg.GetComponent<Image>().CrossFadeAlpha(0, 0.5f, false);
        yield return new WaitForSeconds(0.5f);
        BackGroundImg.SetActive(false);
    }

    IEnumerator GameStartCoRoutine()
    {
        CinematicCameraMove.SetActive(false);
        PlayerCamera.SetActive(true);
        UpFog.SetActive(false);
        StartCoroutine(Fade());
        check = false;
        PointUI.SetActive(false);
        GameStart.SetActive(true);
        LineUI.SetActive(false);
        yield return new WaitForSeconds(2f);
        wall.SetActive(false);
    }


    public bool ShieldCheck
    {
        get
        {
            return enemy.hasShield;
        }
        set
        {
            // 저장된 값, 현재 값(value)을 비교하여 변화 감지
            if (enemy.hasShield != value)
            {
                noticeText.text = "훌륭합니다! 치명타 공격이 적중하면 심박수가 3 증가합니다. 심박수를 잘 관리하세요.".ToString();
            }

            //enemy.hasShield = value;
        }

    }
}
