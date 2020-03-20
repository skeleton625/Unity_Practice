using UnityEngine;
using UnityEngine.AI;

public class AINavigator : MonoBehaviour
{
    [SerializeField]
    private Transform Target;
    // AI의 Navigation 객체
    private NavMeshAgent Agent;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        SetDirection(Target.position);
    }

    // AI에게 매개변수 좌표로 이동하도록 함
    private void SetDirection(Vector3 _dir)
    {
        Agent.SetDestination(_dir);
    }
}
