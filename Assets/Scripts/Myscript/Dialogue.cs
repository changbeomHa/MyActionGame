using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [Tooltip("캐릭터 명")]
    public string name;

    [Tooltip("대사")]
    public string[] contexts;
}


[System.Serializable]
public class DialogueEvent
{
    public string name; // (장소, 상황등을 적는다) 특별한 기능은 하지 않으나 편리성을 위해 만들어둔다.

    public Vector2 line;
    public Dialogue[] dialogues;
}