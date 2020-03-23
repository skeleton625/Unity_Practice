using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform[] Vertices;
    [SerializeField]
    private int RiverBound;
    [SerializeField]
    private float RiverDepth;
    [SerializeField]
    private TerrainInfo Info;

    private float A, B;
    private int[,] ArrayDir;

    private void Awake()
    {
        ArrayDir = new int[8, 2]
        {
            {-1, -1 }, {-1, 0 },{-1, 1 },
            {0, -1 }, {0, 1 },
            {1, -1 }, {1, 0 }, {1, 1 }
        };
    }

    private void Start()
    {
        Info = TerrainInfo.Instance;
    }

    public void GenerateStraightRiver(ref float[,] _field)
    {
        for (int c = 0; c < Vertices.Length - 1; c++)
        {
            Vector3 _start = Vertices[c].position, _end = Vertices[c + 1].position;
            float value = Info.HeightLimit - RiverBound * RiverDepth;

            CalculateAandB(_start, _end);
            for (int x = (int)_start.x; x < _end.x; x++)
            {
                int _nz = (int)(A * x + B);
                if (_nz < 0 || _nz >= Info.Width)
                    continue;

                _field[_nz, x] = value;
                DownValueInArray(ref _field, x, _nz, value, 0);
            }

            for (int z = (int)_start.z; z < _end.z; z++)
            {
                int _nx = (int)((z - B) / A);
                if (_nx < 0 || _nx >= Info.Height)
                    continue;

                _field[z, _nx] = value;
                DownValueInArray(ref _field, _nx, z, value, 0);
            }
        }
    }

    private void DownValueInArray(ref float[,] _field, int x, int z, float val, int cnt)
    {
        if (cnt > RiverBound)
            return;

        int nx, nz;
        for (int i = 0; i < 8; i++)
        {
            nx = x + ArrayDir[i, 0];
            nz = z + ArrayDir[i, 1];

            if (nx < 0 || nx >= Info.Width || nz < 0 || nz >= Info.Height)
                continue;
            else if (_field[nz, nx] < val)
                continue;
            else if (_field[nz, nx] > val)
            {
                _field[nz, nx] = val;
                DownValueInArray(ref _field, nx, nz, val + 0.01f, cnt);
            }
            else
            {
                _field[nz, nx] = val;
                DownValueInArray(ref _field, nx, nz, val + RiverDepth, cnt + 1);
            }
        }
    }

    private void CalculateAandB(Vector3 _s, Vector3 _e)
    {
        float x1 = _s.x, x2 = _e.x;
        float z1 = _s.z, z2 = _e.z;

        A = (z2 - z1) / (x2 - x1);
        B = (x2 * z1 - x1 * z2) / (x2 - x1);
    }
}
