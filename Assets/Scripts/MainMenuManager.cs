using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        FadeImg.CrossFadeAlpha(0f, 2f, false);
        yield return new WaitForSeconds(2f);
        FadeImg.CrossFadeAlpha(1f, 2f, false);
        yield return new WaitForSeconds(3f);
        FadeObj.SetActive(false);
        LogoFadeObj.SetActive(false);
    }

}