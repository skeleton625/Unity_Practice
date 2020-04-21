using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;

public class VillagerScript : MonoBehaviour
{
    [SerializeField]
    private VillagerData villager;
    [SerializeField]
    private DialogueData diaogue;
    [SerializeField]
    private Renderer eyesRenderer;
    [SerializeField]
    private GameObject[] Particles;
    private Dictionary<string, ParticleSystem> EmotionParticles;

    private bool villagerIsTalking;

    private TMP_Animated animatedText;
    private Animator animator;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        // 스크립트에 할당된 Particle 오브젝트를 딕셔너리에 추가
        for(int i = 0; i < Particles.Length; i++)
            EmotionParticles.Add(Particles[i].name, Particles[i].GetComponent<ParticleSystem>());
       
        animator = GetComponent<Animator>();
        animatedText = InterfaceManager.instance.animatedText;
        animatedText.onAction.AddListener((action) => SetAction(action));
        animatedText.onEmotionChange.AddListener((newEmotion) => EmotionChanger(newEmotion));
    }

    private void EmotionChanger(Emotion e)
    {
        if (this != InterfaceManager.instance.currentVillager)
            return;

        animator.SetTrigger(e.ToString());

        // Eye Material을 회전해 눈 감정 표현 정의
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
        else
        {
            PlayParticle(action);

            if (action.Equals("sparkle"))
            {

            }
            else if (action.Equals("rain"))
            {

            }
        }
    }

    private void PlayParticle(string action)
    {
        action += "Particle";
        // 딕셔너리에 해당 action이 없을 경우 넘김
        if (EmotionParticles[action] == null)
            return;
        EmotionParticles[action].Play();
    }
}
