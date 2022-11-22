using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private CombatScript combatScript;
    private MovementInput movementInput;
    private Animator anim;

    public GameObject temp;

    public int styleIndex;

    public static bool isDash;
    public static bool isAttack;
    public static bool isBasicControl;
    public static Vector3 targetVector;
    public static string jointInput = "";
    public static string jointOutput = "";
    public static string currentStyleIndex = "";

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        combatScript = this.GetComponent<CombatScript>();
        movementInput = this.GetComponent<MovementInput>();
        Instantiate(temp);
    }

    // Update is called once per frame
    void Update()
    {
        currentStyleIndex = styleIndex.ToString();

        if (movementInput.isPFNN)
        {
            isDash = movementInput.isDash;
            isAttack = combatScript.isAttackingEnemy || combatScript.isCountering;
            isBasicControl = isDash || isAttack;
            anim.enabled = isBasicControl;
        }
    }
}
