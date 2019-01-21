using UnityEngine;
using UnityEditor;
using System;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform))]
    public class TransformEditor : Editor {

        //-----------------------------------------------------------------------------
        // Member
        //-----------------------------------------------------------------------------

        private static TransformEditor instance;

        public static Vector3? copyPosition = Vector3.zero;
        public static Quaternion? copyRotation = Quaternion.identity;
        public static Vector3? copyScale = Vector3.zero;

        private Transform transform;
        private SerializedProperty pos;
        private SerializedProperty rot;
        private SerializedProperty scale;
        private Space space = Space.Local;
        private int selectionCount = 0;

        GUIStyle btnSmall;

        //-----------------------------------------------------------------------------
        // Properties
        //-----------------------------------------------------------------------------

        private bool IsLocal {
            get { return this.space == Space.Local; }
        }

        //-----------------------------------------------------------------------------
        // Methods
        //-----------------------------------------------------------------------------

        private void OnEnable() {

            TransformEditor.instance = this;
            this.pos = this.serializedObject.FindProperty(GlobalConfig.PROPERTY_ID_POSITION);
            this.rot = this.serializedObject.FindProperty(GlobalConfig.PROPERTY_ID_ROTATION);
            this.scale = this.serializedObject.FindProperty(GlobalConfig.PROPERTY_ID_SCALE);
            this.transform = (Transform)this.serializedObject.targetObject;
            this.selectionCount = Selection.gameObjects.Length;
        }

        //-----------------------------------------------------------------------------

        private void OnDestroy() {
            TransformEditor.instance = null;
        }

        //-----------------------------------------------------------------------------

        public override void OnInspectorGUI() {

            this.EnsureStyles();

            //this.DrawDefaultInspector();
            //EditorGUILayout.Space();
            //EditorGUILayout.Space();

            this.DrawTransformHeader();
            GUILayout.Space(1);
            this.serializedObject.Update();

            float resetLabelWith = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 15f;

            this.DrawTransformPosition();
            this.DrawTransformRotation();
            this.DrawTransformScale();

            if (this.selectionCount > 1) {
                EditorGUILayout.LabelField("Selected: " + this.selectionCount + " Objects");
            }

            EditorUtility.SetDirty(this.transform);
            this.serializedObject.ApplyModifiedProperties();
            EditorGUIUtility.labelWidth = resetLabelWith;

            EditorGUILayout.Space();

        }

        //-----------------------------------------------------------------------------

        private void EnsureStyles() {

            this.btnSmall = new GUIStyle(EditorStyles.toolbarButton);
            this.btnSmall.fixedHeight = 16;
            this.btnSmall.fixedWidth = 20;
        }

        //-----------------------------------------------------------------------------

        private void DrawTransformHeader() {

            using (new EditorGUILayout.HorizontalScope()) {

                string val = this.selectionCount > 1 ? ("ID: Multiple") : ("ID: " + this.transform.GetInstanceID());
                EditorGUILayout.LabelField(val, GUILayout.MinWidth(0));

                GUILayout.FlexibleSpace();

                this.space = (Space)this.EnumToolbar(this.space);
            }
        }

        //-----------------------------------------------------------------------------

        private void DrawTransformPosition() {

            using (new EditorGUILayout.HorizontalScope()) {

                bool reset = GUILayout.Button("R", this.btnSmall);

                GUILayoutOption option = GUILayout.MinWidth(63);
                if (this.IsLocal) {

                    EditorGUILayout.PropertyField(this.pos.FindPropertyRelative("x"), option);
                    EditorGUILayout.PropertyField(this.pos.FindPropertyRelative("y"), option);
                    EditorGUILayout.PropertyField(this.pos.FindPropertyRelative("z"), option);
                }
                else {

                    Vector3 pos = this.transform.position;
                    this.FloatField("X", ref pos.x, option);
                    this.FloatField("Y", ref pos.y, option);
                    this.FloatField("Z", ref pos.z, option);
                    this.transform.position = pos;
                }
                if (GUILayout.Button("C", this.btnSmall)) {
                    TransformEditor.copyPosition = this.IsLocal ? Selection.activeTransform.localPosition : Selection.activeTransform.position;
                }
                if (GUILayout.Button("V", this.btnSmall)) {

                    if (TransformEditor.copyPosition.HasValue) {

                        ExtensionTools.RegisterUndo("Paste Position", this.serializedObject.targetObjects);
                        if (this.IsLocal) {
                            Selection.activeTransform.localPosition = TransformEditor.copyPosition.Value;
                        }
                        else {
                            Selection.activeTransform.position = TransformEditor.copyPosition.Value;
                        }
                    }
                }

                if (reset) {

                    ExtensionTools.RegisterUndo("Reset Position", this.serializedObject.targetObjects);
                    if (this.IsLocal) {
                        this.pos.vector3Value = Vector3.zero;
                    }
                    else {
                        this.transform.position = Vector3.zero;
                    }
                    GUIUtility.keyboardControl = 0;
                }
            }
        }

        //-----------------------------------------------------------------------------

        private void DrawTransformRotation() {

            using (new EditorGUILayout.HorizontalScope()) {

                bool reset = GUILayout.Button("R", this.btnSmall);

                GUILayoutOption option = GUILayout.MinWidth(63);
                Vector3 euler = this.IsLocal ? (this.serializedObject.targetObject as Transform).localRotation.eulerAngles : (this.serializedObject.targetObject as Transform).rotation.eulerAngles;
                euler = this.WrapAngle(euler);

                Axes changed = this.CheckDifference(this.rot);
                Axes altered = Axes.None;

                if (this.FloatField("X", ref euler.x, option)) {
                    altered |= Axes.X;
                }
                if (this.FloatField("Y", ref euler.y, option)) {
                    altered |= Axes.Y;
                }
                if (this.FloatField("Z", ref euler.z, option)) {
                    altered |= Axes.Z;
                }

                if (GUILayout.Button("C", this.btnSmall)) {
                    TransformEditor.copyRotation = this.IsLocal ? Selection.activeTransform.localRotation : Selection.activeTransform.rotation;
                }
                if (GUILayout.Button("V", this.btnSmall)) {

                    if (TransformEditor.copyRotation.HasValue) {

                        ExtensionTools.RegisterUndo("Paste Rotation", this.serializedObject.targetObjects);
                        if (this.IsLocal) {
                            Selection.activeTransform.localRotation = TransformEditor.copyRotation.Value;
                        }
                        else {
                            Selection.activeTransform.rotation = TransformEditor.copyRotation.Value;
                        }
                    }
                }

                if (reset) {

                    ExtensionTools.RegisterUndo("Reset Rotation", this.serializedObject.targetObjects);
                    if (this.IsLocal) {

                        this.rot.quaternionValue = Quaternion.identity;
                        this.transform.localEulerAngles = Vector3.zero;
                    }
                    else {

                        this.transform.rotation = Quaternion.identity;
                        this.transform.eulerAngles = Vector3.zero;
                    }
                    GUIUtility.keyboardControl = 0;
                }

                if (altered != Axes.None) {

                    ExtensionTools.RegisterUndo("Changed Rotation", this.serializedObject.targetObjects);

                    foreach (UnityEngine.Object obj in this.serializedObject.targetObjects) {

                        Transform t = obj as Transform;
                        Vector3 v;
                        v = this.IsLocal ? t.localEulerAngles : t.eulerAngles;
                        if ((altered & Axes.X) != 0) {
                            v.x = euler.x;
                        }
                        if ((altered & Axes.Y) != 0) {
                            v.y = euler.y;
                        }
                        if ((altered & Axes.Z) != 0) {
                            v.z = euler.z;
                        }
                        if (this.IsLocal) {
                            t.localEulerAngles = v;
                        }
                        else {
                            t.eulerAngles = v;
                        }
                    }
                }
            }

        }

        //-----------------------------------------------------------------------------

        private void DrawTransformScale() {

            using (new EditorGUILayout.HorizontalScope()) {

                bool reset = GUILayout.Button("R", this.btnSmall);

                GUILayoutOption option = GUILayout.MinWidth(63);
                if (this.IsLocal) {

                    EditorGUILayout.PropertyField(this.scale.FindPropertyRelative("x"), option);
                    EditorGUILayout.PropertyField(this.scale.FindPropertyRelative("y"), option);
                    EditorGUILayout.PropertyField(this.scale.FindPropertyRelative("z"), option);
                }
                else {

                    Vector3 scale = this.transform.lossyScale;
                    this.FloatField("X", ref scale.x, option);
                    this.FloatField("Y", ref scale.y, option);
                    this.FloatField("Z", ref scale.z, option);
                }

                if (GUILayout.Button("C", this.btnSmall) && this.IsLocal) {
                    TransformEditor.copyScale = Selection.activeTransform.localScale;
                }
                if (GUILayout.Button("V", this.btnSmall) && this.IsLocal) {

                    if (TransformEditor.copyScale.HasValue) {

                        ExtensionTools.RegisterUndo("Paste Scale", this.serializedObject.targetObjects);
                        Selection.activeTransform.localScale = TransformEditor.copyScale.Value;
                    }
                }

                if (reset) {

                    if (this.IsLocal) {

                        ExtensionTools.RegisterUndo("Reset Scale", this.serializedObject.targetObjects);
                        this.scale.vector3Value = Vector3.one;
                    }
                    GUIUtility.keyboardControl = 0;
                }
            }

        }

        //-----------------------------------------------------------------------------
        // Helper - Methods
        //-----------------------------------------------------------------------------

        private bool FloatField(string name, ref float val, params GUILayoutOption[] options) {

            float newVal = val;
            EditorGUI.BeginChangeCheck();
            newVal = EditorGUILayout.FloatField(name, newVal, options);
            if (EditorGUI.EndChangeCheck() && this.Differs(val, newVal)) {

                val = newVal;
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------

        private Enum EnumToolbar(Enum selected) {

            string[] items = Enum.GetNames(selected.GetType());
            int count = items.Length;
            Array vals = Enum.GetValues(selected.GetType());
            GUIContent[] contents = new GUIContent[count];
            for (int i = 0; i < count; i++) {
                contents[i] = new GUIContent(items[i], "");
            }
            int selectedIdx = 0;
            while (selectedIdx < count) {

                if (selected.ToString() == vals.GetValue(selectedIdx).ToString()) {
                    break;
                }
                selectedIdx++;
            }
            selectedIdx = GUILayout.Toolbar(selectedIdx, contents);
            return (Enum)vals.GetValue(selectedIdx);

        }

        //-----------------------------------------------------------------------------

        private Axes CheckDifference(SerializedProperty rotProperty) {

            Axes axes = Axes.None;
            if (rotProperty.hasMultipleDifferentValues) {

                Vector3 original = rotProperty.quaternionValue.eulerAngles;
                foreach (UnityEngine.Object obj in this.serializedObject.targetObjects) {

                    axes |= this.CheckDifference(obj as Transform, original);
                    if (axes == Axes.All) {
                        break;
                    }
                }
            }
            return axes;
        }

        //-----------------------------------------------------------------------------

        private Axes CheckDifference(Transform t, Vector3 original) {

            Vector3 next = this.IsLocal ? t.localEulerAngles : t.eulerAngles;

            Axes axes = Axes.None;
            if (this.Differs(next.x, original.x)) {
                axes |= Axes.X;
            }

            if (this.Differs(next.y, original.y)) {
                axes |= Axes.Y;
            }

            if (this.Differs(next.z, original.z)) {
                axes |= Axes.Z;
            }

            return axes;
        }

        //-----------------------------------------------------------------------------

        private bool Differs(float a, float b) {
            return System.Math.Abs(a - b) > 0.0001f;
        }

        //-----------------------------------------------------------------------------

        private float WrapAngle(float angle) {

            while (angle > 180f) {
                angle -= 360f;
            }
            while (angle < -180f) {
                angle += 360f;
            }
            return angle;
        }

        //-----------------------------------------------------------------------------

        private Vector3 WrapAngle(Vector3 angle) {

            angle.x = this.WrapAngle(angle.x);
            angle.y = this.WrapAngle(angle.y);
            angle.z = this.WrapAngle(angle.z);
            return angle;
        }
    }

    //-----------------------------------------------------------------------------
    // ENUMS
    //-----------------------------------------------------------------------------

    internal enum Space {
        Local = 0,
        Global = 1
    }

    //-----------------------------------------------------------------------------

    internal enum Axes : int {

        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        All = 7,
    }
}
