using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    //[SerializeField] UIManager theUI;
    [SerializeField] GameObject go_DialogueBar;

    [SerializeField] TextMeshProUGUI txt_Dialogue;
    //[SerializeField] TextMeshProUGUI txt_Name;

    Dialogue[] dialogues;

    bool isDialogue = false; // 대화중일 경우 true.
    bool isNext = false; // 다음 문장 까지 대기

    [Header("텍스트 출력 딜레이")]
    [SerializeField] float textDelay;


    int lineCount = 0; // 대화 카운트.
    int contextCount = 0; // 대사 카운트.

    private void Update()
    {
        if (isDialogue)
        {
            if (isNext)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    isNext = false;
                    txt_Dialogue.text = "";
                    if (++contextCount < dialogues[lineCount].contexts.Length)
                    {
                        StartCoroutine(TypeWriter());
                    }
                    else
                    {
                        contextCount = 0;
                        if(++lineCount < dialogues.Length)
                        {
                            StartCoroutine(TypeWriter());
                        }
                        else
                        {
                            EndDialogue();
                        }
                    }
                }
            }
        }
    }

    public void ShowDIalogue(Dialogue[] p_dialogues)
    {
        isDialogue = true;
        txt_Dialogue.text = "";
        //txt_Name.text = "";
        //theUI.SetUI(false);
        dialogues = p_dialogues;
        StartCoroutine(TypeWriter());

    }

    void EndDialogue()
    {
        isDialogue = false;
        contextCount = 0;
        lineCount = 0;
        dialogues = null;
        isNext = false;
        //theUI.SetUI(true);
        SettingUI(false);
    }

    IEnumerator TypeWriter()
    {
        SettingUI(true);
        string t_ReplaceText = dialogues[lineCount].contexts[contextCount];
        t_ReplaceText = t_ReplaceText.Replace("`", ",");

        //txt_Name.text = dialogues[lineCount].name; // 화자 이름
        for(int i = 0;i < t_ReplaceText.Length; i++)
        {
            txt_Dialogue.text += t_ReplaceText[i];
            yield return new WaitForSeconds(textDelay);
        }

        isNext = true;
    }


    // 후에 이걸 애니메이션으로 만들어주자
    void SettingUI(bool p_flag)
    {
        go_DialogueBar.SetActive(p_flag);
    }

}
