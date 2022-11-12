using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCube : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrefab;
    public float spawnRateMin = 0.5f; // 최소 생성 주기
    public float spawnRateMax = 3.0f; // 최대 생성 주기

    public Transform target;
    Vector3 startVec;

    private float spawnRate; // 생성 주기
    private float timeAfterSpawn; // 최근 생성 시점에서 지난 시간


    // 옵저버를 사용하는 경우 활성화
    //public Observer observer;

    // Start is called before the first frame update
    void Start()
    {
        startVec = this.transform.position;
        // 타이머를 리셋
        timeAfterSpawn = 0;
        // 총알 생성 간격을 spawnRateMin과 spawnRateMax 사이에서 랜덤 결정
        spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        // 총알을 조준할 대상으로 PlayerController를 가진 게임 오브젝트를 찾기
        
    }

    // Update is called once per frame
    void Update()
    {
        // 옵저버를 사용하는 경우 활성화
        //if(observer.isPlayerInRange == true)
        //{
        //    target = observer.player;
        //    this.transform.LookAt(target);
        //}
        //else
        //{
        //    // y축만 움직였고, 나머지는 고정시킨다.
        //    this.transform.rotation = Quaternion.Euler(0, startVec.y, 0);
        //}

        // 옵저버를 사용하는 경우 비활성화
        this.transform.LookAt(target);


        // timeAfterSpawn을 갱신
        timeAfterSpawn += Time.deltaTime;

        if (timeAfterSpawn >= spawnRate)
        {
            // 총알 생성 간격보다 타이머 값이 크면, 타이머를 리셋
            timeAfterSpawn = 0;

            // 총알 생성

            // bulletPrefab의 복제본을 총알 생성기의 위치와 회전에 생성
            //GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            var bullet = ObjectPoolBullet.GetObject();
            //var direction = new Vector3(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, this.gameObject.transform.rotation.z);

            bullet.transform.position = this.gameObject.transform.position;
            //bullet.transform.rotation = this.gameObject.transform.rotation;
            bullet.Shoot(/*direction*/);

            // 생성한 총알의 앞쪽방향을 target을 바라보도록 변경
            //bullet.transform.LookAt(target);

            // 다음번 생성 시점 까지의 간격을 랜덤 설정
            spawnRate = Random.Range(spawnRateMin, spawnRateMax);
        }
    }
}
