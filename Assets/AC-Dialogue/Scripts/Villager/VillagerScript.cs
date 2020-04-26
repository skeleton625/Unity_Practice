using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;

public class VillagerScript : MonoBehaviour
{
    public VillagerData villager;
    public DialogueData dialogue;
    [SerializeField]
    private Renderer eyesRenderer;
    [SerializeField]
    private GameObject[] Particles;
    private Dictionary<string, ParticleSystem> EmotionParticles;

    private DialogueAudio dialogueAudio;
    private TMP_Animated animatedText;
    private Animator animator;
    private Camera mainCamera;

    private bool villagerIsTalking;

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        dialogueAudio = GetComponent<DialogueAudio>();

        // 스크립트에 할당된 Particle 오브젝트를 딕셔너리에 추가
        for(int i = 0; i < Particles.Length; i++)
            EmotionParticles.Add(Particles[i].name, Particles[i].GetComponent<ParticleSystem>());
       
        animator = GetComponent<Animator>();
        animatedText = InterfaceManager.instance.animatedText;
        animatedText.onAction.AddListener((action) => SetAction(action));
        animatedText.onEmotionChange.AddListener((newEmotion) => EmotionChanger(newEmotion));
    }

    // Emotion 변수에 따라 캐릭터의 눈 표정을 변환시키는 함수
    private void EmotionChanger(Emotion e)
    {
        if (this != InterfaceManager.instance.currentVillager)
            return;

        animator.SetTrigger(e.ToString());

        // Eye Material을 회전해 눈 표정을 정의
        if (e == Emotion.suprised)
            eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.33f, 0));
        else if (e == Emotion.angry)
            eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.66f, 0));
        else if (e == Emotion.sad)
            eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.33f, -.33f));
    }

    private void SetAction(string action)
    {
        // 현재 접촉 중인 시민이 없을 경우 넘김
        if (this != InterfaceManager.instance.currentVillager)
            return;

        // action이 shake일 경우, 카메라를 흔들어주는 효과 작동
        if (action.Equals("shake"))
            mainCamera.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        // 다른 action 의 경우 -> 음성 action
        else
        {
            // action에 해당하는 ParticleSystem 실행
            PlayParticle(action);
            // action에 해당하는 음성 출력
            dialogueAudio.PlayAudioByAction(action);
        }
    }

    // action 명으로 시작하는 ParticleSystem을 실행하는 함수
    private void PlayParticle(string action)
    {
        action += "Particle";
        // 딕셔너리에 해당 action이 없을 경우 넘김
        if (EmotionParticles[action] == null)
            return;
        EmotionParticles[action].Play();
    }

    // 캐릭터 표정 초기화 함수
    public void Reset()
    {
        animator.SetTrigger("normal");
        eyesRenderer.material.SetTextureOffset("_BaseMap", Vector2.zero);
    }

    // 시민 Object의 방향을 playerPos Vector3 방향으로 회전하는 함수
    public void TurnToPlayer(Vector3 playerPos)
    {
        transform.DOLookAt(playerPos, (transform.position - playerPos).sqrMagnitude / 5);
        string turnMotion = isRightSide(transform.forward, playerPos) ? "rturn" : "lturn";
        animator.SetTrigger(turnMotion);
    }

    // 플레이어 캐릭터가 바라보고 있는 시민 캐릭터의 왼쪽, 오른쪽에 있는지 판별하는 함수
    private bool isRightSide(Vector3 fwd, Vector3 targetDir)
    {
        // Vector3.Cross -> 두 매개변수 백터의 외적, 즉 fwd 방향의 오른쪽 백터
        Vector3 right = Vector3.Cross(Vector3.up.normalized, fwd.normalized);
        // Vector3.Dot -> 두 매개변수 백터간의 각도
        float dir = Vector3.Dot(right, targetDir.normalized);
        // -> 
        return dir > 0f;
    }
}
