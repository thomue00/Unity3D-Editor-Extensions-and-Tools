//-----------------------------------------------------------------------------

using System;
using UnityEngine;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    public partial class SpotlightWindow {

        private class Blur {

            //-----------------------------------------------------------------------------
            // Member
            //-----------------------------------------------------------------------------

            private Color tint = Color.black;
            private float tinting = 0.4f;
            private float blurSize = 4.0f;
            private int passes = 8;
            private Material blurMaterial;
            private RenderTexture destTexture;

            //-----------------------------------------------------------------------------
            // Methods
            //-----------------------------------------------------------------------------

            public Blur(int width, int height, int passes = 2) {

                this.passes = passes;
                this.blurMaterial = new Material(Shader.Find("Hidden/Blur"));
                this.blurMaterial.SetColor("_Tint", this.tint);
                this.blurMaterial.SetFloat("_Tinting", this.tinting);
                this.blurMaterial.SetFloat("_BlurSize", this.blurSize);

                this.destTexture = new RenderTexture(width, height, 0);
                this.destTexture.Create();
            }

            //-----------------------------------------------------------------------------

            public Texture BlurTexture(Texture sourceTexture) {

                // Cache original RenderTexture so we can restore when we're done.
                RenderTexture active = RenderTexture.active; 
                try {

                    // Grab 2 Screen Samples
                    RenderTexture tempA = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
                    RenderTexture tempB = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
                    // Apply Blurpasses
                    for (int i = 0; i < this.passes; i++) {

                        if (i == 0) {
                            Graphics.Blit(sourceTexture, tempA, this.blurMaterial, 0);
                        }
                        else {
                            Graphics.Blit(tempB, tempA, this.blurMaterial, 0);
                        }
                        Graphics.Blit(tempA, tempB, this.blurMaterial, 1);
                    }
                    // Blit them together
                    Graphics.Blit(tempB, this.destTexture, this.blurMaterial, 2);
                    // Release Screengrabs
                    RenderTexture.ReleaseTemporary(tempA);
                    RenderTexture.ReleaseTemporary(tempB);
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
                finally {
                    // Restore cached Rendertexture
                    RenderTexture.active = active;
                }

                return this.destTexture;
            }
        }
    }
}
