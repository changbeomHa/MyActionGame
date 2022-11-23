using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] GameObject FadeObj;
    [SerializeField] GameObject LogoFadeObj;
    [SerializeField] Image FadeImg;

    // Start is called before the first frame update
    void Start()
    {   
        StartCoroutine(FadeObjDown());
    }
    IEnumerator FadeObjDown()
    {
        FadeImg.CrossFadeAlpha(0f, 1.5f, false);
        yield return new WaitForSeconds(2.5f);
        FadeImg.CrossFadeAlpha(1f, 1.5f, false);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Intro");
    }

}