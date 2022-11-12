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
            Debug.Log("�� �޽����� ���δٸ� exit ó���� ���Դϴ�.");
            player.lockedTarget = player.enemyDetection.CurrentTarget();
        }
    }
}
