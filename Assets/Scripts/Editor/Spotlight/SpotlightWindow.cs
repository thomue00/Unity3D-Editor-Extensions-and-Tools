//-----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    public partial class SpotlightWindow : EditorWindow {

        [MenuItem("Window/Spotlight %,")]
        public static void OpenSpotlight() {

            SpotlightWindow.FillActionData(false);
            SpotlightWindow.win = EditorWindow.CreateInstance<SpotlightWindow>();

            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
            if (h > w) {
                // OnEnd Display roation
                w = Screen.currentResolution.height;
                h = Screen.currentResolution.width;
            }

            SpotlightWindow.win.position = new Rect(w / 2 - WIDTH_WINDOW * 0.5f, h / 2 - HEIGHT_WINDOW * 0.5f, WIDTH_WINDOW, HEIGHT_WINDOW);
            SpotlightWindow.background = GrabScreen(win.position);

            SpotlightWindow.Blur b = new SpotlightWindow.Blur(WIDTH_WINDOW, HEIGHT_WINDOW, 16);
            SpotlightWindow.background = b.BlurTexture(SpotlightWindow.background);

            SpotlightWindow.win.ShowPopup();
            SpotlightWindow.win.Focus();
            SpotlightWindow.win.Init();
        }

        //-----------------------------------------------------------------------------

        [MenuItem("Window/Spotlight %.")]
        public static void OpenSpotlightAll() {

            SpotlightWindow.FillActionData(true);
            SpotlightWindow.win = EditorWindow.CreateInstance<SpotlightWindow>();

            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
            if (h > w) {
                // OnEnd Display roation
                w = Screen.currentResolution.height;
                h = Screen.currentResolution.width;
            }

            SpotlightWindow.win.position = new Rect(w / 2 - WIDTH_WINDOW * 0.5f, h / 2 - HEIGHT_WINDOW * 0.5f, WIDTH_WINDOW, HEIGHT_WINDOW);
            SpotlightWindow.background = GrabScreen(win.position);

            SpotlightWindow.Blur b = new SpotlightWindow.Blur(WIDTH_WINDOW, HEIGHT_WINDOW, 16);
            SpotlightWindow.background = b.BlurTexture(SpotlightWindow.background);

            SpotlightWindow.win.ShowPopup();
            SpotlightWindow.win.Focus();
            SpotlightWindow.win.Init();
        }

        //-----------------------------------------------------------------------------
        // Member
        //-----------------------------------------------------------------------------
        /// <summary>Window Reference</summary>
        private static SpotlightWindow win;
        /// <summary>Current Screen Background when Spotlight is opened, this will be blured</summary>
        private static Texture background;
        /// <summary>All crawled Actions that can be searched and be executed</summary>
        private static Dictionary<string, Action> actionData = new Dictionary<string, Action>();

        /// <summary>Default Width of the Window</summary>
        private const int WIDTH_WINDOW = 500;
        /// <summary>Default Height of the Window</summary>
        private const int HEIGHT_WINDOW = 300;
        /// <summary>Singleline Height for each search hit</summary>
        private const int BASELAYOUT_HEIGHT = 30;
        /// <summary>Search hit Icon width</summary>
        private const int INFOPANEL_WIDTH = 50;
        /// <summary>Search hit description width</summary>
        private const int HITPANEL_WIDTH = 440;

        /// <summary>Input Handler</summary>
        private InputEventWrapper inputWrapper;
        /// <summary>Stylewrapper for Icons</summary>
        private StyleTextureWrapper styles;
        /// <summary>Searchpattern typed by user</summary>
        private string searchPattern = "";
        /// <summary>Found hit descs from ActionData Lookup</summary>
        private List<string> hits = new List<string>();
        /// <summary>Found hit actions from ActionData Lookup</summary>
        private List<Action> hitsAction = new List<Action>();
        /// <summary>Found hit icons from ActionData Lookup</summary>
        private List<StyleTextureWrapper.IconType> hitsIcon = new List<StyleTextureWrapper.IconType>();
        /// <summary>Currently selected hit result index</summary>
        private int currentSelection = 0;

        //-----------------------------------------------------------------------------
        // Methods
        //-----------------------------------------------------------------------------

        public void Init() {

            this.inputWrapper = new InputEventWrapper();
            this.styles = new StyleTextureWrapper();
        }

        //-----------------------------------------------------------------------------

        private void OnGUI() {

            // Draw blurred Background
            GUI.DrawTexture(new Rect(0, 0, SpotlightWindow.WIDTH_WINDOW, SpotlightWindow.HEIGHT_WINDOW), SpotlightWindow.background);

            // Handle Spotlight Closing with Esc-Key
            Event cur = Event.current;
            if (cur != null && cur.isKey && cur.keyCode == KeyCode.Escape) {

                this.CloseSpotlight();
                return;
            }

            // Get all relevant Inputs
            this.inputWrapper.GatherInputs();

            // Draw Canvas SearchContent
            this.DrawContent();

            // Handle Input Data
            this.ProcessInputs();
        }

        //-----------------------------------------------------------------------------

        private void OnLostFocus() {

            /* 
             * Right now this is not working, dont ask me why... Unity 2017.4.14f1 was working fine. Since 2018 this results in a corrupted hostview: 
             * "The object of type 'HostView' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object."
             * No clue why this is happening, there are some official bugreports about this which are marked as "not reproducable". They are all 2017.2 and 2017.3. 
             * @TODO create a small sample project to narrow this problem down
             * 
             * At the moment you have to press ESC in order to close the window, or use 2017.4.14f1... Damn this sucks
            */
            //this.CloseSpotlight();
        }

        //-----------------------------------------------------------------------------

        private void DrawContent() {

            // Searchbar
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("SearchPattern");
            this.searchPattern = EditorGUILayout.TextField(this.searchPattern, this.styles.SearchStyle, GUILayout.Height(BASELAYOUT_HEIGHT));
            EditorGUI.FocusTextInControl("SearchPattern");

            if (EditorGUI.EndChangeCheck()) {


                this.currentSelection = 0;

                // Check searchpattern against all Actions
                this.hits.Clear();
                this.hitsAction.Clear();
                this.hitsIcon.Clear();

                if (!string.IsNullOrEmpty(this.searchPattern)) {

                    string s2 = this.searchPattern.Replace(" ", ""); // fast than regex in this case
                    s2 = s2.Replace("/", "");
                    int counter = 0;
                    foreach (var item in SpotlightWindow.actionData) {

                        string s1 = item.Key.Replace(" ", ""); // fast than regex in this case
                        s1 = s1.Replace("/", "");
                        int index = s1.IndexOf(s2, StringComparison.InvariantCultureIgnoreCase);

                        if (index != -1) {

                            this.hits.Add(item.Key);
                            this.hitsAction.Add(item.Value);
                            this.hitsIcon.Add(this.styles.GetIconTypeFromString(item.Key));
                            counter++;
                        }
                        if (counter >= 20) {
                            break;
                        }
                    }
                }
            }

            // Draw Items - Only Draw 7 Items without any scrollbar (this is to expensive) and setoff the start- and endpoint of the forLoop
            using (new EditorGUILayout.VerticalScope(GUIStyle.none)) {

                // Offset Loop
                int maxItems = 6; // This is corrent for 7 items. Dont worry we all make the mistake to count from 1 (damn it)
                int start = this.currentSelection;
                int end = this.currentSelection + maxItems;

                if (end >= this.hits.Count) {

                    end = this.hits.Count - 1;
                    start = end - maxItems;
                    if (start < 0) {
                        start = 0;
                    }
                }

                // Start offset Loop
                for (int i = start; i <= end; i++) {

                    // Clamp max length of hitDescription (if length > 45) and preadd "..."
                    string desc = this.hits[i];
                    if (desc.Length > 45) {
                        desc = "..." + desc.Substring(Mathf.Max(0, desc.Length - 45));
                    }

                    // Drawing One HitResult Line
                    using (new EditorGUILayout.HorizontalScope(GUIStyle.none)) {

                        // Icon on the left side
                        using (new EditorGUILayout.VerticalScope(GUIStyle.none, GUILayout.Width(INFOPANEL_WIDTH))) {

                            GUILayout.Box(this.styles.GetTexture(this.hitsIcon[i], i == this.currentSelection), this.styles.IconStyle, GUILayout.Height(BASELAYOUT_HEIGHT));
                        }

                        // HitResult Description on the right side
                        using (new EditorGUILayout.VerticalScope(GUIStyle.none, GUILayout.Width(HITPANEL_WIDTH))) {

                            // Use a Button so we can also click the results with the mouse
                            if (GUILayout.Button(desc, i == this.currentSelection ? this.styles.HitStyleSelected : this.styles.HitStyle, GUILayout.Height(30))) {
                                this.ExecuteItemAtIndex(i);
                            }
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------

        private void ExecuteItemAtIndex(int i) {

            try {

                if (this.hitsIcon[i] == StyleTextureWrapper.IconType.Component) {

                    if (Selection.activeGameObject == null) {
                        Debug.LogWarning("Can't execute \"" + this.hits[i] + "\" with no Gameobject selected");
                    }
                }

                SpotlightWindow.actionData[this.hits[i]]();
                //this.CloseSpotlight();
            }
            catch (Exception) {
                Debug.LogWarning("Error while executing item at index: " + i + " (Search: " + this.searchPattern + ") -> Ask T3 for Help with this LogMessage");
            }
        }

        //-----------------------------------------------------------------------------
        // Inputs
        //-----------------------------------------------------------------------------

        private void ProcessInputs() {

            if (this.inputWrapper.DownArrow) {

                this.currentSelection++;
                this.Repaint();
            }
            else if (this.inputWrapper.UpArrow) {

                this.currentSelection--;
                this.Repaint();
            }
            else if (this.inputWrapper.ScrollPositive) {

                this.currentSelection++;
                this.Repaint();
            }
            else if (this.inputWrapper.ScrollNegative) {

                this.currentSelection--;
                this.Repaint();
            }

            if (this.hits.Count <= 0) {
                this.currentSelection = 0;
            }
            else if (this.currentSelection >= this.hits.Count) {
                this.currentSelection = 0;
            }
            else if (this.currentSelection < 0) {
                this.currentSelection = this.hits.Count - 1;
            }

            if (this.inputWrapper.Confirm) {

                if (this.hits.Count > 0) {
                    this.ExecuteItemAtIndex(this.currentSelection);
                }
            }

            this.inputWrapper.ClearInputs();
        }

        //-----------------------------------------------------------------------------
        // Static Methods
        //-----------------------------------------------------------------------------

        private void CloseSpotlight() {

            if (win != null) {
                this.Close();
            }
        }

        //-----------------------------------------------------------------------------

        private static void FillActionData(bool includeAssetdatabase = false) {

            if (SpotlightWindow.actionData.Count == 0) {

                SpotlightWindow.actionData.Clear();
                SpotlightWindow.AddDefaultItems();
                SpotlightWindow.GetMenuItems();
                SpotlightWindow.GetComponentsItems();
                if (includeAssetdatabase) {
                    SpotlightWindow.GetAssetItems(); // this is fast but the search ends up beeing slow as hell
                }
            }
        }

        //-----------------------------------------------------------------------------

        private static void AddDefaultItems() {

            List<string> items = new List<string>();

            items.Add("Edit/Preferences...");
            items.Add("Edit/Modules...");
            items.Add("Edit/Project Settings/Input");
            items.Add("Edit/Project Settings/Tags and Layers");
            items.Add("Edit/Project Settings/Audio");
            items.Add("Edit/Project Settings/Time");
            items.Add("Edit/Project Settings/Player");
            items.Add("Edit/Project Settings/Physics");
            items.Add("Edit/Project Settings/Physics 2D");
            items.Add("Edit/Project Settings/Quality");
            items.Add("Edit/Project Settings/Graphics");
            items.Add("Edit/Project Settings/Network");
            items.Add("Edit/Project Settings/Editor");
            items.Add("Edit/Project Settings/Script Execution Order");


            items.Add("File/New Scene");
            items.Add("File/Open Scene");
            items.Add("File/Save Project");
            items.Add("File/Build Settings...");
            items.Add("File/Build % Run");
            items.Add("File/Exit");



            items.Add("GameObject/Create Empty");
            items.Add("GameObject/Create Empty Child");
            items.Add("GameObject/3D Object/Cube");
            items.Add("GameObject/3D Object/Sphere");
            items.Add("GameObject/3D Object/Capsule");
            items.Add("GameObject/3D Object/Cylinder");
            items.Add("GameObject/3D Object/Plane");
            items.Add("GameObject/3D Object/Quad");
            items.Add("GameObject/2D Object/Sprite");
            items.Add("GameObject/Light/Directional Light");
            items.Add("GameObject/Light/Point Light");
            items.Add("GameObject/Light/Spotlight");
            items.Add("GameObject/Light/Area Light");
            items.Add("GameObject/Light/Reflection Probe");
            items.Add("GameObject/Light/Light Probe Group");
            items.Add("GameObject/Audio/Audio Source");
            items.Add("GameObject/Audio/Audio Reverb Zone");
            items.Add("GameObject/Video/Video Player");
            items.Add("GameObject/Effects/Particle System");
            items.Add("GameObject/Effects/Trail");
            items.Add("GameObject/Effects/Line");
            items.Add("GameObject/Camera");

            items.Add("GameObject/Set as first sibling %=");
            items.Add("GameObject/Set as first sibling %");
            items.Add("GameObject/Set as last sibling");
            items.Add("GameObject/Set as last sibling %-");
            items.Add("GameObject/Move To View %&f");
            items.Add("GameObject/Move To View %&f");
            items.Add("GameObject/Align With View %#f");
            items.Add("GameObject/Align With View %#f");
            items.Add("GameObject/Align View to");
            items.Add("GameObject/Align View to Selected");
            items.Add("GameObject/Toggle Active State");
            items.Add("GameObject/Toggle Active State &#a");

            foreach (var item in items) {

                if (!SpotlightWindow.actionData.ContainsKey(item)) {

                    SpotlightWindow.actionData.Add(item,
                        () => {
                            try {
                                EditorApplication.ExecuteMenuItem(item);
                            }
                            catch (Exception e) {
                                Debug.LogError(e.Message);
                            }
                        }
                        );
                }
            }
        }

        //-----------------------------------------------------------------------------
        public static Type[] GetAllTypes() {

            List<Type> res = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                res.AddRange(assembly.GetTypes());
            }
            return res.ToArray();
        }

        //-----------------------------------------------------------------------------

        private static void GetComponentsItems() {

            Type[] types = GetAllTypes();

            for (int i = 0; i < types.Length; i++) {

                if (types[i].IsInterface) {
                    continue;
                }
                if (types[i].IsAbstract) {
                    continue;
                }

                if (!types[i].IsSubclassOf(typeof(Component))) {
                    continue;
                }

                string desc = "Component/" + types[i].FullName;

                if (!SpotlightWindow.actionData.ContainsKey(desc)) {

                    string tt = types[i].FullName + ", " + Assembly.GetAssembly(types[i]);
                    SpotlightWindow.actionData.Add(desc, () => {
                        ExecuteComponentAdd(tt);
                    });
                }
            }
            //}
        }

        //-----------------------------------------------------------------------------

        private static void GetAssetItems() {

            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allAssets.Length; i++) {

                //string desc = allAssets[i].Split('/').Last();
                string desc = "PingAsset/" + allAssets[i];
                string path = allAssets[i];
                if (!SpotlightWindow.actionData.ContainsKey(desc)) {
                    SpotlightWindow.actionData.Add(desc, () => {
                        SelectAndPingAsset(path);
                    });
                }

            }
        }

        //-----------------------------------------------------------------------------

        private static void SelectAndPingAsset(string path) {

            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        //-----------------------------------------------------------------------------

        private static void ExecuteComponentAdd(string type) {


            Type t = Type.GetType(type);
            if (t != null) {

                if (Selection.gameObjects.Length != 0) {
                    for (int i = 0; i < Selection.gameObjects.Length; i++) {
                        Selection.gameObjects[i].AddComponent(t);
                    }
                }
            }
            else {
                Debug.LogError(type);
            }

        }

        //-----------------------------------------------------------------------------

        private static void GetMenuItems() {

            List<Type> classes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(t =>
                    t.Assembly.FullName.Contains("Assembly-CSharp-Editor")
                    ).ToList();
            for (int i = 0; i < classes.Count; i++) {

                List<MethodInfo> methods = classes[i].GetMethods(BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .Where(m => SpotlightWindow.GetCustomAttribute<MenuItem>(m) != null).ToList();

                foreach (var method in methods) {

                    var attr = SpotlightWindow.GetCustomAttribute<MenuItem>(method);
                    if (!attr.menuItem.StartsWith("CONTEXT") && !SpotlightWindow.actionData.ContainsKey(attr.menuItem)) {
                        SpotlightWindow.actionData.Add(attr.menuItem, () => EditorApplication.ExecuteMenuItem(attr.menuItem));
                    }
                }
            }
        }


        //-----------------------------------------------------------------------------

        private static Texture GrabScreen(Rect rect) {

            Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(rect.position, (int)rect.width, (int)rect.height);

            var tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        //-----------------------------------------------------------------------------

        public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute {
            return GetCustomAttribute<T>(member, true);
        }

        //-----------------------------------------------------------------------------

        public static T GetCustomAttribute<T>(MemberInfo member, bool inherit) where T : Attribute {
            return (T)((IEnumerable<object>)member.GetCustomAttributes(typeof(T), inherit)).FirstOrDefault<object>();
        }
    }
}
