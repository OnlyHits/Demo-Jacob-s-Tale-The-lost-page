using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableLabelBrowser : EditorWindow
{
    private Vector2 _scroll;
    private Dictionary<string, List<AddressableAssetEntry>> _labelToAssets = new();
    private Dictionary<string, bool> _foldouts = new();

    [MenuItem("Tools/Addressables/Label Browser")]
    public static void ShowWindow()
    {
        GetWindow<AddressableLabelBrowser>("Addressable Label Browser");
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            Refresh();
        }

        if (_labelToAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("No addressable assets found.", MessageType.Info);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var kvp in _labelToAssets)
        {
            string label = kvp.Key;
            List<AddressableAssetEntry> assets = kvp.Value;

            _foldouts.TryAdd(label, true);
            _foldouts[label] = EditorGUILayout.Foldout(_foldouts[label], $"{label} ({assets.Count})", true);

            if (_foldouts[label])
            {
                EditorGUI.indentLevel++;
                foreach (var asset in assets)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Name:", asset.MainAsset?.name ?? "Unnamed");
                    EditorGUILayout.LabelField("Path:", asset.AssetPath);
                    EditorGUILayout.LabelField("Type:", asset.MainAsset?.GetType().Name ?? "Unknown");
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Refresh()
    {
        _labelToAssets.Clear();

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("No AddressableAssetSettings found.");
            return;
        }

        foreach (var group in settings.groups)
        {
            if (group == null) continue;

            foreach (var entry in group.entries)
            {
                foreach (var label in entry.labels)
                {
                    if (!_labelToAssets.TryGetValue(label, out var list))
                    {
                        list = new List<AddressableAssetEntry>();
                        _labelToAssets[label] = list;
                    }
                    list.Add(entry);
                }
            }
        }
    }
}
