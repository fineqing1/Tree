using UnityEngine;

public enum LevelElementType
{
    Empty,
    Floor
}

public class LevelElementMarker : MonoBehaviour
{
    public LevelElementType elementType = LevelElementType.Floor;
    public Vector2Int gridPosition;
}
