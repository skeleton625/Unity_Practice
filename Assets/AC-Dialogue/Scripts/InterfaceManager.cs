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
    public TMP_Animated animatedText;
    [HideInInspector]
    public VillagerScript currentVillager;

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

    private void FinishDialogue()
    {
    }
}
