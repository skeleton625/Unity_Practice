using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] RiverPaths;
    [SerializeField]
    private float[] WaterLevels, Spaces, Strengths;
    [SerializeField]
    private TerrainGenerator generator;
    [SerializeField]
    private int RestrictPos;
    [SerializeField]
    private float minRot, maxRot;
    [SerializeField]
    public GameObject MainRiver;
    [SerializeField]
    private Camera mainCamera;
    private RaycastHit info;

    // Start is called before the first frame update
    public void GenerateAllRivers()
    {
        int interval = 1024 / RiverPaths.Length;
        int startPos = 0;
        for (int i = 0; i < RiverPaths.Length; i++)
        {
            Vector3 pos = RiverPaths[i].transform.position;
            Vector3 rot = RiverPaths[i].transform.rotation.eulerAngles;
            rot.y += (Random.Range(0, 2) == 0 ? -1 : 1) * Random.Range(minRot, maxRot);

            pos.z = Random.Range(startPos + RestrictPos, (startPos+interval) - RestrictPos);

            int depth = Random.Range(0, generator.DepthCount);
            float water = WaterLevels[CalculateRiverDepth(depth)];
            Debug.Log(depth + " " + water);
            RiverPaths[i].transform.position = pos;
            RiverPaths[i].transform.rotation = Quaternion.Euler(rot);
            RiverPaths[i].GetComponent<PathCreator>().CreateRandomRiver(depth, water, Spaces[depth], Strengths[depth]);
            startPos += interval;
        }
        generator.ApplyPreTerrainHeights();
        SetCombineAllRiverMesh();
    }

    private int CalculateRiverDepth(int DepthLevel)
    {
        switch(DepthLevel)
        {
            case 0:
                return Random.Range(0, WaterLevels.Length - 2);
            case 1:
                return Random.Range(0, WaterLevels.Length - 1);
            case 2:
                return Random.Range(0, WaterLevels.Length) ;
            default:
                return 0;
        }
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
