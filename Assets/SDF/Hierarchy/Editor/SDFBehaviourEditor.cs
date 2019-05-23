using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SDF.Hierarchy {
  using NodeType = SDFBehaviour.NodeType;
  using Category = SDFBehaviour.CategoryAttribute;

  [CanEditMultipleObjects]
  [CustomEditor(typeof(SDFBehaviour))]
  public class SDFBehaviourEditor : Editor {

    private static GUIContent[] NodeTypeTitles;
    private static Dictionary<string, NodeType[]> PropToNodeTypeMap;

    private void OnEnable() {
      if (NodeTypeTitles == null) {
        PropToNodeTypeMap = typeof(SDFBehaviour).
          GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
          Select(f => (field: f, attr: f.GetCustomAttribute<SDFBehaviour.DisplayForAttribute>())).
          Where(t => t.attr != null).
          ToDictionary(t => t.field.Name, t => t.attr.Types);

        NodeTypeTitles = (Enum.GetValues(typeof(NodeType)) as NodeType[]).
          Select(e => (val: e, attr: typeof(NodeType).GetMember(e.ToString())[0].GetCustomAttribute<Category>())).
          Where(e => e.attr != null).
          Select(e => e.attr.Category + "/" + ObjectNames.NicifyVariableName(e.val.ToString())).
          Select(s => new GUIContent(s)).
          ToArray();
      }
    }

    public override void OnInspectorGUI() {
      EditorGUI.BeginDisabledGroup(true);
      EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginChangeCheck();

      var nodeTypeProp = serializedObject.FindProperty(SDFBehaviour.NODE_TYPE_PROPERTY);

      //Draw the node type index
      {
        int index;
        if (nodeTypeProp.hasMultipleDifferentValues) {
          index = 0;
          EditorGUI.showMixedValue = true;
        } else {
          index = nodeTypeProp.enumValueIndex;
        }

        EditorGUI.BeginChangeCheck();

        int newIndex = EditorGUILayout.Popup(EditorGUIUtility.TrTempContent(nodeTypeProp.displayName),
                                             index,
                                             NodeTypeTitles);

        if (EditorGUI.EndChangeCheck()) {
          nodeTypeProp.enumValueIndex = newIndex;
        }

        EditorGUI.showMixedValue = false;
      }

      var nodeType = (NodeType)nodeTypeProp.intValue;

      var it = serializedObject.GetIterator();
      it.NextVisible(enterChildren: true);
      while (it.NextVisible(enterChildren: false)) {
        //We already drew the node type property
        if (SerializedProperty.EqualContents(it, nodeTypeProp)) {
          continue;
        }

        bool shouldDraw = true;

        NodeType[] types = null;
        if (PropToNodeTypeMap.TryGetValue(it.name, out types)) {
          if (nodeTypeProp.hasMultipleDifferentValues) {
            shouldDraw = false;
          } else {
            shouldDraw = types.Contains(nodeType);
          }
        }

        if (shouldDraw) {
          EditorGUILayout.PropertyField(it);
        }
      }

      if (EditorGUI.EndChangeCheck()) {
        serializedObject.ApplyModifiedProperties();
      }
    }
  }
}
