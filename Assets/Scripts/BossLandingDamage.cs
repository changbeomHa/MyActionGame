using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLandingDamage : MonoBehaviour
{
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<CombatScript>().DamageNumber = 6;
            other.GetComponent<CombatScript>().DamageEvent();
            other.GetComponent<CombatScript>().health -= 1;
        }
    }

    
}
