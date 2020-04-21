using UnityEngine;

// 상단 메뉴 Asset에 VillagerData 스크립트 생성 버튼 추가
[CreateAssetMenu(fileName = "New Villager", menuName = "Villager")]
public class VillagerData : ScriptableObject
{
    public string vilalgerName;
    public Color villagerColor;
    public Color villagerNameColor;
    public DialogueData dialogue;
}
