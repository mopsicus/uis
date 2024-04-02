using UnityEditor;
using UnityEngine;

namespace UIS {

    [CustomEditor(typeof(Scroller))]
    public class ScrollerEditor : Editor {

        /// <summary>
        /// Scroller target
        /// </summary>
        Scroller _target;

        /// <summary>
        /// Serialized target object
        /// </summary>
        SerializedObject _object;

        /// <summary>
        /// Item list prefab
        /// </summary>
        SerializedProperty _prefab;

        /// <summary>
        /// Top padding
        /// </summary>
        SerializedProperty _topPadding;

        /// <summary>
        /// Bottom padding
        /// </summary>
        SerializedProperty _bottomPadding;

        /// <summary>
        /// Spacing between items
        /// </summary>
        SerializedProperty _itemSpacing;

        /// <summary>
        /// Label font asset
        /// </summary>
        SerializedProperty _labelsFont;

        /// <summary>
        /// Label font color
        /// </summary>
        SerializedProperty _fontColor;

        /// <summary>
        /// Label font size
        /// </summary>
        SerializedProperty _fontSize;

        /// <summary>
        /// Pull top text label
        /// </summary>
        SerializedProperty _topPullLabel;

        /// <summary>
        /// Release top text label
        /// </summary>
        SerializedProperty _topReleaseLabel;

        /// <summary>
        /// Pull bottom text label
        /// </summary>
        SerializedProperty _bottomPullLabel;

        /// <summary>
        /// Release bottom text label
        /// </summary>
        SerializedProperty _bottomReleaseLabel;

        /// <summary>
        /// Can we pull from top
        /// </summary>
        SerializedProperty _isPullTop;

        /// <summary>
        /// Can we pull from bottom
        /// </summary>
        SerializedProperty _isPullBottom;

        /// <summary>
        /// Left padding
        /// </summary>
        SerializedProperty _leftPadding;

        /// <summary>
        /// Right padding
        /// </summary>
        SerializedProperty _rightPadding;

        /// <summary>
        /// Pull left text label
        /// </summary>
        SerializedProperty _leftPullLabel;

        /// <summary>
        /// Release left text label
        /// </summary>
        SerializedProperty _leftReleaseLabel;

        /// <summary>
        /// Pull right text label
        /// </summary>
        SerializedProperty _rightPullLabel;

        /// <summary>
        /// Release right text label
        /// </summary>
        SerializedProperty _rightReleaseLabel;

        /// <summary>
        /// Can we pull from left
        /// </summary>
        SerializedProperty _isPullLeft;

        /// <summary>
        /// Can we pull from right
        /// </summary>
        SerializedProperty _isPullRight;

        /// <summary>
        /// Coefficient when labels should action
        /// </summary>
        SerializedProperty _pullValue;

        /// <summary>
        /// Label position offset
        /// </summary>
        SerializedProperty _labelOffset;

        /// <summary>
        /// Container for calc width/height if anchors exists
        /// </summary>
        SerializedProperty _parentContainer;

        /// <summary>
        /// Init data
        /// </summary>
        void OnEnable() {
            _target = (Scroller)target;
            _object = new SerializedObject(target);
            _prefab = _object.FindProperty("Prefab");
            _topPadding = _object.FindProperty("TopPadding");
            _bottomPadding = _object.FindProperty("BottomPadding");
            _itemSpacing = _object.FindProperty("ItemSpacing");
            _labelsFont = _object.FindProperty("LabelsFont");
            _fontColor = _object.FindProperty("FontColor");
            _fontSize = _object.FindProperty("FontSize");
            _topPullLabel = _object.FindProperty("TopPullLabel");
            _topReleaseLabel = _object.FindProperty("TopReleaseLabel");
            _bottomPullLabel = _object.FindProperty("BottomPullLabel");
            _bottomReleaseLabel = _object.FindProperty("BottomReleaseLabel");
            _isPullTop = _object.FindProperty("IsPullTop");
            _isPullBottom = _object.FindProperty("IsPullBottom");
            _leftPadding = _object.FindProperty("LeftPadding");
            _rightPadding = _object.FindProperty("RightPadding");
            _leftPullLabel = _object.FindProperty("LeftPullLabel");
            _leftReleaseLabel = _object.FindProperty("LeftReleaseLabel");
            _rightPullLabel = _object.FindProperty("RightPullLabel");
            _rightReleaseLabel = _object.FindProperty("RightReleaseLabel");
            _isPullLeft = _object.FindProperty("IsPullLeft");
            _isPullRight = _object.FindProperty("IsPullRight");
            _pullValue = _object.FindProperty("PullValue");
            _labelOffset = _object.FindProperty("LabelOffset");
            _parentContainer = _object.FindProperty("ParentContainer");
        }

        /// <summary>
        /// Draw inspector
        /// </summary>
        public override void OnInspectorGUI() {
            _object.Update();
            EditorGUI.BeginChangeCheck();
            _target.Type = GUILayout.Toolbar(_target.Type, new string[] { "Vertical", "Horizontal" });
            switch (_target.Type) {
                case 0:
                    EditorGUILayout.PropertyField(_prefab);
                    EditorGUILayout.PropertyField(_topPadding);
                    EditorGUILayout.PropertyField(_bottomPadding);
                    EditorGUILayout.PropertyField(_itemSpacing);
                    EditorGUILayout.PropertyField(_labelsFont);
                    EditorGUILayout.PropertyField(_fontColor);
                    EditorGUILayout.PropertyField(_fontSize);
                    EditorGUILayout.PropertyField(_topPullLabel);
                    EditorGUILayout.PropertyField(_topReleaseLabel);
                    EditorGUILayout.PropertyField(_bottomPullLabel);
                    EditorGUILayout.PropertyField(_bottomReleaseLabel);
                    EditorGUILayout.PropertyField(_isPullTop);
                    EditorGUILayout.PropertyField(_isPullBottom);
                    EditorGUILayout.PropertyField(_pullValue);
                    EditorGUILayout.PropertyField(_labelOffset);
                    EditorGUILayout.PropertyField(_parentContainer);
                    break;
                case 1:
                    EditorGUILayout.PropertyField(_prefab);
                    EditorGUILayout.PropertyField(_leftPadding);
                    EditorGUILayout.PropertyField(_rightPadding);
                    EditorGUILayout.PropertyField(_itemSpacing);
                    EditorGUILayout.PropertyField(_labelsFont);
                    EditorGUILayout.PropertyField(_fontColor);
                    EditorGUILayout.PropertyField(_fontSize);
                    EditorGUILayout.PropertyField(_leftPullLabel);
                    EditorGUILayout.PropertyField(_leftReleaseLabel);
                    EditorGUILayout.PropertyField(_rightPullLabel);
                    EditorGUILayout.PropertyField(_rightReleaseLabel);
                    EditorGUILayout.PropertyField(_isPullLeft);
                    EditorGUILayout.PropertyField(_isPullRight);
                    EditorGUILayout.PropertyField(_pullValue);
                    EditorGUILayout.PropertyField(_labelOffset);
                    EditorGUILayout.PropertyField(_parentContainer);
                    break;
                default:
                    break;
            }
            if (EditorGUI.EndChangeCheck()) {
                _object.ApplyModifiedProperties();
            }
        }
    }
}