using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI txtScore = null;
    [SerializeField] int increaseHeartRate = 0;
    public int currentHeartRate = 0; // 심박수 증가 비율. 이지모드의 경우 1, 하드모드일 경우 3으로 하면 될듯?

    [SerializeField] float[] weight = null;
    [SerializeField] int comboBonusScore = 10;

    Animator anim;
    private CombatScript playerCombat;
   
    int currentCombo = 0; // 심박수 비율 낮추는 콤보
    public int awakeningCombo = 0; // 70점에서 각성 콤보
    [SerializeField] TextMeshProUGUI txtCombo = null;
    [SerializeField] TextMeshProUGUI awkCombo = null;


    bool needCalmdown; // 원래 콤보 점수 활성화 조건으로 사용되는 변수였으나, 지금은 사용되지 않음

    // Start is called before the first frame update
    void Start()
    {
        txtCombo.gameObject.SetActive(false);
        //theCM = FindObjectOfType<ComboManager>();
        anim = GetComponent<Animator>();
        playerCombat = FindObjectOfType<CombatScript>();
    }

    public void IncreaseScore(int p_JudgementState)
    {
        //int t_currentCombo = theCM.GetCurrentCombo();

        // 가중치 계산
        int t_increaseScore = increaseHeartRate;
        t_increaseScore = (int)(t_increaseScore * weight[p_JudgementState]);

        currentHeartRate += t_increaseScore;
        txtScore.text = currentHeartRate.ToString();

        // 콤보 활성화 조건
        /*if (currentHeartRate >= 80)
        {
            needCalmdown = true;           
        }
        else if(currentHeartRate == 70)
        {
            needCalmdown = false;
        }*/

        // 콤보점수 (심박수 정상화, 70 초과면 콤보점수로 다시 70까지 떨어뜨릴 수 있다)
        if (currentHeartRate > 70)
        {
            if (currentHeartRate > 80)
            {
                playerCombat.Stunned();
                ResetCombo();
                currentHeartRate = 75;
                txtScore.text = currentHeartRate.ToString();
            }

            if (p_JudgementState == 0 && currentHeartRate >= 70)
            {
                currentCombo += 1;
                if (currentCombo > 2)
                {
                    currentHeartRate -= 1;
                    txtCombo.gameObject.SetActive(true);
                    txtCombo.text = (-1 * (currentCombo - 2)).ToString();
                }

                txtScore.text = currentHeartRate.ToString();
            }
        }
        else if (currentHeartRate == 70)
        {
            if (p_JudgementState == 0)
            {
                awakeningCombo += 1;
                if (awakeningCombo >= 4)
                {
                    playerCombat.nowPowerful = true;
                    playerCombat.AwakeningOK.color = Color.red;
                }
            }
            else if (p_JudgementState != 0)
            {
                awakeningCombo = 0;
            }
            ResetCombo();
        }

        anim.SetTrigger("ScoreUp");
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        txtCombo.text = "0";
        txtCombo.gameObject.SetActive(false);
    }
}
