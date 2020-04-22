using UnityEngine;
using TMPro;
using DG.Tweening;

public class DialogueAudio : MonoBehaviour
{
    private VillagerScript villager;
    private TMP_Animated animatedText;

    [SerializeField]
    private Transform mouthQuad;

    // 일반 문자 음성, 특수 문자 음성 소스
    [SerializeField]
    private AudioClip[] voices, punctuations;

    // 일반 문자, 특수 문자, 효과 음성 관리 변수
    [Space]
    public AudioSource voiceSource, punctuationSource, effectSource;

    [Space]
    private AudioClip sparkleClip, rainClip;

    private void Start()
    {
        villager = GetComponent<VillagerScript>();

        animatedText = InterfaceManager.instance.animatedText;
        animatedText.onTextReveal.AddListener((newChar) => ReproduceSound(newChar));
    }

    // 문자 c에 따라 맞는 소리를 출력하는 함수
    private void ReproduceSound(char c)
    {
        if (villager != InterfaceManager.instance.currentVillager)
            return;

        // 현재 문자가 특수문자면서 특수문자 음성이 출력되지 않을 경우
        if (char.IsPunctuation(c) && !punctuationSource.isPlaying)
        {
            voiceSource.Stop();
            // 특수문자 랜덤 음성을 출력
            voiceSource.clip = punctuations[Random.Range(0, punctuations.Length)];
            voiceSource.Play();
        }
        // 현재 문자가 일반문자면서 일반문자 음성이 출력되지 않을 경우
        else if(char.IsLetter(c) && !voiceSource.isPlaying)
        {
            punctuationSource.Stop();
            // 일반문자 랜덤 음성을 출력
            voiceSource.clip = voices[Random.Range(0, voices.Length)];
            voiceSource.Play();

            // 캐릭터의 입모양 수정 코드 <- DOTween 패키지
            mouthQuad.localScale = new Vector3(1, 0, 1);
            /* 
             * DOScale(float 목표값, float 변화시간) -> 변화시간동안 목표값으로 이동
             * OnComplete -> DOScaleY 함수가 완료 시, 할당받은 매개 함수를 실행
             */
            mouthQuad.DOScaleY(1, .2f).OnComplete(() => mouthQuad.DOScaleY(0, .2f));
        }
    }

    public void PlayAudioByAction(string action)
    {
        if(action.Equals("sparkle"))
        {
            effectSource.clip = sparkleClip;
            effectSource.Play();
        }
        else if(action.Equals("rain"))
        {
            effectSource.clip = rainClip;
            effectSource.Play();
        }
    }
}
