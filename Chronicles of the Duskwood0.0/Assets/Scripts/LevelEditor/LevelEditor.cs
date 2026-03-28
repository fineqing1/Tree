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

    [Header("Floor")]
    [SerializeField] private GameObject floorTile;

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

#if UNITY_EDITOR
    public SceneAsset TargetLevelScene { get => targetLevelScene; set => targetLevelScene = value; }
#endif

    readonly Dictionary<Vector2Int, GameObject> floorObjects = new Dictionary<Vector2Int, GameObject>();
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
            floorObjects.Clear();
        }
#else
        floorObjects.Clear();
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
            floorObjects.Clear();
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
        floorObjects.Clear();
        FindFloorParent();
        if (floorParent == null) return;

        foreach (Transform child in floorParent)
        {
            var marker = child.GetComponent<LevelElementMarker>();
            if (marker != null)
                floorObjects[marker.gridPosition] = child.gameObject;
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

    void SetFloorSpriteSorting(GameObject obj, int gridY)
    {
        int order = -1 - gridY;
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>(true))
            sr.sortingOrder = order;
    }
#endif

    public void PlaceFloorAt(Vector2Int pos)
    {
#if UNITY_EDITOR
        if (!IsValidGridPosition(pos)) return;
        if (HasFloorAt(pos)) return;

        if (floorTile == null)
        {
            Debug.LogWarning("Floor prefab is not assigned.");
            return;
        }

        EnsureFloorParentInTargetScene();

        Vector3 worldPos = GridToWorld(pos);
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(floorTile);
        obj.transform.SetParent(floorParent);
        obj.transform.position = worldPos;
        obj.name = $"Floor_{pos.x}_{pos.y}";

        var marker = obj.GetComponent<LevelElementMarker>();
        if (marker == null) marker = obj.AddComponent<LevelElementMarker>();
        marker.elementType = LevelElementType.Floor;
        marker.gridPosition = pos;

        SetFloorSpriteSorting(obj, pos.y);

        floorObjects[pos] = obj;

        Scene targetScene = ResolveTargetScene();
        if (targetScene.IsValid()) EditorSceneManager.MarkSceneDirty(targetScene);
#endif
    }

    public void RemoveFloorAt(Vector2Int pos)
    {
#if UNITY_EDITOR
        if (floorObjects.TryGetValue(pos, out GameObject obj))
        {
            GameObject toDestroy = obj;
            floorObjects.Remove(pos);

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
        if (floorObjects.ContainsKey(pos)) return true;

#if UNITY_EDITOR
        EnsureFloorParentInTargetScene();
        if (floorParent != null)
        {
            foreach (Transform child in floorParent)
            {
                var marker = child.GetComponent<LevelElementMarker>();
                if (marker != null && marker.gridPosition == pos)
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

        floorObjects.Clear();

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
