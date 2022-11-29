using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private CombatScript combatScript;
    private MovementInput movementInput;
    private Animator anim;

    public temp temp;
    public static List<Vector3> tempVector = new List<Vector3>(new Vector3[28]);
    public static int tempI;

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
    }

    void InputStyle()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            styleIndex = 0;
            print("style motion: strutting_normal_walking_000.bvh");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            styleIndex = 1;
            print("style motion: old_normal_walking_000.bvh");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            styleIndex = 2;
            print("style motion: proud_normal_walking_000.bvh");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            styleIndex = 3;
            print("style motion: childlike_normal_walking_000.bvh");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            styleIndex = 4;
            print("style motion: depressed_normal_walking_000.bvh");
        }
    }
    // Update is called once per frame
    void Update()
    {
        InputStyle();
        currentStyleIndex = styleIndex.ToString();

        if (movementInput.isPFNN)
        {
            isDash = movementInput.isDash;
            isAttack = combatScript.isAttackingEnemy || combatScript.isCountering;
            isBasicControl = isDash || isAttack;
            anim.enabled = isBasicControl;
        }

        tempVector[temp.i] = new Vector3(temp.a, temp.b, temp.c);
        tempI = temp.i;
    }
}
