using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public int bpm = 0;
    double currentTime = 0d;

    [SerializeField] Transform tfNoteAppear = null;


    TimingManager theTM;
    ScoreManager theSM;

    // Start is called before the first frame update
    void Start()
    {
        theTM = GetComponent<TimingManager>();
        theSM = FindObjectOfType<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        // 60 / 120 = 1beat당 0.5초
        // 60s / bpm = 1beat 시간
        if(currentTime >= 60d / bpm)
        {
            GameObject t_note = ObjectPool.instance.objQueue.Dequeue();
            t_note.transform.position = tfNoteAppear.position;
            t_note.SetActive(true);
           
            theTM.boxNoteList.Add(t_note);

            // 미세한 오차의 누적을 방지하기 위해 0으로 초기화가 아닌, 값을 빼주는 형식으로 만들어준다
            currentTime -= 60d / bpm;
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Note"))
        {
            if(collision.GetComponent<Note>().GetNoteFlag() == false) theSM.ResetCombo();

            theTM.boxNoteList.Remove(collision.gameObject);
            ObjectPool.instance.objQueue.Enqueue(collision.gameObject);
            collision.gameObject.SetActive(false);
            
        }
    }
}
