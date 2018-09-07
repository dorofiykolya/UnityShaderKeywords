using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Dorofiy.Shaders
{
  public class ShaderKeywordEditorWindow : EditorWindow
  {
    private static readonly string Key = typeof(ShaderKeywordEditorWindow).FullName + ".Key";

    [MenuItem("Window/ShaderKeywords")]
    public static void Open()
    {
      var window = GetWindow<ShaderKeywordEditorWindow>("ShaderKeywords");
      window.Keywords = EditorPrefs.GetString(Key);
      window.Show(true);
    }

    private const string Separator = "|||";
    private static readonly List<string> RemoveKeys = new List<string>();

    public string Keywords { get; set; }

    private string[] _keys;
    private string _newKey;
    private Vector2 _scroll;

    private void OnGUI()
    {
      if (_keys == null && Keywords != null)
      {
        _keys = Keywords.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
      }
      var needUpdate = false;
      _scroll = EditorGUILayout.BeginScrollView(_scroll);
      EditorGUILayout.BeginVertical();

      if (GUILayout.Button("Collect"))
      {
        var files = AssetDatabase.FindAssets("t:Shader").Select(AssetDatabase.GUIDToAssetPath);
        var set = new HashSet<string>();
        var pragma = "#pragma multi_compile ";
        foreach (var file in files)
        {
          var shader = AssetDatabase.LoadAssetAtPath<Shader>(file) as Shader;
          if (shader != null)
          {
            var lines = File.ReadAllLines(file);
            foreach (var sourceLine in lines)
            {
              var line = sourceLine.Trim();
              if (line.StartsWith(pragma))
              {
                var str = line.Remove(0, pragma.Length);
                var keys = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var key in keys)
                {
                  set.Add(key.Trim());
                }
              }
            }
          }
        }

        _keys = set.ToArray();
      }
      if (_keys != null)
      {
        foreach (var key in _keys)
        {
          EditorGUILayout.BeginHorizontal();
          var value = Shader.IsKeywordEnabled(key);
          var newValue = EditorGUILayout.ToggleLeft(key, value);
          if (newValue != value)
          {
            if (newValue) Shader.EnableKeyword(key);
            else Shader.DisableKeyword(key);

            needUpdate = true;
          }
          if (GUILayout.Button("", "OL Minus"))
          {
            RemoveKeys.Add(key);
          }
          EditorGUILayout.EndHorizontal();
        }
      }
      EditorGUILayout.Separator();
      EditorGUILayout.BeginHorizontal("ProgressBarBack");
      _newKey = EditorGUILayout.TextField(_newKey);
      if (GUILayout.Button("", (GUIStyle)"OL Plus", GUILayout.Width(50f)))
      {
        if (_keys == null) _keys = new string[0];
        ArrayUtility.Add(ref _keys, _newKey);
        needUpdate = true;
      }
      EditorGUILayout.EndHorizontal();
      foreach (var removeKey in RemoveKeys)
      {
        ArrayUtility.Remove(ref _keys, removeKey);
        needUpdate = true;
      }
      RemoveKeys.Clear();
      if (needUpdate)
      {
        Keywords = string.Join(Separator, _keys);
        EditorPrefs.SetString(Key, Keywords);
      }
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndScrollView();
    }
  }
}