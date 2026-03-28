#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    LevelEditor levelEditor;
    bool isEditMode = true;
    bool showGrid = true;
    LevelElementType paintBrush = LevelElementType.Floor;
    Vector2Int lastGridPos = new Vector2Int(-1, -1);
    Vector2 scroll;

    [MenuItem("Tools/Deadwood Level Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<LevelEditorWindow>("Deadwood Level Editor");
        w.minSize = new Vector2(240, 360);
        w.Show();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.delayCall += OnSelectionChanged;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode ||
            state == PlayModeStateChange.EnteredEditMode)
            levelEditor = null;

        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += OnSelectionChanged;
    }

    void OnSelectionChanged()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (Selection.activeGameObject != null)
        {
            var e = Selection.activeGameObject.GetComponent<LevelEditor>();
            if (e != null) levelEditor = e;
        }

        if (levelEditor == null)
        {
            var found = Object.FindObjectOfType<LevelEditor>();
            if (found != null && found.gameObject.scene.IsValid())
                levelEditor = found;
        }

        Repaint();
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField("Deadwood Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (levelEditor == null)
        {
            EditorGUILayout.HelpBox("场景中没有 LevelEditor。可打开 LevelEditor 场景，或点击下方创建。", MessageType.Warning);
            if (GUILayout.Button("创建 LevelEditor", GUILayout.Height(26)))
            {
                var go = new GameObject("LevelEditor");
                levelEditor = go.AddComponent<LevelEditor>();
                Undo.RegisterCreatedObjectUndo(go, "Create LevelEditor");
                Selection.activeGameObject = go;
            }

            EditorGUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.LabelField("目标关卡", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "地板/水/树会写入「已加载场景列表」里，场景名以左侧前缀开头且顺序最靠前的那一个（与 Hierarchy 中场景顺序一致）。例如先加载 LevelEditor，再 Additive 打开 Level 0，则写入 Level 0。",
            MessageType.Info);
        var so = new SerializedObject(levelEditor);
        so.Update();
        EditorGUILayout.PropertyField(so.FindProperty("levelSceneNamePrefix"), new GUIContent("场景名前缀"));
        so.ApplyModifiedProperties();

        EditorGUILayout.LabelField("当前解析", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(levelEditor.GetResolvedTargetSceneLabel(), EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("将活动场景设为备用", GUILayout.Height(22)))
            levelEditor.LockCurrentLevel();
        if (GUILayout.Button("刷新", GUILayout.Height(22)))
            levelEditor.RefreshElements();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("模式", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = isEditMode ? Color.green : Color.white;
        if (GUILayout.Button("编辑（左键铺瓦片 / 右键擦掉）", GUILayout.Height(26)))
        {
            isEditMode = true;
            SceneView.RepaintAll();
        }

        GUI.backgroundColor = !isEditMode ? Color.cyan : Color.white;
        if (GUILayout.Button("选择", GUILayout.Height(26)))
        {
            isEditMode = false;
            SceneView.RepaintAll();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("瓦片笔刷", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        DrawBrushButton(LevelElementType.Floor, "地板", new Color(0.4f, 0.85f, 0.45f));
        DrawBrushButton(LevelElementType.Water, "水", new Color(0.35f, 0.75f, 1f));
        DrawBrushButton(LevelElementType.Tree, "树", new Color(0.45f, 0.65f, 0.35f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
        if (GUILayout.Button("保存关卡场景", GUILayout.Height(28)))
            levelEditor.SaveScene();

        GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
        if (GUILayout.Button("清空全部瓦片", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("清空", "删除目标关卡中 FloorElements 下所有瓦片（地板/水/树）？", "确定", "取消"))
                levelEditor.ClearLevel();
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("网格", EditorStyles.boldLabel);
        showGrid = EditorGUILayout.Toggle("显示网格", showGrid);

        EditorGUI.BeginChangeCheck();
        var w = EditorGUILayout.IntField("宽度（格）", levelEditor.GridWidth);
        var h = EditorGUILayout.IntField("高度（格）", levelEditor.GridHeight);
        var cw = EditorGUILayout.FloatField("格宽", levelEditor.CellWidth);
        var ch = EditorGUILayout.FloatField("格高", levelEditor.CellHeight);
        var origin = EditorGUILayout.Vector3Field("原点", levelEditor.GridOrigin);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelEditor, "Grid");
            levelEditor.GridWidth = Mathf.Max(1, w);
            levelEditor.GridHeight = Mathf.Max(1, h);
            levelEditor.CellWidth = Mathf.Max(0.1f, cw);
            levelEditor.CellHeight = Mathf.Max(0.1f, ch);
            levelEditor.GridOrigin = origin;
            SceneView.RepaintAll();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("16×9"))
        {
            Undo.RecordObject(levelEditor, "Grid");
            levelEditor.GridWidth = 16;
            levelEditor.GridHeight = 9;
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("20×12"))
        {
            Undo.RecordObject(levelEditor, "Grid");
            levelEditor.GridWidth = 20;
            levelEditor.GridHeight = 12;
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("瓦片预制体", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        var floor = (GameObject)EditorGUILayout.ObjectField("地板", levelEditor.FloorPrefab, typeof(GameObject), false);
        var water = (GameObject)EditorGUILayout.ObjectField("水", levelEditor.WaterPrefab, typeof(GameObject), false);
        var tree = (GameObject)EditorGUILayout.ObjectField("树", levelEditor.TreePrefab, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelEditor, "Tile prefabs");
            levelEditor.FloorPrefab = floor;
            levelEditor.WaterPrefab = water;
            levelEditor.TreePrefab = tree;
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawBrushButton(LevelElementType type, string label, Color accent)
    {
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = paintBrush == type ? accent : Color.white;
        if (GUILayout.Button(label, GUILayout.Height(24)))
        {
            paintBrush = type;
            SceneView.RepaintAll();
        }

        GUI.backgroundColor = prev;
    }

    void OnSceneGUI(SceneView sv)
    {
        if (levelEditor == null) return;

        var e = Event.current;
        var mouseWorld = GuiPointOnGridPlane(e.mousePosition, levelEditor.GridOrigin);

        var local = mouseWorld - levelEditor.GridOrigin;
        var gridPos = new Vector2Int(
            Mathf.FloorToInt(local.x / levelEditor.CellWidth),
            Mathf.FloorToInt(local.y / levelEditor.CellHeight));

        var valid = gridPos.x >= 0 && gridPos.x < levelEditor.GridWidth &&
                    gridPos.y >= 0 && gridPos.y < levelEditor.GridHeight;

        if (showGrid)
            DrawGrid(levelEditor.GridOrigin, levelEditor.GridWidth, levelEditor.GridHeight,
                levelEditor.CellWidth, levelEditor.CellHeight);

        if (valid && isEditMode)
            DrawHover(gridPos, levelEditor.GridOrigin, levelEditor.CellWidth, levelEditor.CellHeight, paintBrush);

        if (!isEditMode) return;

        var gw = levelEditor.GridWidth;
        var gh = levelEditor.GridHeight;

        if (e.type == EventType.MouseDown && e.button == 0 && valid)
        {
            lastGridPos = gridPos;
            levelEditor.PlaceTileAt(gridPos, paintBrush);
            e.Use();
            sv.Repaint();
        }

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            if (valid && gridPos != lastGridPos && lastGridPos.x >= 0)
            {
                foreach (var c in CellsOnGridLine(lastGridPos, gridPos))
                {
                    if (c.x < 0 || c.x >= gw || c.y < 0 || c.y >= gh) continue;
                    levelEditor.PlaceTileAt(c, paintBrush);
                }

                lastGridPos = gridPos;
            }
            else if (valid && lastGridPos.x < 0)
            {
                lastGridPos = gridPos;
                levelEditor.PlaceTileAt(gridPos, paintBrush);
            }

            e.Use();
            sv.Repaint();
        }

        if (e.type == EventType.MouseUp && e.button == 0)
            lastGridPos = new Vector2Int(-1, -1);

        if (e.type == EventType.MouseDown && e.button == 1 && valid)
        {
            lastGridPos = gridPos;
            levelEditor.RemoveFloorAt(gridPos);
            e.Use();
            sv.Repaint();
        }

        if (e.type == EventType.MouseDrag && e.button == 1)
        {
            if (valid && gridPos != lastGridPos && lastGridPos.x >= 0)
            {
                foreach (var c in CellsOnGridLine(lastGridPos, gridPos))
                {
                    if (c.x < 0 || c.x >= gw || c.y < 0 || c.y >= gh) continue;
                    levelEditor.RemoveFloorAt(c);
                }

                lastGridPos = gridPos;
            }
            else if (valid && lastGridPos.x < 0)
            {
                lastGridPos = gridPos;
                levelEditor.RemoveFloorAt(gridPos);
            }

            e.Use();
            sv.Repaint();
        }

        if (e.type == EventType.MouseUp && e.button == 1)
            lastGridPos = new Vector2Int(-1, -1);
    }

    /// <summary>SceneView 鼠标射线与网格所在平面（过 GridOrigin、法线 forward）求交，避免仅用 ray.origin.z=0 在正交/斜视角下错位。</summary>
    static Vector3 GuiPointOnGridPlane(Vector2 guiMouse, Vector3 gridOriginOnPlane)
    {
        var ray = HandleUtility.GUIPointToWorldRay(guiMouse);
        var plane = new Plane(Vector3.forward, gridOriginOnPlane);
        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);
        return ray.origin;
    }

    /// <summary>栅格线段上所有格子（含端点），用于快速拖拽时补全中间格。</summary>
    static IEnumerable<Vector2Int> CellsOnGridLine(Vector2Int a, Vector2Int b)
    {
        int x0 = a.x, y0 = a.y, x1 = b.x, y1 = b.y;
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    static void DrawGrid(Vector3 origin, int width, int height, float cellW, float cellH)
    {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        for (var x = 0; x <= width; x++)
        {
            var a = origin + new Vector3(x * cellW, 0f, 0f);
            var b = origin + new Vector3(x * cellW, height * cellH, 0f);
            Handles.DrawLine(a, b);
        }

        for (var y = 0; y <= height; y++)
        {
            var a = origin + new Vector3(0f, y * cellH, 0f);
            var b = origin + new Vector3(width * cellW, y * cellH, 0f);
            Handles.DrawLine(a, b);
        }

        Handles.color = new Color(0.3f, 0.7f, 1f, 0.8f);
        Handles.DrawLine(origin, origin + new Vector3(width * cellW, 0f, 0f));
        Handles.DrawLine(origin + new Vector3(width * cellW, 0f, 0f), origin + new Vector3(width * cellW, height * cellH, 0f));
        Handles.DrawLine(origin + new Vector3(width * cellW, height * cellH, 0f), origin + new Vector3(0f, height * cellH, 0f));
        Handles.DrawLine(origin + new Vector3(0f, height * cellH, 0f), origin);
    }

    static void DrawHover(Vector2Int gridPos, Vector3 origin, float cellW, float cellH, LevelElementType brush)
    {
        var center = origin + new Vector3(
            gridPos.x * cellW + cellW / 2f,
            gridPos.y * cellH + cellH / 2f,
            0f);
        Color fill;
        Color outline;
        switch (brush)
        {
            case LevelElementType.Water:
                fill = new Color(0.35f, 0.75f, 1f, 0.28f);
                outline = new Color(0.2f, 0.55f, 1f);
                break;
            case LevelElementType.Tree:
                fill = new Color(0.35f, 0.7f, 0.25f, 0.28f);
                outline = new Color(0.2f, 0.5f, 0.15f);
                break;
            default:
                fill = new Color(0.5f, 1f, 0.5f, 0.25f);
                outline = Color.green;
                break;
        }
        Handles.DrawSolidRectangleWithOutline(
            new[]
            {
                center + new Vector3(-cellW / 2f, -cellH / 2f, 0f),
                center + new Vector3(cellW / 2f, -cellH / 2f, 0f),
                center + new Vector3(cellW / 2f, cellH / 2f, 0f),
                center + new Vector3(-cellW / 2f, cellH / 2f, 0f)
            },
            fill,
            outline);
    }
}
#endif
