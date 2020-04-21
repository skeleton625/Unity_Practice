using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue")]
public class DialogueData : ScriptableObject
{
    // 대화 정의 변수 ( Text 공간을 4 x 4 로 정의 )
    [TextArea(4, 4)]
    public List<string> conversationBlock;
}
