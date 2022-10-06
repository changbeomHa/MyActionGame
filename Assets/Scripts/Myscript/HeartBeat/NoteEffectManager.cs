using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteEffectManager : MonoBehaviour
{
    [SerializeField] ParticleSystem heartBeat;
    public void NoteHitEffect()
    {
        //이펙트 발동 코드 (에니메이션 트리거 라던가..)
        heartBeat.Play();
    }
}
