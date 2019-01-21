using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    [CustomPropertyDrawer(typeof(Vector3))]
    public class Vector3PropertyDrawer : PropertyDrawer {

        private const int BUTTON_WITH = 25;
        private static GUIStyle resetStyle;
        bool twoLines = false;
        bool valueChanged = false;


        //-----------------------------------------------------------------------------
        // Methods
        //-----------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            Event evt = Event.current;
            bool wideMode = EditorGUIUtility.wideMode;
            label = EditorGUI.BeginProperty(position, label, property);
            {
                Vector3 vector = property.vector3Value;
                EditorGUI.BeginChangeCheck();

                if (evt.type != EventType.Layout && evt.type != EventType.Used) {

                    if (position.width < 313) {
                        this.twoLines = true;
                    }
                    else {
                        EditorGUIUtility.wideMode = true;
                        this.twoLines = false;
                    }
                }
                Vector3 newVector = EditorGUI.Vector3Field(new Rect(position.x, position.y, position.width, position.height), label, vector);
                Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                if (property.depth > 0) {
                    labelRect = new Rect(position.x, position.y, 10, EditorGUIUtility.singleLineHeight);
                }

                if (EditorGUI.EndChangeCheck()) {

                    ExtensionTools.RegisterUndo("Changed Value", property.serializedObject.targetObjects);
                    property.vector3Value = newVector;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndProperty();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            if (this.valueChanged) {

                GUIUtility.keyboardControl = 0;
                this.valueChanged = false;
            }

            EditorGUIUtility.wideMode = wideMode;
        }

        //-----------------------------------------------------------------------------

        private void ChangePropertyValue(SerializedProperty property, Vector3 value) {


            if (property.vector3Value != value) {

                this.valueChanged = true;
                ExtensionTools.RegisterUndo("Change Value", property.serializedObject.targetObjects);
                property.vector3Value = value;
                property.serializedObject.ApplyModifiedProperties();

            }
        }

        //-----------------------------------------------------------------------------

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return this.twoLines ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight;
        }

    }
}
