#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[Serializable]
public class UDictionary
{
    public class SplitAttribute : PropertyAttribute
    {
        public float Key { get; protected set; }
        public float Value { get; protected set; }

        public SplitAttribute(float key, float value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    [CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
    public class Drawer : PropertyDrawer
    {
        SerializedProperty property;

        public bool IsExpanded
        {
            get => property.isExpanded;
            set => property.isExpanded = value;
        }

        SerializedProperty keys;
        SerializedProperty values;

        public bool IsAligned => keys.arraySize == values.arraySize;

        ReorderableList list;
        GUIContent label;
        SplitAttribute split;

        public float KeySplit => split == null ? 30f : split.Key;
        public float ValueSplit => split == null ? 70f : split.Value;

        public static float SingleLineHeight => EditorGUIUtility.singleLineHeight;

        public const float ElementHeightPadding = 6f;
        public const float ElementSpacing = 10f;
        public const float ElementFoldoutPadding = 20f;

        public const float TopPadding = 5f;
        public const float BottomPadding = 5f;

        void Init(SerializedProperty value)
        {
            if (SerializedProperty.EqualContents(value, property)) return;

            property = value;

            keys = property.FindPropertyRelative(nameof(keys));
            values = property.FindPropertyRelative(nameof(values));

            split = attribute as SplitAttribute;

            list = new ReorderableList(property.serializedObject, keys, true, true, true, true);

            list.drawHeaderCallback = DrawHeader;

            list.onAddCallback = Add;
            list.onRemoveCallback = Remove;

            list.elementHeightCallback = GetElementHeight;

            list.drawElementCallback = DrawElement;

            list.onReorderCallbackWithDetails += Reorder;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            var height = TopPadding + BottomPadding;

            if (IsAligned)
                height += IsExpanded ? list.GetHeight() : list.headerHeight;
            else
                height += SingleLineHeight;

            return height;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            label.text = $" {label.text}";

            this.label = label;

            Init(property);

            rect = EditorGUI.IndentedRect(rect);

            rect.y += TopPadding;
            rect.height -= TopPadding + BottomPadding;

            if (IsAligned == false)
            {
                DrawAlignmentWarning(ref rect);
                return;
            }

            if (IsExpanded)
                DrawList(ref rect);
            else
                DrawCompleteHeader(ref rect);
        }

        void DrawList(ref Rect rect)
        {
            EditorGUIUtility.labelWidth = 80f;
            EditorGUIUtility.fieldWidth = 80f;

            list.DoList(rect);
        }

        void DrawAlignmentWarning(ref Rect rect)
        {
            var width = 80f;
            var spacing = 5f;

            rect.width -= width;

            EditorGUI.HelpBox(rect, "  Misalignment Detected", MessageType.Error);

            rect.x += rect.width + spacing;
            rect.width = width - spacing;

            if (GUI.Button(rect, "Fix"))
            {
                if (keys.arraySize > values.arraySize)
                {
                    var difference = keys.arraySize - values.arraySize;

                    for (int i = 0; i < difference; i++)
                        keys.DeleteArrayElementAtIndex(keys.arraySize - 1);
                }
                else if (keys.arraySize < values.arraySize)
                {
                    var difference = values.arraySize - keys.arraySize;

                    for (int i = 0; i < difference; i++)
                        values.DeleteArrayElementAtIndex(values.arraySize - 1);
                }
            }
        }

        #region Draw Header
        void DrawHeader(Rect rect)
        {
            rect.x += 10f;

            IsExpanded = EditorGUI.Foldout(rect, IsExpanded, label, true);
        }

        void DrawCompleteHeader(ref Rect rect)
        {
            ReorderableList.defaultBehaviours.DrawHeaderBackground(rect);

            rect.x += 6;
            rect.y += 0;

            DrawHeader(rect);
        }
        #endregion

        float GetElementHeight(int index)
        {
            SerializedProperty key = keys.GetArrayElementAtIndex(index);
            SerializedProperty value = values.GetArrayElementAtIndex(index);

            var kHeight = GetChildernSingleHeight(key);
            var vHeight = GetChildernSingleHeight(value);

            var max = Mathf.Max(kHeight, vHeight);

            if (max < SingleLineHeight) max = SingleLineHeight;

            return max + ElementHeightPadding;
        }

        #region Draw Element
        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height -= ElementHeightPadding;
            rect.y += ElementHeightPadding / 2;

            var areas = Split(rect, KeySplit, ValueSplit);

            DrawKey(areas[0], index);
            DrawValue(areas[1], index);
        }

        void DrawKey(Rect rect, int index)
        {
            var property = keys.GetArrayElementAtIndex(index);

            rect.x += ElementSpacing / 2f;
            rect.width -= ElementSpacing;

            DrawField(rect, property);
        }

        void DrawValue(Rect rect, int index)
        {
            var property = values.GetArrayElementAtIndex(index);

            rect.x += ElementSpacing / 2f;
            rect.width -= ElementSpacing;

            DrawField(rect, property);
        }

        void DrawField(Rect rect, SerializedProperty property)
        {
            rect.height = SingleLineHeight;

            if (IsInline(property))
            {
                EditorGUI.PropertyField(rect, property, GUIContent.none);
            }
            else
            {
                rect.x += ElementSpacing / 2f;
                rect.width -= ElementSpacing;

                foreach (var child in IterateChildern(property))
                {
                    EditorGUI.PropertyField(rect, child, false);

                    rect.y += SingleLineHeight + +2f;
                }
            }
        }
        #endregion

        void Reorder(ReorderableList list, int oldIndex, int newIndex)
        {
            values.MoveArrayElement(oldIndex, newIndex);
        }

        void Add(ReorderableList list)
        {
            values.InsertArrayElementAtIndex(values.arraySize);

            ReorderableList.defaultBehaviours.DoAddButton(list);
        }

        void Remove(ReorderableList list)
        {
            values.DeleteArrayElementAtIndex(list.index);

            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        //Static Utility
        static Rect[] Split(Rect source, params float[] cuts)
        {
            var rects = new Rect[cuts.Length];

            var x = 0f;

            for (int i = 0; i < cuts.Length; i++)
            {
                rects[i] = new Rect(source);

                rects[i].x += x;
                rects[i].width *= cuts[i] / 100;

                x += rects[i].width;
            }

            return rects;
        }

        static bool IsInline(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return property.hasVisibleChildren == false;
            }

            return true;
        }

        static IEnumerable<SerializedProperty> IterateChildern(SerializedProperty property)
        {
            var path = property.propertyPath;

            property.Next(true);

            while (true)
            {
                yield return property;

                if (property.NextVisible(false) == false) break;
                if (property.propertyPath.StartsWith(path) == false) break;
            }
        }

        float GetChildernSingleHeight(SerializedProperty property)
        {
            if (IsInline(property)) return SingleLineHeight;

            var height = 0f;

            foreach (var child in IterateChildern(property))
                height += SingleLineHeight + 2f;

            return height;
        }
    }
}
#endif
