using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolBullet : MonoBehaviour
{
    public static ObjectPoolBullet Instance;

    [SerializeField] // 생성해야 할 오브젝트의 프리팹
    private GameObject poolingObjectPrefab;

    Queue<Bullet> poolingObjectQueue = new Queue<Bullet>();

    // 싱글톤 (오브젝트를 생성하는 어떤 코드에서든 접근이 가능해야 한다)
    private void Awake()
    {
        Instance = this;

        Initialize(10);
    }
    // 왜 이 갯수를 넘기면 오브젝트가 사라지지 않는가
    //오브젝트 풀을 초기화 하는 함수
    private void Initialize(int initCount)
    {
        for (int i = 0; i < initCount; i++)
        {
            // 게임이 시작하기 전에 사용될 게임 오브젝트를 미리 적절한 갯수로 만들어 놓는다.
            poolingObjectQueue.Enqueue(CreateNewObject());
        }
    }

    // poolingObjectPrefab으로부터 새 게임 오브젝트를 만든 뒤 비활성화 해서 반환한다.
    private Bullet CreateNewObject()
    {
        // 이 스크립트를 장착한 오브젝트의 위치에서 프리팹을 생성한다
        var newObj = Instantiate(poolingObjectPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    // 오브젝트 풀이 가지고 있는 게임오브젝트를 요청한 곳에 꺼내준다. 모든 오브젝트를 꺼내줘서 poolingObjectQueue에 빌려줄 오브젝트가 없다면 CreateNewObject함수를 호출하여 새 오브젝트를 생성한다.
    public static Bullet GetObject()
    {
        if (Instance.poolingObjectQueue.Count > 0)
        {
            var obj = Instance.poolingObjectQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObject();
            newObj.gameObject.SetActive(true);
            newObj.transform.SetParent(null);
            return newObj;
        }
    }

    // 빌려준 오브젝트를 돌려받고 비활성화한다.
    public static void ReturnObject(Bullet obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform);
        Instance.poolingObjectQueue.Enqueue(obj);
    }
}
