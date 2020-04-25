using UnityEngine;

[CreateAssetMenu(fileName = "New Field Map Data", menuName = "FieldData/Field Map")]
public class FieldMapData : ScriptableObject
{
    [System.Serializable]
    public struct SplatHeights
    {
        public int textureIndex;
        public float startingHeight;
    }
    // Terrain의 가로, 세로, 깊이, 높낮이 규모 정의
    /* Terrain의 가로, 세로 정의의 경우, 2의 제곱 수로 정의해야 함 */
    public int Width, Height, Depth, Scale;
    public float HeightLimit;
    public Terrain FieldTerrain;
    public SplatHeights[] TexBound;
}
