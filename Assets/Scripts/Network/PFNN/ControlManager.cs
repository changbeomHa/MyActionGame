using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private CombatScript combatScript;
    private MovementInput movementInput;
    private Animator anim;

    public static bool isBasicControl;
    public static Vector3 targetVector;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        combatScript = this.GetComponent<CombatScript>();
        movementInput = this.GetComponent<MovementInput>();
    }

    // Update is called once per frame
    void Update()
    {
        isBasicControl = combatScript.isAttackingEnemy || combatScript.isCountering || movementInput.isDash;
        anim.enabled = isBasicControl;
    }
}
