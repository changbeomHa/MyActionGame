using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingManager : MonoBehaviour
{
    public List<GameObject> boxNoteList = new List<GameObject>();

    [SerializeField] Transform Center = null;
    [SerializeField] RectTransform[] timingRect = null; // 판정범위 (좋음, 나쁨 등)
    Vector2[] timingBoxs = null; // 판정 범위의 최소값x, 최대값 y

    AudioSource theAudio;
    NoteEffectManager theEM;    
    ScoreManager theSM;

    // Start is called before the first frame update
    void Start()
    {
        theSM = FindObjectOfType<ScoreManager>();
        theAudio = GetComponent<AudioSource>();
        theEM = FindObjectOfType<NoteEffectManager>();

        // 타이밍 박스 설정, 0번째가 가장 좁고(perfect) 마지막이 가장 넓다(bad)
        timingBoxs = new Vector2[timingRect.Length];

        // 판정 범위를 for문으로 넣어준다.
        for(int i = 0; i < timingRect.Length; i++)
        {
            // 판정범위 => 최소값 = 중심 - (이미지의 너비 / 2), 최대값 = (중심 + 이미지의 너비 / 2)
            timingBoxs[i].Set(Center.localPosition.x - timingRect[i].rect.width / 2, Center.localPosition.x + timingRect[i].rect.width / 2);
        }
    }

    public void CheckTiming()
    {
        for(int i = 0; i < boxNoteList.Count; i++)
        {
            float t_notePosX = boxNoteList[i].transform.localPosition.x;

            for(int x = 0; x < timingBoxs.Length; x++)
            {
                if(timingBoxs[x].x <= t_notePosX && t_notePosX <= timingBoxs[x].y)
                {                   
                    // 타이밍을 맞춘 노트는 알파값이 줄어든다
                    boxNoteList[i].GetComponent<Image>().CrossFadeAlpha(0.5f, 0, false);

                    if (x == 0)
                    {
                        boxNoteList[i].GetComponent<Note>().isChecked = true; // perfect라면 isChecked를 true로 만들어 콤보상태를 준비한다
                    }
                    else theSM.ResetCombo();

                    if (x < timingBoxs.Length - 1) theEM.NoteHitEffect(); // perfect, cool, normal까지만 효과가 나타난다
                    boxNoteList.RemoveAt(i); // list에서 사라진다
                    
                    // 판정 효과
                    theAudio.Play();
                    theSM.IncreaseScore(x);
                  
                    return;
                }
            }

        }
        //theSM.ResetCombo();
        Debug.Log("Miss");
    }
}
