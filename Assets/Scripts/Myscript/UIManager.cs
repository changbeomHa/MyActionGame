using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject[] SettingUI;

    public void SetUI(bool _flag)
    {
        SettingUI[0].SetActive(_flag);
        SettingUI[1].SetActive(_flag);
        SettingUI[2].SetActive(_flag);
        SettingUI[3].SetActive(_flag);
    }

}
