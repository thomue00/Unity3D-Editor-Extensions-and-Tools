//-----------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    public partial class SpotlightWindow {

        private class StyleTextureWrapper {

            //-----------------------------------------------------------------------------

            public enum IconType {

                Unknown = 0,
                File = 1,
                Assets = 2,
                GameObject = 3,
                Component = 4,
                Window = 5,
                Helper = 6,
                PingAsset = 7
            }

            //-----------------------------------------------------------------------------

            public StyleTextureWrapper() {

                // Create new Lookup DataSet for the Search hit Icons
                this.textureLookup = new Dictionary<IconType, TextureIconContainer>();

                // Init Styles
                if (this.SearchStyle == null) {

                    this.SearchStyle = new GUIStyle(EditorStyles.textField);

                    this.SearchStyle.margin.top = 5;
                    this.SearchStyle.margin.left = 5;
                    this.SearchStyle.margin.right = 5;
                    this.SearchStyle.margin.bottom = 10;

                    Texture2D grey = new Texture2D(1, 1);
                    grey.SetPixel(0, 0, new Color(0.7f, 0.7f, 0.7f, 0.5f));
                    grey.Apply();
                    this.SearchStyle.focused.background = grey;
                    this.SearchStyle.normal.background = grey;
                    this.SearchStyle.active.background = grey;

                    this.SearchStyle.alignment = TextAnchor.MiddleCenter;
                    this.SearchStyle.fontSize = 18;
                    this.SearchStyle.fontStyle = FontStyle.Bold;
                    this.SearchStyle.focused.textColor = Color.black;
                    this.SearchStyle.normal.textColor = Color.black;
                    this.SearchStyle.active.textColor = Color.black;
                }

                if (this.HitStyle == null) {

                    this.HitStyle = new GUIStyle("button");
                    this.HitStyle.margin.left = 0;
                    this.HitStyle.margin.right = 0;
                    Texture2D grey = new Texture2D(1, 1);
                    grey.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f, 0.3f));
                    grey.Apply();
                    this.HitStyle.focused.background = grey;
                    this.HitStyle.normal.background = grey;
                    this.HitStyle.active.background = grey;

                    this.HitStyle.alignment = TextAnchor.MiddleCenter;
                    this.HitStyle.fontSize = 12;
                    this.HitStyle.fontStyle = FontStyle.Normal;
                    this.HitStyle.wordWrap = true;
                    this.HitStyle.focused.textColor = Color.black;
                    this.HitStyle.normal.textColor = Color.black;
                    this.HitStyle.active.textColor = Color.black;
                }

                if (this.HitStyleSelected == null) {

                    this.HitStyleSelected = new GUIStyle(this.HitStyle);
                    Texture2D bgWithBorder = StyleTextureWrapper.MakeTex(16, 16, new Color(0.20f, 0.58f, 0.14f, 0.6f), new RectOffset(2, 2, 2, 2), new Color(0.9f, 0.9f, 0.9f, 0.3f));
                    this.HitStyleSelected.border = new RectOffset(3, 3, 3, 3);
                    this.HitStyleSelected.fontStyle = FontStyle.Bold;
                    this.HitStyleSelected.focused.background = bgWithBorder;
                    this.HitStyleSelected.normal.background = bgWithBorder;
                    this.HitStyleSelected.active.background = bgWithBorder;
                }

                if (this.IconStyle == null) {

                    this.IconStyle = new GUIStyle(GUIStyle.none);
                    this.IconStyle.alignment = TextAnchor.MiddleCenter;

                }
            }

            //-----------------------------------------------------------------------------

            private struct TextureIconContainer {

                //-----------------------------------------------------------------------------
                // Member
                //-----------------------------------------------------------------------------

                public Texture selected;
                public Texture unSelected;

                //-----------------------------------------------------------------------------
                // Methods
                //-----------------------------------------------------------------------------

                public TextureIconContainer(IconType type) {

                    this.selected = null;
                    this.unSelected = null;
                    switch (type) {

                        case IconType.File:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_File_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_File.psd");
                            break;

                        case IconType.Assets:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Assets_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Assets.psd");
                            break;

                        case IconType.GameObject:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_GameObject_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_GameObject.psd");
                            break;

                        case IconType.Component:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Component_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Component.psd");
                            break;

                        case IconType.Window:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Window_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Window.psd");
                            break;

                        case IconType.Helper:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Helper_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Helper.psd");
                            break;

                        case IconType.PingAsset:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_PingAsset_Selected.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_PingAsset.psd");
                            break;

                        case IconType.Unknown:
                        default:
                            this.selected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Unity.psd");
                            this.unSelected = StyleTextureWrapper.LoadCustomIcon("Spotlight_Unity.psd");
                            break;
                    }

                    // Error Material
                    if (this.selected == null) {

                        this.selected = StyleTextureWrapper.MakeTex(30, 30, Color.red, new RectOffset(0, 0, 0, 0), Color.red);
                        Debug.LogError("Can't Load Selected Icon for " + type);
                    }

                    if (this.unSelected == null) {

                        this.unSelected = StyleTextureWrapper.MakeTex(30, 30, Color.red, new RectOffset(0, 0, 0, 0), Color.red);
                        Debug.LogError("Can't Load Unselected Icon for " + type);
                    }

                }

            }

            //-----------------------------------------------------------------------------
            // Member
            //-----------------------------------------------------------------------------

            private Dictionary<IconType, TextureIconContainer> textureLookup;

            //-----------------------------------------------------------------------------
            // Properties
            //-----------------------------------------------------------------------------

            public GUIStyle SearchStyle { get; private set; }
            public GUIStyle HitStyle { get; private set; }
            public GUIStyle HitStyleSelected { get; private set; }
            public GUIStyle IconStyle { get; private set; }

            //-----------------------------------------------------------------------------
            // Methods
            //-----------------------------------------------------------------------------

            public IconType GetIconTypeFromString(string s) {

                if (s.StartsWith("File")) {
                    return IconType.File;
                }
                else if (s.StartsWith("Assets")) {
                    return IconType.Assets;
                }
                else if (s.StartsWith("GameObject")) {
                    return IconType.GameObject;
                }
                else if (s.StartsWith("Component")) {
                    return IconType.Component;
                }
                else if (s.StartsWith("Window")) {
                    return IconType.Window;
                }
                else if (s.StartsWith("Help")) {
                    return IconType.Helper;
                }
                else if (s.StartsWith("PingAsset")) {
                    return IconType.PingAsset;
                }
                return IconType.Unknown;
            }

            //-----------------------------------------------------------------------------

            public Texture GetTexture(IconType type, bool selected) {

                if (!this.textureLookup.ContainsKey(type)) {
                    this.textureLookup.Add(type, new TextureIconContainer(type));
                }
                return selected ? this.textureLookup[type].selected : this.textureLookup[type].unSelected;
            }

            //-----------------------------------------------------------------------------

            private static Texture LoadCustomIcon(string name) {

                Texture tex = EditorGUIUtility.Load("Spotlight/" + name) as Texture;
                if (tex == null) {
                    Debug.LogWarning("Couldn't load Texture: Editor Default Resources/Spotlight/" + name);
                }
                return tex;
            }

            //-----------------------------------------------------------------------------

            private static Texture2D MakeTex(int width, int height, Color32 textureColor, RectOffset border, Color32 bordercolor) {

                Color[] pix = new Color[width * height];

                for (int i = 0; i < pix.Length; i++) {

                    if (i < (border.bottom * width)) {
                        pix[i] = bordercolor;
                    }
                    else if (i >= width * (height - border.top)) {  //Border Top
                        pix[i] = bordercolor;
                    }
                    else { //Center of Texture

                        if ((i % width) < border.left) { // Border left

                            pix[i] = bordercolor;
                        }
                        else if ((i % width) >= (width - border.right)) { //Border right
                            pix[i] = bordercolor;
                        }
                        else {
                            pix[i] = textureColor; //Color texture
                        }
                    }
                }

                Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);

                result.alphaIsTransparency = true;
                result.filterMode = FilterMode.Point;
                result.SetPixels(pix);

                result.Apply();
                return result;
            }
        }
    }
}
