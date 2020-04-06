using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverCollider : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
    }
}
