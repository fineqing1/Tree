using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class LevelEditor : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 12;
    [SerializeField] private float cellWidth = 1f;
    [SerializeField] private float cellHeight = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Tiles")]
    [SerializeField] private GameObject floorTile;
    [SerializeField] private GameObject waterTile;
    [SerializeField] private GameObject treeTile;

    [Header("Target level scene")]
    [Tooltip("在已加载场景列表中（顺序同 Hierarchy），取第一个场景名以此字符串开头的场景作为铺地目标，例如「Level 0」「Level 1」对应前缀「Level 」。")]
    [SerializeField] private string levelSceneNamePrefix = "Level ";

#if UNITY_EDITOR
    [Tooltip("可选备用：当前缀匹配不到任何已加载场景时，再尝试按此资源路径解析；仍失败则回退为 LevelEditor 所在场景。")]
    [SerializeField] private SceneAsset targetLevelScene;
#endif

    public int GridWidth { get => gridWidth; set => gridWidth = value; }
    public int GridHeight { get => gridHeight; set => gridHeight = value; }
    public float CellWidth { get => cellWidth; set => cellWidth = value; }
    public float CellHeight { get => cellHeight; set => cellHeight = value; }
    public Vector3 GridOrigin { get => gridOrigin; set => gridOrigin = value; }
    public GameObject FloorPrefab { get => floorTile; set => floorTile = value; }
    public GameObject WaterPrefab { get => waterTile; set => waterTile = value; }
    public GameObject TreePrefab { get => treeTile; set => treeTile = value; }

#if UNITY_EDITOR
    public SceneAsset TargetLevelScene { get => targetLevelScene; set => targetLevelScene = value; }
#endif

    readonly Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    Transform floorParent;

    void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            FindFloorParent();
            ScanExistingFloors();
        }
        else
        {
            cellObjects.Clear();
        }
#else
        cellObjects.Clear();
#endif
    }

#if UNITY_EDITOR
    void OnDestroy()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            cellObjects.Clear();
            floorParent = null;
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;

        EditorApplication.delayCall += () =>
        {
            if (this == null || gameObject == null) return;
            if (Application.isPlaying) return;
            FindFloorParent();
            ScanExistingFloors();
        };
    }
#endif

    void FindFloorParent()
    {
        floorParent = null;
        Scene targetScene = ResolveTargetScene();
        if (!targetScene.IsValid() || !targetScene.isLoaded) return;

        foreach (var obj in targetScene.GetRootGameObjects())
        {
            if (obj.name == "FloorElements")
            {
                floorParent = obj.transform;
                return;
            }
        }
    }

    /// <summary>第一个已加载且场景名以 <see cref="levelSceneNamePrefix"/> 开头的场景；否则按备用 SceneAsset；再否则为 LevelEditor 所在场景。</summary>
    Scene ResolveTargetScene()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(levelSceneNamePrefix))
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                if (scene.name.StartsWith(levelSceneNamePrefix, StringComparison.Ordinal))
                    return scene;
            }
        }

        if (targetLevelScene != null)
        {
            string scenePath = AssetDatabase.GetAssetPath(targetLevelScene);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path == scenePath) return scene;
            }
        }
#endif
        return gameObject.scene;
    }

#if UNITY_EDITOR
    public string GetResolvedTargetSceneLabel()
    {
        var s = ResolveTargetScene();
        if (!s.IsValid() || !s.isLoaded) return "(无效)";
        bool byPrefix = !string.IsNullOrEmpty(levelSceneNamePrefix) &&
                        s.name.StartsWith(levelSceneNamePrefix, StringComparison.Ordinal);
        return byPrefix ? $"{s.name}（前缀匹配）" : $"{s.name}（备用/回退）";
    }
#endif

#if UNITY_EDITOR
    public void EnsureTargetSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded) continue;
            if (!string.IsNullOrEmpty(levelSceneNamePrefix) &&
                scene.name.StartsWith(levelSceneNamePrefix, StringComparison.Ordinal))
                return;
        }

        if (targetLevelScene == null)
        {
            Debug.LogWarning(
                $"未找到已加载且场景名以「{levelSceneNamePrefix}」开头的场景。请先 Additive 打开关卡（如 Level 0），或在组件上指定备用的 Target Level Scene。");
            return;
        }

        string scenePath = AssetDatabase.GetAssetPath(targetLevelScene);
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == scenePath) return;
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
    }
