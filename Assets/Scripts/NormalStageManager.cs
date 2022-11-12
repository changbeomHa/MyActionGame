using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalStageManager : MonoBehaviour
{
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] GameObject EnemyObj;
    public bool stageStart;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (stageStart)
        {
            stageStart = false;
            enemyManager.EnemySpawn();
        }
    }
}
