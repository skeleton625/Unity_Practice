using UnityEngine;
using UnityEngine.AI;

public class AINavigator : MonoBehaviour
{
    [SerializeField]
    private Transform MainCamera;
    [SerializeField]
    private Camera PlayerCamera;
    // AI의 Navigation 객체
    private NavMeshAgent Agent;

    private Vector3 PrePosition;
    
    private float CameraX, CameraY, CameraZ;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;

        CameraX = MainCamera.position.x;
        CameraY = MainCamera.position.y;
        CameraZ = MainCamera.position.z;
        PrePosition = transform.position;
    }

    private void Update()
    {
        TranslatePositionByMouse();
        TranslateCamera();
        RotateAI();
    }

    // AI에게 매개변수 좌표로 이동하도록 함
    private void SetDirection(Vector3 _dir)
    {
        Agent.SetDestination(_dir);
    }

    // 마우스로 클릭한 위치로 이동하도록 동작하는 함수
    private void TranslatePositionByMouse()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray _ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit _info;
            if(Physics.Raycast(_ray.origin, _ray.direction, out _info, 1000f))
                SetDirection(_info.point);
        }
    }

    private void TranslateCamera()
    {
        if(!PrePosition.Equals(transform.position))
        {
            Vector3 _pos = transform.position;

            _pos.x += CameraX;
            _pos.y += CameraY;
            MainCamera.position = _pos;
            PrePosition = transform.position;
        }

    }

    // NavMeshAgent를 더 빠르게 회전시키는 함수
    private void RotateAI()
    {
        if(Agent.desiredVelocity.sqrMagnitude >= 0.01f)
        {
            Vector3 direction = Agent.desiredVelocity;
            Quaternion targetAngle = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  targetAngle,
                                                  Time.deltaTime * 8f);
        }
    }
}
