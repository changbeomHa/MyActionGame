using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class BossStageEnter : MonoBehaviour
{
    public GameObject cam1;
    public GameObject cam2;
    public Image FadeImg;
    public Animator playerAnim;
    public GameObject Player;
    public Transform pos;
   

    // Start is called before the first frame update
    void Start()
    {
        FadeImg.CrossFadeAlpha(0, 1f, false);
        StartCoroutine(Delay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(8f);
        cam2.SetActive(true);
        cam1.SetActive(false);
        playerAnim.SetTrigger("go");
        Player.transform.DOMove(pos.transform.position, 8);
        yield return new WaitForSeconds(10f);
        FadeImg.CrossFadeAlpha(1, 1f, false);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MasterRoom");
    }
}
