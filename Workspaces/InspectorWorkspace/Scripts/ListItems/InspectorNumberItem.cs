﻿using System;
using UnityEditor.Experimental.EditorVR.Data;
using UnityEditor.Experimental.EditorVR.Handles;
using UnityEditor.Experimental.EditorVR.UI;
using UnityEngine;

#if !UNITY_EDITOR
public enum SerializedPropertyType
{
    Generic = -1,
    Integer = 0,
    Boolean = 1,
    Float = 2,
    String = 3,
    Color = 4,
    ObjectReference = 5,
    LayerMask = 6,
    Enum = 7,
    Vector2 = 8,
    Vector3 = 9,
    Vector4 = 10, // 0x0000000A
    Rect = 11, // 0x0000000B
    ArraySize = 12, // 0x0000000C
    Character = 13, // 0x0000000D
    AnimationCurve = 14, // 0x0000000E
    Bounds = 15, // 0x0000000F
    Gradient = 16, // 0x00000010
    Quaternion = 17, // 0x00000011
    ExposedReference = 18, // 0x00000012
    FixedBufferSize = 19, // 0x00000013
    Vector2Int = 20, // 0x00000014
    Vector3Int = 21, // 0x00000015
    RectInt = 22, // 0x00000016
    BoundsInt = 23, // 0x00000017
}
#endif

namespace UnityEditor.Experimental.EditorVR.Workspaces
{
    sealed class InspectorNumberItem : InspectorPropertyItem
    {
        [SerializeField]
        NumericInputField m_InputField;

        [SerializeField]
        WorkspaceButton[] m_IncrementDecrementButtons;

        public SerializedPropertyType propertyType { get; private set; }
        public event Action<PropertyData> arraySizeChanged;

        public override void Setup(InspectorData data)
        {
            base.Setup(data);

#if UNITY_EDITOR
            propertyType = m_SerializedProperty.propertyType;
#endif

            OnObjectModified();
        }

        public override void OnObjectModified()
        {
            base.OnObjectModified();
            UpdateInputField();
        }

        void UpdateInputField()
        {
            var val = string.Empty;

#if UNITY_EDITOR
            switch (m_SerializedProperty.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Integer:
                    val = m_SerializedProperty.intValue.ToString();
                    m_InputField.numberType = NumericInputField.NumberType.Int;
                    break;
                case SerializedPropertyType.Float:
                    val = m_SerializedProperty.floatValue.ToString();
                    m_InputField.numberType = NumericInputField.NumberType.Float;
                    break;
            }
#endif

            m_InputField.text = val;
            m_InputField.ForceUpdateLabel();
        }

        public void SetValue(string input)
        {
#if UNITY_EDITOR
            // Do not increment undo group because NumericInputField does it for us
            if (SetValueIfPossible(input))
                data.serializedObject.ApplyModifiedProperties();
#endif
        }

        bool SetValueIfPossible(string input)
        {
#if UNITY_EDITOR
            switch (m_SerializedProperty.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                    int size;
                    if (int.TryParse(input, out size) && m_SerializedProperty.intValue != size)
                    {
                        if (size < 0)
                            return false;

                        m_SerializedProperty.arraySize = size;

                        m_InputField.text = size.ToString();
                        m_InputField.ForceUpdateLabel();

                        if (arraySizeChanged != null)
                            arraySizeChanged((PropertyData)data);

                        return true;
                    }
                    break;
                case SerializedPropertyType.Integer:
                    int i;
                    if (int.TryParse(input, out i) && m_SerializedProperty.intValue != i)
                    {
                        m_SerializedProperty.intValue = i;

                        m_InputField.text = i.ToString();
                        m_InputField.ForceUpdateLabel();

                        return true;
                    }
                    break;
                case SerializedPropertyType.Float:
                    float f;
                    if (float.TryParse(input, out f) && !Mathf.Approximately(m_SerializedProperty.floatValue, f))
                    {
                        m_SerializedProperty.floatValue = f;

                        m_InputField.text = f.ToString();
                        m_InputField.ForceUpdateLabel();

                        return true;
                    }
                    break;
            }
#endif

            return false;
        }

        protected override object GetDropObjectForFieldBlock(Transform fieldBlock)
        {
            return m_InputField.text;
        }

        protected override bool CanDropForFieldBlock(Transform fieldBlock, object dropObject)
        {
            return dropObject is string;
        }

        protected override void ReceiveDropForFieldBlock(Transform fieldBlock, object dropObject)
        {
            if (SetValueIfPossible(dropObject.ToString()))
                FinalizeModifications();
        }

        protected override void OnHorizontalDragStart(Transform rayOrigin, Transform fieldBlock)
        {
            base.OnHorizontalDragStart(rayOrigin, fieldBlock);
            foreach (var button in m_IncrementDecrementButtons)
                button.alternateIconVisible = true;
        }

        protected override void OnDragEnded(BaseHandle handle, HandleEventData eventData)
        {
            base.OnDragEnded(handle, eventData);

#if UNITY_EDITOR
            // Update field value in case drag value was invalid (i.e. array size < 0)
            if (m_DraggedField)
            {
                switch (m_SerializedProperty.propertyType)
                {
                    case SerializedPropertyType.ArraySize:
                    case SerializedPropertyType.Integer:
                        m_DraggedField.text = m_SerializedProperty.intValue.ToString();
                        m_DraggedField.ForceUpdateLabel();
                        break;
                    case SerializedPropertyType.Float:
                        m_DraggedField.text = m_SerializedProperty.floatValue.ToString();
                        m_DraggedField.ForceUpdateLabel();
                        break;
                }
            }
#endif

            foreach (var button in m_IncrementDecrementButtons)
                button.alternateIconVisible = false;
        }

        public void Increment()
        {
#if UNITY_EDITOR
            switch (m_SerializedProperty.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Integer:
                    if (SetValueIfPossible((m_SerializedProperty.intValue + 1).ToString()))
                        FinalizeModifications();
                    break;
                case SerializedPropertyType.Float:
                    if (SetValueIfPossible((m_SerializedProperty.floatValue + 1).ToString()))
                        FinalizeModifications();
                    break;
            }
#endif
        }

        public void Decrement()
        {
#if UNITY_EDITOR
            switch (m_SerializedProperty.propertyType)
            {
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Integer:
                    if (SetValueIfPossible((m_SerializedProperty.intValue - 1).ToString()))
                        FinalizeModifications();
                    break;
                case SerializedPropertyType.Float:
                    if (SetValueIfPossible((m_SerializedProperty.floatValue - 1).ToString()))
                        FinalizeModifications();
                    break;
            }
#endif
        }
    }
}
