using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : MonoBehaviour
{
    private bool Check = false;
    public ParticleSystem particle;

    private void Start()
    {
        particle = GameObject.Find("BombParticle").GetComponent<ParticleSystem>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Check)
        {
            //Check = true;
            other.GetComponent<CombatScript>().DamageNumber = 1;
            other.GetComponent<CombatScript>().DamageEvent();
            StartCoroutine(DestroyThis());
        }
        
    }
    public float count = 0.0f;
    private void Update()
    {
        count += Time.deltaTime;
        if (count > 2.0) Check = true;

        if (count > 5)
        {
            particle.transform.position = transform.position;
            particle.Play();
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(1f);
        particle.transform.position = transform.position;
        particle.Play();
        Destroy(gameObject);
    }
}
