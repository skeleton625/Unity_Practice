using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] RiverPaths;
    [SerializeField]
    private float[] Spaces, StrengthLevel;
    [SerializeField]
    private TerrainGenerator generator;
    [SerializeField]
    private int RestrictPos;
    [SerializeField]
    private float minRot, maxRot;
    [SerializeField]
    public GameObject MainRiver;
    private static int num;

    private void Start()
    {
        GenerateAllRivers();
    }

    // Start is called before the first frame update
    public void GenerateAllRivers()
    {
        int interval = 1024 / RiverPaths.Length;
        int startPos = (int) transform.position.z;
        for (int i = 0; i < RiverPaths.Length; i++)
        {
            Vector3 pos = RiverPaths[i].transform.position;
            Vector3 rot = RiverPaths[i].transform.rotation.eulerAngles;
            rot.y += (Random.Range(0, 2) == 0 ? -1 : 1) * Random.Range(minRot, maxRot);

            pos.z = Random.Range(startPos + RestrictPos, (startPos+interval) - RestrictPos);
            int depth = Random.Range(0, generator.DepthCount);
            int strength = Random.Range(0, StrengthLevel.Length);
            RiverPaths[i].transform.position = pos;
            RiverPaths[i].transform.rotation = Quaternion.Euler(rot);
            RiverPaths[i].GetComponent<PathCreator>().
                                    CreateRandomRiver(depth, 0, 
                                                      Spaces[depth], 
                                                      StrengthLevel[strength] / 1500, 
                                                      generator);
            startPos += interval;
        }
        generator.ApplyPreTerrainHeights(false);
        SetCombineAllRiverMesh();
        gameObject.SetActive(false);
    }

    private void SetCombineAllRiverMesh()
    {
        MeshFilter[] meshes = MainRiver.transform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshes.Length];

        for (int i = 0; i < meshes.Length; i++)
        {
            combine[i].mesh = meshes[i].sharedMesh;
            combine[i].transform = meshes[i].transform.localToWorldMatrix;
            meshes[i].gameObject.SetActive(false);
        }

        MeshFilter RiverMesh = MainRiver.GetComponent<MeshFilter>();
        RiverMesh.mesh = new Mesh();
        RiverMesh.mesh.CombineMeshes(combine);
        MainRiver.SetActive(true);
    }
}
