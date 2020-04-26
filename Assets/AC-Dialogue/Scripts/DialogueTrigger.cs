using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

public class DialogueTrigger : MonoBehaviour
{
    private InterfaceManager ui;
    private VillagerScript currentVillager;
    private MovementInput movement;

    [SerializeField]
    private CinemachineTargetGroup targetGroup;

    [Space]
    [Header("Post Processing"), SerializeField]
    public Volume dialogueDof;

    private void Start()
    {
        ui = InterfaceManager.instance;
        movement = GetComponent<MovementInput>();
    }

    private void Update()
    {
        ActivateDialogueWithVillager();
    }

    private void ActivateDialogueWithVillager()
    {
        // 현재 대화 대상이 존재하며, 대화 중이 아닐 시, 'F' 키를 통해 대화 시도
        if(Input.GetKeyDown(KeyCode.F) && !ui.inDialogue && currentVillager != null)
        {
            // m_Targets[0] ==> Player Chracter
            targetGroup.m_Targets[1].target = currentVillager.transform;
            movement.active = false;
            ui.InitiateDialogue();
            currentVillager.TurnToPlayer(transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 시민 Object의 충돌체와 충돌한 경우, 해당 시민들 대화 대상으로 정의
        if(other.CompareTag("Villager"))
        {
            currentVillager = other.GetComponent<VillagerScript>();
            ui.currentVillager = currentVillager;
        }
    }

    private void nTriggerExit(Collider other)
    {
        // 시민 Object의 충돌체에서 떨어진 경우, 대화 대상에서 제거
        if(other.CompareTag("Villager"))
        {
            currentVillager = null;
            ui.currentVillager = null;
        }
    }
}
