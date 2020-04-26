using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Cinemachine;
using TMPro;
using DG.Tweening;


public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager instance;

    // 현재 다이얼로그 표시 상태
    [HideInInspector]
    public bool inDialogue;
    [HideInInspector]
    public VillagerScript currentVillager;

    public TMP_Animated animatedText;
    [SerializeField]
    private MovementInput characterInput;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private TextMeshProUGUI nameTMP;
    [SerializeField]
    private Image nameBubble;

    [Space]

    [Header("Cameras")]
    [SerializeField]
    private GameObject gameCam, dialogueCam;

    [Space]

    [SerializeField]
    private Volume dialogueDof;

    private int dialogueIndex;
    private bool canExit, nextDialogue;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        animatedText.onDialogueFinish.AddListener(() => FinishDialogue());
    }

    private void ResetState()
    {
        currentVillager.Reset();
        characterInput.active = true;
        inDialogue = false;
        canExit = false;
    }

    private void FinishDialogue()
    {
        if(dialogueIndex < currentVillager.dialogue.conversationBlock.Count - 1)
        {
            ++dialogueIndex;
            nextDialogue = true;
        }
        else
        {
            nextDialogue = false;
            canExit = true;
        }
    }

    // 시민에 따른 대화창 초기화 함수
    private void SetCharNameAndColor()
    {
        nameTMP.text = currentVillager.villager.name;
        nameTMP.color = currentVillager.villager.villagerNameColor;
        nameBubble.color = currentVillager.villager.villagerColor;
    }

    private void DialogueDOF(float x)
    {
        dialogueDof.weight = x;
    }

    // 대화 시, 대화 창을 비워주는 함수
    private void ClearText()
    {
        animatedText.text = string.Empty;
    }

    private void CameraChange(bool dialogue)
    {
        // 대화 실행 여부에 따라 해당 cinemachine 오브젝트를 활성화
        gameCam.SetActive(!dialogue);
        dialogueCam.SetActive(dialogue);

        float dofWeight = dialogueCam.activeSelf ? 1 : 0;
        DOVirtual.Float(dialogueDof.weight, dofWeight, .8f, DialogueDOF);
    }

    private void FadeUI(bool show, float time, float delay)
    {
        Sequence s = DOTween.Sequence();
        // DOTween에서 delay만큼 기다림
        s.AppendInterval(delay);
        // delay 초 뒤, canvasGroup을 DOFade 함
        s.Append(canvasGroup.DOFade(show ? 1 : 0, time));

        // 시민과 대화가 시작할 경우, 대화 초기화 진행
        if (show)
        {
            dialogueIndex = 0;
            s.Join(canvasGroup.transform.DOScale(0, time * 2).From().SetEase(Ease.OutBack));
            s.AppendCallback(() => animatedText.ReadText(currentVillager.dialogue.conversationBlock[0]));
        }
    }

    public void InitiateDialogue()
    {
        SetCharNameAndColor();
        inDialogue = true;
        CameraChange(true);
        ClearText();
        FadeUI(true, .2f, .65f);
    }
}
