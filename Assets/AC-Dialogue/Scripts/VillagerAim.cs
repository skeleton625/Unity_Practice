using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// MultiAimConstraint 클래스의 namespace
using UnityEngine.Animations.Rigging;

public class VillagerAim : MonoBehaviour
{
    [SerializeField]
    private Transform AimController, AimTarget;
    [SerializeField]
    private MultiAimConstraint MultiAim;

    private IEnumerator MoveTargetCoroutine;
    private IEnumerator MoveNormalCoroutine;

    private void Awake()
    {
        MoveTargetCoroutine = MoveHeadToTarget();
        MoveNormalCoroutine = MoveHeadToNormal();
    }

    // 캐릭터의 머리 방향을 Target으로 향하게 하는 Coroutine 함수
    private IEnumerator MoveHeadToTarget()
    {
        Vector3 pos;

        while(true)
        {
            pos = AimTarget.position + Vector3.up;
            MultiAim.weight = Mathf.Lerp(MultiAim.weight, 1f, .05f);
            AimController.position = Vector3.Lerp(AimController.position, pos, .3f);

            yield return null;
        }
    }

    // 캐릭터의 머리 방향을 캐릭터 앞 방향으로 향하게 하는 Coroutine 함수
    private IEnumerator MoveHeadToNormal()
    {
        Vector3 pos;

        while (MultiAim.weight > 0.1)
        {
            pos = transform.position + transform.forward + Vector3.up;
            MultiAim.weight = Mathf.Lerp(MultiAim.weight, 0, .05f);
            AimController.position = Vector3.Lerp(AimController.position, pos, .3f);

            yield return null;
        }

        MultiAim.weight = 0;
        AimController.position = transform.position + transform.forward + Vector3.up;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player Object가 충돌 시, 해당 Object를 목표 타겟으로 설정
        if (other.CompareTag("Player"))
        {
            AimTarget = other.transform;
            StopCoroutine(MoveNormalCoroutine);
            StartCoroutine(MoveTargetCoroutine);
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        // 충돌한 Player Object가 Collider에서 나갈 시, 목표 타겟 제거
        if (other.CompareTag("Player"))
        {
            StopCoroutine(MoveTargetCoroutine);
            MoveNormalCoroutine = MoveHeadToNormal();
            StartCoroutine(MoveNormalCoroutine);
            AimTarget = null;
        }
    }
}
