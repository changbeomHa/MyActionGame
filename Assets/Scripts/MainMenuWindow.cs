using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuWindow : MonoBehaviour
{
    public GameObject BackGround;
    public GameObject PopUpWindow;
    bool nowMenuOn = false;
    public AudioSource[] sfx;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (nowMenuOn == false)
            {
                nowMenuOn = true;
                StartCoroutine(PopUp());
                
            }
            else if (nowMenuOn == true)
            {
                sfx[1].Play();
                nowMenuOn = false;
                PopUpWindow.transform.DOScale(new Vector3(0, 0, 0), 0);
                BackGround.SetActive(false);
            }
        }
        if (nowMenuOn)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene("Intro");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SceneManager.LoadScene("TrainingRoom");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SceneManager.LoadScene("BossRoomEnter");
            }
        }
    }

    IEnumerator PopUp()
    {
        sfx[0].Play();
        BackGround.SetActive(true);
        yield return new WaitForSeconds(0f);
        PopUpWindow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);
    }
}
