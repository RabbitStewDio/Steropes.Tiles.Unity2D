using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityToolbag
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            if (property.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.HelpBox(position, string.Format("{0} is not an integer but has [SortingLayer].", property.name), MessageType.Error);
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);

                // Look up the layer name using the current layer ID
                string oldName = SortingLayer.IDToName(property.intValue);

                // Use the name to look up our array index into the names list
                int oldLayerIndex = Array.IndexOf(sortingLayerNames, oldName);

                // Show the popup for the names
                int newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

                // If the index changes, look up the ID for the new index to store as the new ID
                if (newLayerIndex != oldLayerIndex) {
                    property.intValue = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);
                }

                EditorGUI.EndProperty();
            }
        }
    }
}