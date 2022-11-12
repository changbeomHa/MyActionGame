using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BossCubeFake : MonoBehaviour
{
    public float healthcount;
    public float countdown = 0;
    public Slider healtSilder;
    public GameObject PressGImg;
    public GameObject BombEffect;
    bool playerOn = false;
    public Camera mainCam;


    // Start is called before the first frame update
    void Start()
    {
        //mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        PressGImg = GameObject.Find("StageUI").transform.Find("PressG").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // ui가 카메라를 바라보도록
        healtSilder.value = healthcount;
        countdown += Time.deltaTime;

        if (playerOn && Input.GetKey(KeyCode.G))
        {
            Debug.Log("dho asdlkfjlsk");
            healthcount += Time.deltaTime;
        }

        if (countdown > 8f)
        {
            PressGImg.SetActive(false);           
            StartCoroutine(DestoryThis());
        }

        if(healthcount > 4)
        {
            PressGImg.SetActive(false);
            Destroy(gameObject);
        }
    }
    IEnumerator DestoryThis()
    {
        BombEffect.SetActive(true);
        transform.DOScale(0, 0);
        yield return new WaitForSeconds(.1f);
        //Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.Euler(new Vector3(0, 180, 0)));
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerOn = true;
            PressGImg.SetActive(true);          
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerOn = false;
            PressGImg.SetActive(false);          
        }
    }

}