#endif

    void ScanExistingFloors()
    {
        cellObjects.Clear();
        FindFloorParent();
        if (floorParent == null) return;

        foreach (Transform child in floorParent)
        {
            var marker = child.GetComponent<LevelElementMarker>();
            if (marker != null && marker.elementType != LevelElementType.Empty)
                cellObjects[marker.gridPosition] = child.gameObject;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 a = gridOrigin + new Vector3(x * cellWidth, 0, 0);
            Vector3 b = gridOrigin + new Vector3(x * cellWidth, gridHeight * cellHeight, 0);
            Gizmos.DrawLine(a, b);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 a = gridOrigin + new Vector3(0, y * cellHeight, 0);
            Vector3 b = gridOrigin + new Vector3(gridWidth * cellWidth, y * cellHeight, 0);
            Gizmos.DrawLine(a, b);
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(
            gridPos.x * cellWidth + cellWidth / 2f,
            gridPos.y * cellHeight + cellHeight / 2f,
            0f);
    }

    bool IsValidGridPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    /// <summary>
    /// 从 <see cref="cellObjects"/> 取格子上的实例。若引用已被 Undo/销毁但字典未更新，视为无物体并移除陈旧项
    /// （避免对已销毁对象调用 GetComponent 触发 MissingReferenceException）。
    /// </summary>
    bool TryGetLiveTileFromCache(Vector2Int pos, out GameObject go)
    {
        go = null;
        if (!cellObjects.TryGetValue(pos, out go))
            return false;
        if (go != null)
            return true;
        cellObjects.Remove(pos);
        return false;
    }

#if UNITY_EDITOR
    void EnsureFloorParentInTargetScene()
    {
        if (floorParent != null) return;

        Scene targetScene = ResolveTargetScene();
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            EnsureTargetSceneLoaded();
            targetScene = ResolveTargetScene();
        }

        if (!targetScene.IsValid()) return;

        foreach (var obj in targetScene.GetRootGameObjects())
        {
            if (obj.name == "FloorElements")
            {
                floorParent = obj.transform;
                return;
            }
        }

        var parentObj = new GameObject("FloorElements");
        SceneManager.MoveGameObjectToScene(parentObj, targetScene);
        floorParent = parentObj.transform;
        EditorSceneManager.MarkSceneDirty(targetScene);
    }

    void SetTileSpriteSorting(GameObject obj, int gridY)
    {
        int order = -1 - gridY;
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingOrder = order;
    }

    GameObject PrefabForType(LevelElementType type)
    {
        switch (type)
        {
            case LevelElementType.Floor: return floorTile;
            case LevelElementType.Water: return waterTile;
            case LevelElementType.Tree: return treeTile;
            default: return null;
        }
    }

    static string TileNamePrefix(LevelElementType type)
    {
        switch (type)
        {
            case LevelElementType.Floor: return "Floor";
            case LevelElementType.Water: return "Water";
            case LevelElementType.Tree: return "Tree";
            default: return "Tile";
        }
    }

    /// <summary>同步删除格子上的瓦片（用于替换类型时立即腾出格子）。</summary>
    void RemoveTileAtImmediate(Vector2Int pos)
    {
        GameObject obj = null;
        if (!TryGetLiveTileFromCache(pos, out obj))
        {
            EnsureFloorParentInTargetScene();
            if (floorParent == null) return;
            foreach (Transform child in floorParent)
            {
                var marker = child.GetComponent<LevelElementMarker>();
                if (marker != null && marker.gridPosition == pos)
                {
                    obj = child.gameObject;
                    break;
                }
            }
        }

        if (obj == null) return;

        cellObjects.Remove(pos);
        if (Selection.activeGameObject == obj)
            Selection.activeGameObject = null;

        Undo.DestroyObjectImmediate(obj);
    }

    /// <summary>在格子上放置指定类型瓦片；若已有其他类型会先替换。</summary>
    public void PlaceTileAt(Vector2Int pos, LevelElementType type)
    {
        if (type == LevelElementType.Empty) return;
        if (!IsValidGridPosition(pos)) return;

        var prefab = PrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"{type} prefab is not assigned on LevelEditor.");
            return;
        }

        EnsureFloorParentInTargetScene();
        if (floorParent == null) return;

        if (TryGetLiveTileFromCache(pos, out GameObject existing))
        {
            var em = existing.GetComponent<LevelElementMarker>();
            if (em != null && em.elementType == type)
                return;
        }
        else
        {
            foreach (Transform child in floorParent)
            {
                var em = child.GetComponent<LevelElementMarker>();
                if (em != null && em.gridPosition == pos)
                {
                    if (em.elementType == type)
                        return;
                    break;
                }
            }
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName($"Place {type}");
        int undoGroup = Undo.GetCurrentGroup();

        RemoveTileAtImmediate(pos);

        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(obj, $"Place {type}");
        obj.transform.SetParent(floorParent);
        obj.transform.position = worldPos;
        obj.name = $"{TileNamePrefix(type)}_{pos.x}_{pos.y}";

        var marker = obj.GetComponent<LevelElementMarker>();
        if (marker == null) marker = obj.AddComponent<LevelElementMarker>();
        marker.elementType = type;
        marker.gridPosition = pos;

        SetTileSpriteSorting(obj, pos.y);

        cellObjects[pos] = obj;

        Undo.CollapseUndoOperations(undoGroup);

        Scene targetScene = ResolveTargetScene();
        if (targetScene.IsValid()) EditorSceneManager.MarkSceneDirty(targetScene);
    }
