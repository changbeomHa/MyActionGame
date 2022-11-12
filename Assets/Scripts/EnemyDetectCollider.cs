using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectCollider : MonoBehaviour
{
    [SerializeField] CombatScript player;

    // Start is called before the first frame update


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            player.lockedTarget = other.gameObject.GetComponent<EnemyScript>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("이 메시지가 보인다면 exit 처리된 것입니다.");
            player.lockedTarget = player.enemyDetection.CurrentTarget();
        }
    }
}
