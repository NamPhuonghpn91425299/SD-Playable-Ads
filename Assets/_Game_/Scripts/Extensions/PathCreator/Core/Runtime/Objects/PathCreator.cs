using System.Collections.Generic;
using UnityEngine;

namespace PathCreation {
    public class PathCreator : MonoBehaviour {

        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
        /// Attach to a GameObject to create a new path editor.

        public event System.Action pathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;

        GlobalDisplaySettings globalEditorDisplaySettings;

        // Vertex path created from the current bezier path
        public VertexPath path {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.GetVertexPath(transform);
            }
        }

        // The bezier path created in the editor
        public BezierPath bezierPath {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.bezierPath;
            }
            set {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                editorData.bezierPath = value;
            }
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData (bool in2DMode) {
            if (editorData == null) {
                editorData = new PathCreatorData ();
            }
            editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
            editorData.bezierOrVertexPathModified += TriggerPathUpdate;

            editorData.Initialize (in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData {
            get {
                return editorData;
            }

        }

        public void TriggerPathUpdate () {
            if (pathUpdated != null) {
                pathUpdated ();
            }
        }

#if UNITY_EDITOR

        // // Draw the path when path objected is not selected (if enabled in settings)
        // void OnDrawGizmos () {
        //
        //     // Only draw path gizmo if the path object is not selected
        //     // (editor script is resposible for drawing when selected)
        //     GameObject selectedObj = UnityEditor.Selection.activeGameObject;
        //     if (selectedObj != gameObject) {
        //
        //         if (path != null) {
        //             path.UpdateTransform (transform);
        //
        //             if (globalEditorDisplaySettings == null) {
        //                 globalEditorDisplaySettings = GlobalDisplaySettings.Load ();
        //             }
        //
        //             if (globalEditorDisplaySettings.visibleWhenNotSelected) {
        //
        //                 Gizmos.color = globalEditorDisplaySettings.bezierPath;
        //
        //                 for (int i = 0; i < path.NumPoints; i++) {
        //                     int nextI = i + 1;
        //                     if (nextI >= path.NumPoints) {
        //                         if (path.isClosedLoop) {
        //                             nextI %= path.NumPoints;
        //                         } else {
        //                             break;
        //                         }
        //                     }
        //                     Gizmos.DrawLine (path.GetPoint (i), path.GetPoint (nextI));
        //                 }
        //             }
        //         }
        //     }
        // }
        void OnDrawGizmos()
        {
            // Thoát sớm nếu object đang được chọn hoặc nếu path bằng null
            if (UnityEditor.Selection.activeGameObject == gameObject || path == null) return;

            path.UpdateTransform(transform);

            // Lấy cài đặt hiển thị toàn cục (nạp nếu chưa có)
            globalEditorDisplaySettings ??= GlobalDisplaySettings.Load();

            // Thoát sớm nếu tùy chọn không cho phép hiển thị khi object không được chọn
            if (!globalEditorDisplaySettings.visibleWhenNotSelected) return;

            Gizmos.color = globalEditorDisplaySettings.bezierPath;

            // Vẽ đường nối các điểm bằng vòng lặp
            for (int i = 0; i < path.NumPoints; i++)
            {
                int nextI = (i + 1) % path.NumPoints; // Tính điểm tiếp theo (vòng nếu là đường khép kín)
                if (!path.isClosedLoop && nextI == 0) break; // Thoát nếu kết thúc đường không khép kín
                Gizmos.DrawLine(path.GetPoint(i), path.GetPoint(nextI)); // Vẽ đường nối
            }
        }
#endif

        #endregion
    }
}