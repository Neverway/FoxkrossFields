using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(StructureTemplate))]
public class StructureTemplateEditor : Editor
{
    private const int CellSize = 28;
    private const int GridPadding = 10;
    private const int GridRadius = 8;

    private string selectedTileID = "cliff";
    private TileLayers selectedLayer = TileLayers.Ground;
    private bool selectedIsWarp = false;
    private string selectedWarpTarget = "AutumnCaves";
    private bool erasing = false;

    private readonly string[] paletteIDs = new[]
    {
        "cliff", "cliff_top", "cave_entrance_warp", "ground", "water", "tree", "leaf"
    };

    private bool showTileList = false;

    public override void OnInspectorGUI()
    {
        var template = (StructureTemplate)target;
        serializedObject.Update();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Structure Template Editor", EditorStyles.boldLabel);
        template.id = EditorGUILayout.TextField("Structure ID", template.id);
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile:", GUILayout.Width(30));
        foreach (var id in paletteIDs)
        {
            bool isSelected = selectedTileID == id && !erasing;
            var style = isSelected ? GetSelectedButtonStyle() : GUI.skin.button;
            if (GUILayout.Button(id, style, GUILayout.Height(22)))
            {
                selectedTileID = id;
                erasing = false;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Custom:", GUILayout.Width(52));
        selectedTileID = EditorGUILayout.TextField(selectedTileID, GUILayout.Width(120));
        GUILayout.FlexibleSpace();
        var eraseStyle = erasing ? GetSelectedButtonStyle() : GUI.skin.button;
        if (GUILayout.Button("✕ Erase", eraseStyle, GUILayout.Width(70), GUILayout.Height(22)))
            erasing = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        selectedLayer = (TileLayers)EditorGUILayout.EnumPopup("Layer", selectedLayer);

        selectedIsWarp = EditorGUILayout.Toggle("Is Warp Tile", selectedIsWarp);
        if (selectedIsWarp)
            selectedWarpTarget = EditorGUILayout.TextField("Warp Target Env", selectedWarpTarget);

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Grid  (click to paint, right-click to erase)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Origin (0,0) is the anchor. Multiple layers can stack per cell. Erase only affects the active layer.", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        DrawGrid(template);

        EditorGUILayout.Space(8);

        showTileList = EditorGUILayout.Foldout(showTileList, $"Tile Data ({template.tiles?.Length ?? 0} tiles)");
        if (showTileList)
        {
            EditorGUI.indentLevel++;
            if (template.tiles != null)
            {
                foreach (var t in template.tiles)
                {
                    string label = t.isWarp
                        ? $"({t.dx},{t.dy}) WARP → {t.warpTargetEnvironment}"
                        : $"({t.dx},{t.dy}) [{(TileLayers)t.layer}] {t.tileID}";
                    EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
                }
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Clear All Tiles", GUILayout.Height(24)))
        {
            if (EditorUtility.DisplayDialog("Clear Structure", "Remove all tiles?", "Yes", "Cancel"))
            {
                template.tiles = new StructureTile[0];
                EditorUtility.SetDirty(target);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGrid(StructureTemplate template)
    {
        int gridDiameter = GridRadius * 2 + 1;
        int gridPixelSize = gridDiameter * CellSize + GridPadding * 2;

        Rect gridRect = GUILayoutUtility.GetRect(gridPixelSize, gridPixelSize + 20);
        gridRect.x += GridPadding;
        gridRect.y += GridPadding;

        if (Event.current.type == EventType.Repaint)
            EditorGUI.DrawRect(new Rect(gridRect.x - 2, gridRect.y - 2, gridDiameter * CellSize + 4, gridDiameter * CellSize + 4), new Color(0.15f, 0.15f, 0.15f));

        var tileMap = BuildTileMap(template);

        for (int gridY = GridRadius; gridY >= -GridRadius; gridY--)
        {
            for (int gridX = -GridRadius; gridX <= GridRadius; gridX++)
            {
                int pixelX = (int)gridRect.x + (gridX + GridRadius) * CellSize;
                int pixelY = (int)gridRect.y + (GridRadius - gridY) * CellSize;
                Rect cellRect = new Rect(pixelX, pixelY, CellSize - 1, CellSize - 1);

                bool isOrigin = gridX == 0 && gridY == 0;
                tileMap.TryGetValue((gridX, gridY), out var tilesAtCell);

                bool hasTile = tilesAtCell != null && tilesAtCell.Count > 0;

                StructureTile topTile = hasTile
                    ? tilesAtCell.OrderByDescending(t => t.layer).First()
                    : null;

                Color bgColor;
                if (isOrigin && !hasTile)
                    bgColor = new Color(0.3f, 0.3f, 0.1f);
                else if (hasTile)
                    bgColor = GetTileColor(topTile);
                else
                    bgColor = (gridX + gridY) % 2 == 0 ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.25f, 0.25f, 0.25f);

                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(cellRect, bgColor);

                    if (hasTile)
                    {
                        // Abbreviate the top tile ID; show a +N badge if more layers are stacked
                        string label = topTile.isWarp ? "W" : Abbrev(topTile.tileID);
                        if (tilesAtCell.Count > 1) label += $"\n+{tilesAtCell.Count - 1}";
                        GUI.Label(cellRect, label, GetCellLabelStyle());
                    }
                    else if (isOrigin)
                    {
                        GUI.Label(cellRect, "✦", GetCellLabelStyle());
                    }
                }

                if (cellRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.Repaint)
                        EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 1f, 0.08f));

                    bool leftClick = Event.current.type == EventType.MouseDown && Event.current.button == 0;
                    bool rightClick = Event.current.type == EventType.MouseDown && Event.current.button == 1;

                    if (leftClick || rightClick)
                    {
                        if (erasing || rightClick)
                            RemoveTile(template, gridX, gridY);
                        else
                            SetTile(template, gridX, gridY);

                        EditorUtility.SetDirty(target);
                        Event.current.Use();
                        Repaint();
                    }
                }
            }
        }

        if (Event.current.type == EventType.Repaint)
        {
            for (int gridX = -GridRadius; gridX <= GridRadius; gridX += 2)
            {
                int pixelX = (int)gridRect.x + (gridX + GridRadius) * CellSize;
                int pixelY = (int)gridRect.y + gridDiameter * CellSize + 2;
                GUI.Label(new Rect(pixelX, pixelY, CellSize * 2, 14), gridX.ToString(), EditorStyles.centeredGreyMiniLabel);
            }
        }
    }

    private Dictionary<(int, int), List<StructureTile>> BuildTileMap(StructureTemplate template)
    {
        var map = new Dictionary<(int, int), List<StructureTile>>();
        if (template.tiles == null) return map;
        foreach (var t in template.tiles)
        {
            var key = (t.dx, t.dy);
            if (!map.ContainsKey(key)) map[key] = new List<StructureTile>();
            map[key].Add(t);
        }
        return map;
    }

    private void SetTile(StructureTemplate template, int dx, int dy)
    {
        var list = template.tiles?.ToList() ?? new List<StructureTile>();

        var existing = list.FirstOrDefault(t => t.dx == dx && t.dy == dy && t.layer == (int)selectedLayer);
        if (existing != null) list.Remove(existing);

        list.Add(new StructureTile
        {
            dx = dx,
            dy = dy,
            tileID = selectedTileID,
            layer = (int)selectedLayer,
            isWarp = selectedIsWarp,
            warpTargetEnvironment = selectedIsWarp ? selectedWarpTarget : ""
        });

        template.tiles = list.ToArray();
    }

    private void RemoveTile(StructureTemplate template, int dx, int dy)
    {
        if (template.tiles == null) return;
        template.tiles = template.tiles
            .Where(t => !(t.dx == dx && t.dy == dy && t.layer == (int)selectedLayer))
            .ToArray();
    }

    private string Abbrev(string id) => id.Length > 4 ? id.Substring(0, 4) : id;

    private Color GetTileColor(StructureTile tile)
    {
        if (tile.isWarp) return new Color(0.6f, 0.2f, 0.8f);
        return tile.tileID switch
        {
            "cliff"     => new Color(0.35f, 0.28f, 0.20f),
            "cliff_top" => new Color(0.25f, 0.45f, 0.20f),
            "ground"    => new Color(0.30f, 0.55f, 0.25f),
            "water"     => new Color(0.15f, 0.35f, 0.65f),
            "tree"      => new Color(0.10f, 0.30f, 0.10f),
            "leaf"      => new Color(0.20f, 0.50f, 0.15f),
            _           => new Color(0.40f, 0.40f, 0.40f),
        };
    }

    private GUIStyle GetCellLabelStyle() => new GUIStyle(EditorStyles.miniLabel)
    {
        alignment = TextAnchor.MiddleCenter,
        fontSize = 8,
        normal = { textColor = Color.white }
    };

    private GUIStyle GetSelectedButtonStyle()
    {
        var style = new GUIStyle(GUI.skin.button);
        style.normal.background = MakeTex(2, 2, new Color(0.3f, 0.6f, 1f, 0.8f));
        style.normal.textColor = Color.white;
        return style;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        var tex = new Texture2D(width, height);
        tex.SetPixels(Enumerable.Repeat(col, width * height).ToArray());
        tex.Apply();
        return tex;
    }
}