using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueParser : MonoBehaviour
{
    public Dialogue[] Parse(string _CSVfileName)
    {
        List<Dialogue> dialogueList = new List<Dialogue>(); // 대사 리스트 생성
        TextAsset csvData = Resources.Load<TextAsset>(_CSVfileName); // csv파일 가져옴

        string[] data = csvData.text.Split(new char[] { '\n' }); // 엔터(줄바꿈)으로 구분 => 한줄씩 가져옴
        
        // 첫번째 줄은 구분하는 칸 이므로 두번째 부터 시작
        // 조건문에서 i가 증가하는 부분은 다른곳에서 만든다.
        for(int i = 1; i < data.Length;)
        {           
            string[] row = data[i].Split(new char[] { ',' }); // ,단위로 쪼개져서 row에 들어가게 된다          

            Dialogue dialogue = new Dialogue(); // 대사 리스트 생성

            dialogue.name = row[1]; // 캐릭터 이름
            List<string> contextList = new List<string>();

            do
            {
                contextList.Add(row[2]);
                if (++i < data.Length)
                {
                    row = data[i].Split(new char[] { ',' });
                }
                else
                {
                    break;
                }
            } while (row[0].ToString() == ""); // 캐릭터 명이 바뀌지 않고 다음 대사로 이어진다면 캐릭터 이름 칸이 공백일 것이다. 이를 검사하는 코드

            dialogue.contexts = contextList.ToArray();

            dialogueList.Add(dialogue);
            // 여기까지 캐릭터명과 대사가 한 세트로 묶여 돌아간다.
        }
         
        return dialogueList.ToArray(); // 배열형태로 변환 후 내보낸다
    }


}
