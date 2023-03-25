using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(AspectRatioFitter), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the AspectRatioFitter component.
    ///   Extend this class to write a custom editor for a component derived from AspectRatioFitter.
    /// </summary>
    public class AspectRatioFitterEditor : SelfControllerEditor
    {
        SerializedProperty m_AspectMode;
        SerializedProperty m_AspectRatio;

        protected virtual void OnEnable()
        {
            m_AspectMode = serializedObject.FindProperty("m_AspectMode");
            m_AspectRatio = serializedObject.FindProperty("m_AspectRatio");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_AspectMode);
            EditorGUILayout.PropertyField(m_AspectRatio);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
