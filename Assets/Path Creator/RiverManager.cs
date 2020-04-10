using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] RiverPaths;
    [SerializeField]
    private TerrainGenerator generator;
    [SerializeField]
    private int RestrictPos;
    [SerializeField]
    private float ReStrictRot;

    // Start is called before the first frame update
    public void GenerateAllRivers()
    {
        for (int i = 0; i < RiverPaths.Length; i++)
        {
            Vector3 pos = RiverPaths[i].transform.position;
            Vector3 rot = RiverPaths[i].transform.rotation.eulerAngles;

            if (pos.x == 0 || pos.x == 1024)
                pos.z = Random.Range(RestrictPos, 1024 - RestrictPos);
            else if (pos.z == 0 || pos.z == 1024)
                pos.x = Random.Range(RestrictPos, 1024 - RestrictPos);
            rot.y += (Random.Range(0, 1) == 0 ? 1 : -1) * ReStrictRot;
            
            RiverPaths[i].transform.position = pos;
            RiverPaths[i].transform.rotation = Quaternion.Euler(rot);
            RiverPaths[i].GetComponent<PathCreator>().CreateRandomRiver(1);
        }
        generator.GenerateRestrictHeights(10);
    }
}
