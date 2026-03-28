using UnityEngine;

public enum LevelElementType
{
    Empty,
    Floor,
    Water,
    Tree
}

public class LevelElementMarker : MonoBehaviour
{
    public LevelElementType elementType = LevelElementType.Floor;
    public Vector2Int gridPosition;
}
