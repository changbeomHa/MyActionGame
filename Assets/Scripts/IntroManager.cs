using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    public Image FadeImage;
    public GameObject Player;
    public Animator playerAnimation;
    public Animator IntroAnimation;
    public Text holoInterfaceText;
    public DialogueManager theDM;
    public InteractionEvent interactionEvent;
    public GameObject otherDialogue;

    [Header("카메라")]
    public GameObject cam1;
    public GameObject cam2;
    public GameObject cam3;

    [Header("이동 위치")]
    public Transform pos1;
    public Transform pos2;

    [Header("변수")]
    [SerializeField] bool check1;
    [SerializeField] bool check2;
    [SerializeField] bool check3;
    int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        FadeImage.CrossFadeAlpha(0, 1f, false);
        theDM.ShowDIalogue(interactionEvent.GetDialogue());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            i++;          
        }
        if(i == 2)
        {            
            holoInterfaceText.text = "입장".ToString();
            if(check1 == false)
            {
                check1 = true;                
                StartCoroutine(FadeDelay());
            }        
        }
    }

    IEnumerator FadeDelay()
    {
        playerAnimation.SetTrigger("StandUp");
        yield return new WaitForSeconds(1f);
        FadeImage.CrossFadeAlpha(1, 2f, false);
        yield return new WaitForSeconds(2.5f);
        Player.transform.position = pos1.position;
        Player.transform.LookAt(pos2);
        cam2.SetActive(true);
        cam1.SetActive(false);
        yield return new WaitForSeconds(.5f);
        FadeImage.CrossFadeAlpha(0, 1f, false);
        playerAnimation.SetBool("Walk", true);
        Player.transform.DOMove(pos2.transform.position, 4);
        yield return new WaitForSeconds(1.5f);
        cam3.SetActive(true);
        cam2.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        playerAnimation.SetBool("Stop", true);
        yield return new WaitForSeconds(2f);
        theDM.ShowDIalogue(otherDialogue.GetComponent<InteractionEvent>().GetDialogue());
        yield return new WaitUntil(() => theDM.isDialogue == false);
        IntroAnimation.SetTrigger("Go");
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("TutorialRoom");
    }
}
