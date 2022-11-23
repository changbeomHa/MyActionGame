using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ForVideo : MonoBehaviour
{

    public GameObject cam;
    public Image Fadeimg;
    public Transform pos;
    // Start is called before the first frame update
    void Start()
    {
        Fadeimg.CrossFadeAlpha(0, 0f, false);
        cam.transform.DOMove(pos.position, 4f).SetEase(Ease.InQuad);
        StartCoroutine(Fade());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);
        Fadeimg.CrossFadeAlpha(1, 3f, false);
    }
}
