using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
namespace EditorTools.Extensions {

    public class ExtensionTools : Editor {

        static public void RegisterUndo(string name, params Object[] objects) {

            if (objects != null && objects.Length > 0) {

                Undo.RecordObjects(objects, name);
                foreach (Object obj in objects) {

                    if (obj == null) {
                        continue;
                    }
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}