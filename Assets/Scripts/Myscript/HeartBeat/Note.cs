using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float noteSpeed = 400;
    //UnityEngine.UI.Image noteImage;
    public bool isChecked;

    void OnEnable()
    {
        isChecked = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 캔버스 안에서 움직여야 하므로 localPosition으로 해준다
        transform.localPosition += Vector3.right * noteSpeed * Time.deltaTime;
    }

    public bool GetNoteFlag()
    {
        return isChecked;
    }
}
