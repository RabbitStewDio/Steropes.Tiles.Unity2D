using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace UnityToolbag
{
    [CustomPropertyDrawer(typeof(SerializableSortingLayer))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty rawProperty, GUIContent label)
        {
            var idProperty = rawProperty.FindPropertyRelative("id");
            if (idProperty == null)
            {
                EditorGUI.HelpBox(position, string.Format("{0} INVALID.", rawProperty.name), MessageType.Error);
                return;
            }

            var sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            if (idProperty.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.HelpBox(position, string.Format("{0} is not an integer but has [SortingLayer].", rawProperty.name), MessageType.Error);
            }
            else
            {
                EditorGUI.BeginProperty(position, label, rawProperty);
                
                // Look up the layer name using the current layer ID
                var oldName = SortingLayer.IDToName(idProperty.intValue);

                // Use the name to look up our array index into the names list
                var oldLayerIndex = Array.IndexOf(sortingLayerNames, oldName);
                if (oldLayerIndex == -1)
                {
                    // always exists.
                    oldLayerIndex = Array.IndexOf(sortingLayerNames, "Default");
                }

                // Show the popup for the names
                var newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

                // If the index changes, look up the ID for the new index to store as the new ID
                if (newLayerIndex != oldLayerIndex)
                {
                    Debug.Log("Changed index from " + oldLayerIndex + " to " + newLayerIndex);
                    idProperty.intValue = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);;
                    rawProperty.serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.EndProperty();
            }
        }
    }

}
