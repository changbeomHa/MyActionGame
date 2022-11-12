using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8.0f; //총알 이동 속력

    // 이동에 사용할 리지드바디 컴포넌트
    public Rigidbody bulletRigidbody;
    public bool hit = false;

    // Start is called before the first frame update
    void Start()
    {
        Shoot();
        //가져온 리지드바디의 속도 = 앞쪽 방향 * 이동 속력
        //bulletRigidbody.velocity = direction * speed;
        bulletRigidbody.velocity = transform.right * speed;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 상대방 게임 오브젝트가 Player 태그를 가진 경우
        if (other.tag == "Player")
        {
            // n초 후 함수 실행
            Invoke("DestroyBullet", 0f);
            // 상대방 PlayerController 컴포넌트를 가져와서 Die() 실행
            //PlayerStat.instance.Hit(10);
        }

    }
    //Vector3 direction;
    public void Shoot(/*Vector3 dir*/)
    {
        //this.direction = dir;
        Invoke("DestroyBullet", 5f);
    }

    public void DestroyBullet()
    {
        Destroy(this);
        //ObjectPoolBullet.ReturnObject(this);
    }
}