#endif

    public void PlaceFloorAt(Vector2Int pos)
    {
#if UNITY_EDITOR
        PlaceTileAt(pos, LevelElementType.Floor);
#endif
    }

    public void RemoveFloorAt(Vector2Int pos)
    {
#if UNITY_EDITOR
        if (TryGetLiveTileFromCache(pos, out GameObject obj))
        {
            GameObject toDestroy = obj;
            cellObjects.Remove(pos);

            if (Selection.activeGameObject == toDestroy)
                Selection.activeGameObject = null;

            EditorApplication.delayCall += () =>
            {
                if (toDestroy != null)
                    Undo.DestroyObjectImmediate(toDestroy);
            };

            Scene targetScene = ResolveTargetScene();
            if (targetScene.IsValid()) EditorSceneManager.MarkSceneDirty(targetScene);
            return;
        }

        EnsureFloorParentInTargetScene();
        if (floorParent == null) return;

        foreach (Transform child in floorParent)
        {
            var marker = child.GetComponent<LevelElementMarker>();
            if (marker != null && marker.gridPosition == pos)
            {
                GameObject toDestroy = child.gameObject;

                if (Selection.activeGameObject == toDestroy)
                    Selection.activeGameObject = null;

                EditorApplication.delayCall += () =>
                {
                    if (toDestroy != null)
                        Undo.DestroyObjectImmediate(toDestroy);
                };

                Scene targetScene = ResolveTargetScene();
                if (targetScene.IsValid()) EditorSceneManager.MarkSceneDirty(targetScene);
                return;
            }
        }
#endif
    }

    public bool HasFloorAt(Vector2Int pos)
    {
        if (TryGetLiveTileFromCache(pos, out _))
            return true;

#if UNITY_EDITOR
        EnsureFloorParentInTargetScene();
        if (floorParent != null)
        {
            foreach (Transform child in floorParent)
            {
                var marker = child.GetComponent<LevelElementMarker>();
                if (marker != null && marker.elementType != LevelElementType.Empty &&
                    marker.gridPosition == pos)
                    return true;
            }
        }
#endif
        return false;
    }

    public void ClearLevel()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        FindFloorParent();
        if (floorParent != null)
        {
            for (int i = floorParent.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(floorParent.GetChild(i).gameObject);
        }

        cellObjects.Clear();

        Scene targetScene = ResolveTargetScene();
        if (targetScene.IsValid())
            EditorSceneManager.MarkSceneDirty(targetScene);
#endif
    }

    public void SaveScene()
    {
#if UNITY_EDITOR
        Scene targetScene = ResolveTargetScene();
        if (targetScene.IsValid())
        {
            EditorSceneManager.SaveScene(targetScene);
            Debug.Log($"Saved scene: {targetScene.name}");
        }
        else
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
#endif
    }

    public void LockCurrentLevel()
    {
#if UNITY_EDITOR
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            string scenePath = activeScene.path;
            if (!string.IsNullOrEmpty(scenePath))
            {
                targetLevelScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                FindFloorParent();
                ScanExistingFloors();
                Debug.Log($"Locked to level: {activeScene.name}");
            }
        }
#endif
    }

    public void RefreshElements()
    {
        FindFloorParent();
        ScanExistingFloors();
    }
}
