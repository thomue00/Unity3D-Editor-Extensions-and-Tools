//-----------------------------------------------------------------------------

using UnityEngine;

//-----------------------------------------------------------------------------

namespace EditorTools.Extensions {

    public partial class SpotlightWindow {

        private class InputEventWrapper {

            //-----------------------------------------------------------------------------
            // Properties
            //-----------------------------------------------------------------------------

            public bool DownArrow { get; private set; }
            public bool UpArrow { get; private set; }
            public bool ScrollPositive { get; private set; }
            public bool ScrollNegative { get; private set; }
            public bool Confirm { get; private set; }


            //-----------------------------------------------------------------------------
            // Methods
            //-----------------------------------------------------------------------------

            public void ClearInputs() {

                this.DownArrow = false;
                this.UpArrow = false;
                this.ScrollPositive = false;
                this.ScrollNegative = false;
                this.Confirm = false;
            }

            //-----------------------------------------------------------------------------

            public void GatherInputs() {

                this.ClearInputs();
                Event cur = Event.current;
                if (cur != null) {

                    if (cur.isKey && cur.type == EventType.KeyUp) {

                        if (cur.keyCode == KeyCode.DownArrow) {
                            this.DownArrow = true;
                        }
                        else if (cur.keyCode == KeyCode.UpArrow) {
                            this.UpArrow = true;
                        }
                        else if (cur.keyCode == KeyCode.Return) {
                            this.Confirm = true;
                        }
                    }
                    else if (cur.type == EventType.ScrollWheel) {

                        float delta = cur.delta.y;
                        if (delta > 0) {
                            this.ScrollPositive = true;
                        }
                        else if (delta < 0) {
                            this.ScrollNegative = true;
                        }
                    }
                }
            }
        }
    }
}
