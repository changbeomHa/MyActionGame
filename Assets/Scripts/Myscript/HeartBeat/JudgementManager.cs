using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementManager : MonoBehaviour
{
    TimingManager theTM;
    // Start is called before the first frame update
    void Start()
    {
        theTM = FindObjectOfType<TimingManager>();     
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            theTM.CheckTiming();
        }
    }
}
