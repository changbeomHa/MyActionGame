using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeStop : MonoBehaviour
{
    private float Speed;
    private bool RestoreTime;
    // Start is called before the first frame update
    void Start()
    {
        RestoreTime = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (RestoreTime)
        {
            if(Time.timeScale < 1f)
            {
                Time.timeScale += Time.deltaTime * Speed;
            }
            else
            {
                Time.timeScale = 1f;
                RestoreTime = false;
            }
        }
    }

    public void StopTime(float changeTime, int restoreSpeed, float delay)
    {
        Speed = restoreSpeed;

        if(delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            RestoreTime = true;
        }

        Time.timeScale = changeTime;
    }
    IEnumerator StartTimeAgain(float amt)
    {
        RestoreTime = true;
        yield return new WaitForSeconds(amt);
    }
}
