using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectInfo
{
    public GameObject goPrefab;
    public int count;
    public Transform tfPoolParent; // 오브젝트 풀링 프리팹의 부모변수
}

public class ObjectPool : MonoBehaviour
{
    public ObjectInfo[] objectInfo = null;

    public static ObjectPool instance;

    public Queue<GameObject> objQueue = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //게임을 만들면서 생성파괴가 또 이루어질 만한 무언가가 있다면 배열인덱스를 늘려 거기에 넣어주면 된다. 적절하게 인덱스도 바꾸어준다.
        objQueue = InsertQueue(objectInfo[0]);
    }
  
    public Queue<GameObject> InsertQueue(ObjectInfo p_objectInfo)
    {
        Queue<GameObject> t_queue = new Queue<GameObject>();
        for(int i = 0; i < p_objectInfo.count; i++)
        {
            GameObject t_clone = Instantiate(p_objectInfo.goPrefab, transform.position, Quaternion.identity);
            t_clone.SetActive(false);
            if (p_objectInfo.tfPoolParent != null) t_clone.transform.SetParent(p_objectInfo.tfPoolParent); // 부모 오브젝트 tf가 설정되어있다면 부모 밑으로 생성
            else t_clone.transform.SetParent(this.transform); // 아니면 이 스크립트가 붙어있는 오브젝트 밑으로 생성

            t_queue.Enqueue(t_clone);
        }
        return t_queue;
    }
}
