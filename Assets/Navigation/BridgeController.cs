using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BridgeController : MonoBehaviour
{
    [SerializeField]
    private Transform Bridge;
    [SerializeField]
    private float RotateValue;
    [SerializeField]
    private NavMeshAgent[] Agents;

    private bool IsEnd = true;
    private bool IsUp = false;

    private void Update()
    {
        if(Input.GetMouseButtonDown(1) && IsEnd)
        {
            IsUp = !IsUp;
            StartCoroutine(SetBridgeRotate());
        }
    }

    private IEnumerator SetBridgeRotate()
    {
        IsEnd = false;

        foreach(NavMeshAgent _enemy in Agents)
            _enemy.areaMask = IsUp ?  ~(1 << 6) : ~0;

        float val;
        if(IsUp)
        {
            val = 0;
            while(val >= RotateValue)
            {
                val += Mathf.Lerp(0, RotateValue, 0.01f);
                Bridge.rotation = Quaternion.Euler(val, 0, 0);
                yield return null;
            }
            val = RotateValue;
        }
        else
        {
            val = RotateValue;
            while(val <= 0)
            {
                val += Mathf.Lerp(0, -RotateValue, 0.01f);
                Bridge.rotation = Quaternion.Euler(val, 0, 0);
                yield return null;
            }
            val = 0;
        }

        Bridge.rotation = Quaternion.Euler(val, 0, 0);
        IsEnd = true;
    }
}
