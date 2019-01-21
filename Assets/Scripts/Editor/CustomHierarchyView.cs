using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    [InitializeOnLoad]
    public class CustomHierarchyView {

        //-----------------------------------------------------------------------------
        // Member
        //-----------------------------------------------------------------------------

        private static EditorWindow hierarchyWindow = null;

        //-----------------------------------------------------------------------------
        // Methods
        //-----------------------------------------------------------------------------

        static CustomHierarchyView() {

            Assembly assembly = Assembly.GetAssembly(typeof(EditorWindow));
            Type type = assembly.GetType("UnityEditor.SceneHierarchyWindow");
            EditorWindow[] editorWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < editorWindows.Length; i++) {

                if (editorWindows[i].GetType() == type) {

                    CustomHierarchyView.hierarchyWindow = editorWindows[i];
                    CustomHierarchyView.hierarchyWindow.wantsMouseMove = true;
                    break;
                }
            }

            if (CustomHierarchyView.hierarchyWindow != null) {

                CustomHierarchyView.hierarchyWindow.Repaint();
                EditorApplication.hierarchyWindowItemOnGUI += CustomHierarchyView.OnHierarchyWindowItemOnGUI;
            }


            FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);
            value += EditorGlobalKeyPress;
            info.SetValue(null, value);
        }

        //-----------------------------------------------------------------------------

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {

            if (CustomHierarchyView.hierarchyWindow != null) {
                CustomHierarchyView.hierarchyWindow.wantsMouseMove = true;
            }

            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
            if (go == null) {
                return;
            }

            Event evt = Event.current;
            Transform transform = go.transform;
            Rect r = new Rect(selectionRect.xMax - 15, selectionRect.y, 15, 15);

            GUIStyle style = new GUIStyle(GUIStyle.none);
            if (!go.activeSelf || !go.activeInHierarchy) {
                style.normal.textColor = Color.red;
                GUI.Box(r, "\u2717", style);
                //GUI.Box(r, "✗", style);
            }
            else {
                style.normal.textColor = Color.green;
                GUI.Box(r, "\u2713", style);
                //GUI.Box(r, "✓", style);
            }

            if (EditorApplication.isCompiling) {
                return;
            }

            if (r.Contains(evt.mousePosition)) {

                if (evt.type == EventType.MouseUp && evt.button == 0) {
                    go.SetActive(!go.activeSelf);
                }
            }
        }

        //-----------------------------------------------------------------------------

        private static void EditorGlobalKeyPress() {

            if (Event.current.control && !Event.current.alt && !Event.current.shift) {

                if (Event.current.type == EventType.KeyUp) {

                    if (Event.current.keyCode == KeyCode.G) {

                        if (Selection.gameObjects.Length != 0) {

                            Transform parent = new GameObject("Group_Parent").transform;
                            for (int i = 0; i < Selection.gameObjects.Length; i++) {
                                Selection.gameObjects[i].transform.SetParent(parent);
                            }
                            Selection.activeTransform = parent;
                            Event.current.Use();
                        }
                    }

                    if (Event.current.keyCode == KeyCode.H) {

                        if (Selection.gameObjects.Length != 0) {

                            for (int i = 0; i < Selection.gameObjects.Length; i++) {
                                Selection.gameObjects[i].SetActive(false);
                            }
                        }
                    }
                }
            }

            if (Event.current.control && !Event.current.alt && Event.current.shift) {

                if (Event.current.type == EventType.KeyUp) {

                    if (Event.current.keyCode == KeyCode.G) {

                        if (Selection.gameObjects.Length == 1) {

                            Transform parent = Selection.activeTransform.parent;
                            List<Transform> children = new List<Transform>();
                            foreach (Transform item in Selection.activeTransform) {
                                children.Add(item);
                            }
                            for (int i = 0; i < children.Count; i++) {
                                children[i].SetParent(parent);
                            }
                            Event.current.Use();
                        }
                    }

                    if (Event.current.keyCode == KeyCode.H) {

                        if (Selection.gameObjects.Length != 0) {

                            for (int i = 0; i < Selection.gameObjects.Length; i++) {
                                Selection.gameObjects[i].SetActive(true);
                            }
                        }
                    }
                }
            }

        }
    }
}
