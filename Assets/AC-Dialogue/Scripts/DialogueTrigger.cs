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
    private Volume dialougeDof;

    private void Start()
    {
        ui = InterfaceManager.instance;
        movement = GetComponent<MovementInput>();
    }

    private void Update()
    {
        
    }

    private void ActivateDialogueWithVillager()
    {
        if(Input.GetKeyDown(KeyCode.F) && !ui.inDialogue && currentVillager != null)
        {
            // m_Targets[0] == Player Chracter
            targetGroup.m_Targets[1].target = currentVillager.transform;
            movement.enabled = false;
        }
    }
}
