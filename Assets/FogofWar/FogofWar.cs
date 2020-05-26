using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogofWar : MonoBehaviour
{
    [SerializeField]
    private GameObject FOW_plane;
    [SerializeField]
    private Transform Player_trans;
    [SerializeField]
    private LayerMask FOW_layer;
    [SerializeField]
    private float FogRadius;
    private float FogRadiusSqr { get => FogRadius * FogRadius; }

    private Mesh planeMesh;
    private Vector3[] planeVertices;
    private Color[] planeColors;

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateFogOfWar();
    }

    private void Initialize()
    {
        planeMesh = FOW_plane.GetComponent<MeshFilter>().mesh;
        planeVertices = planeMesh.vertices;
        planeColors = new Color[planeVertices.Length];

        for (int i = 0; i < planeColors.Length; i++)
            planeColors[i] = Color.black;
        UpdateColor();
    }

    private void UpdateFogOfWar()
    {
        Ray r = new Ray(transform.position, Player_trans.position - transform.position);
        RaycastHit info;

        Debug.DrawRay(r.origin, r.direction * 1000, Color.blue);
        /* FogOfWar 레이어의 오브젝트에만 반응하는 Ray 발사 */
        if(Physics.Raycast(r, out info, 1000, FOW_layer, QueryTriggerInteraction.Collide))
        {
            for(int i = 0; i < planeVertices.Length; i++)
            {
                Vector3 vertPos = FOW_plane.transform.TransformPoint(planeVertices[i]);
                float dist = (vertPos - info.point).sqrMagnitude;

                /* 거리에 따라 안개가 걷히도록 정의 */
                if(dist < FogRadiusSqr)
                    planeColors[i].a = Mathf.Min(planeColors[i].a, dist / FogRadiusSqr);
            }
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        planeMesh.colors = planeColors;
    }
}
