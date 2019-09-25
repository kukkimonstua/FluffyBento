//#define Enable_Profiler

#if UNITY_2017_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define VERYANIMATION_TIMELINE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;

namespace VeryAnimation
{
    [Serializable]
    public class VeryAnimationEditorWindow : EditorWindow
    {
        public static VeryAnimationEditorWindow instance;

        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }

        #region GUI
        private bool editorPoseFoldout = true;
        private bool editorBlendPoseFoldout = true;
        private bool editorMuscleFoldout = true;
        private bool editorBlendShapeFoldout = false;
        private bool editorSelectionFoldout = true;

        public bool editorSelectionOnScene { get; private set; }

        private bool editorPoseHelp;
        private bool editorBlendPoseGroupHelp;
        private bool editorMuscleGroupHelp;
        private bool editorBlendShapeGroupHelp;
        private bool editorSelectionHelp;
        #endregion

        #region Strings
        private GUIContent[] RootCorrectionModeString = new GUIContent[(int)VeryAnimation.RootCorrectionMode.Total];
        #endregion

        #region Core
        [SerializeField]
        private BlendPoseTree blendPoseTree;
        [SerializeField]
        private MuscleGroupTree muscleGroupTree;
        [SerializeField]
        private BlendShapeTree blendShapeTree;
        #endregion

        private bool initialized;

        private Vector2 editorScrollPosition;

        private const int QuickSaveSize = 3;
        private PoseTemplate[] quickSaves;

        private string poseSaveDefaultDirectory;

        void OnEnable()
        {
            if (vaw == null || va == null) return;

            instance = this;

            poseSaveDefaultDirectory = Application.dataPath;

            UpdateRootCorrectionModeString();
            Language.OnLanguageChanged += UpdateRootCorrectionModeString;

            titleContent = new GUIContent("VA Editor");
        }
        void OnDisable()
        {
            if (vaw == null || va == null) return;

            Release();

            instance = null;

            if (vaw != null)
            {
                vaw.Release();
            }
        }
        void OnDestroy()
        {
            if (vaw != null)
            {
                vaw.Release();
            }
        }

        public void Initialize()
        {
            Release();

            blendPoseTree = new BlendPoseTree();
            muscleGroupTree = new MuscleGroupTree();
            blendShapeTree = new BlendShapeTree();

            #region EditorPref
            {
                editorPoseFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Pose", true);
                editorBlendPoseFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_BlendPose", false);
                editorMuscleFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Muscle", true);
                editorBlendShapeFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_BlendShape", false);
                editorSelectionFoldout = EditorPrefs.GetBool("VeryAnimation_Editor_Selection", true);

                editorSelectionOnScene = EditorPrefs.GetBool("VeryAnimation_Editor_Selection_OnScene", false);

                va.clampMuscle = EditorPrefs.GetBool("VeryAnimation_ClampMuscle", false);
                va.autoFootIK = EditorPrefs.GetBool("VeryAnimation_AutoFootIK", false);
                va.mirrorEnable = EditorPrefs.GetBool("VeryAnimation_MirrorEnable", false);
                va.collisionEnable = EditorPrefs.GetBool("VeryAnimation_CollisionEnable", false);
                va.rootCorrectionMode = (VeryAnimation.RootCorrectionMode)EditorPrefs.GetInt("VeryAnimation_RootCorrectionMode", (int)VeryAnimation.RootCorrectionMode.Single);
                muscleGroupTree.LoadEditorPref();
                blendShapeTree.LoadEditorPref();
            }
            #endregion

            initialized = true;
        }
        private void Release()
        {
            if (!initialized) return;

            #region EditorPref
            {
                EditorPrefs.SetBool("VeryAnimation_ClampMuscle", va.clampMuscle);
                EditorPrefs.SetBool("VeryAnimation_AutoFootIK", va.autoFootIK);
                EditorPrefs.SetBool("VeryAnimation_MirrorEnable", va.mirrorEnable);
                EditorPrefs.SetBool("VeryAnimation_CollisionEnable", va.collisionEnable);
                EditorPrefs.SetInt("VeryAnimation_RootCorrectionMode", (int)va.rootCorrectionMode);
                muscleGroupTree.SaveEditorPref();
                blendShapeTree.SaveEditorPref();
            }
            #endregion
        }

        void OnInspectorUpdate()
        {
            if (vaw == null || va == null || !vaw.initialized || VeryAnimationControlWindow.instance == null)
            {
                Close();
                return;
            }
        }

        void OnGUI()
        {
            if (va == null || !va.edit || va.isError || !vaw.guiStyleReady)
                return;

#if Enable_Profiler
            Profiler.BeginSample("****VeryAnimationEditorWindow.OnGUI");
#endif
            Event e = Event.current;

            #region Event
            switch (e.type)
            {
            case EventType.KeyDown:
                if (focusedWindow == this)
                    va.HotKeys();
                break;
            case EventType.MouseUp:
                SceneView.RepaintAll();
                break;
            }
            va.Commands();
            #endregion

            #region ToolBar
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    EditorGUI.BeginChangeCheck();
                    editorPoseFoldout = GUILayout.Toggle(editorPoseFoldout, "Pose", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Pose", editorPoseFoldout);
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendPoseFoldout = GUILayout.Toggle(editorBlendPoseFoldout, "Blend Pose", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendPose", editorBlendPoseFoldout);
                    }
                }
                if (va.isHuman)
                {
                    EditorGUI.BeginChangeCheck();
                    editorMuscleFoldout = GUILayout.Toggle(editorMuscleFoldout, "Muscle Group", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Muscle", editorMuscleFoldout);
                    }
                }
                if (blendShapeTree.IsHaveBlendShapeNodes())
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendShapeFoldout = GUILayout.Toggle(editorBlendShapeFoldout, "Blend Shape", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendShape", editorBlendShapeFoldout);
                    }
                }
                {
                    EditorGUI.BeginChangeCheck();
                    editorSelectionFoldout = GUILayout.Toggle(editorSelectionFoldout, "Selection", EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Selection", editorSelectionFoldout);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            if (va.isHuman)
                HumanoidEditorGUI();
            else
                GenericEditorGUI();

#if Enable_Profiler
            Profiler.EndSample();
#endif
        }

        private void HumanoidEditorGUI()
        {
            Event e = Event.current;
            #region Tools
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Options", EditorStyles.miniLabel, GUILayout.Width(48f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.clampMuscle, Language.GetContent(Language.Help.EditorOptionsClamp), EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Clamp");
                            va.clampMuscle = flag;
                        }
                    }
                    {
#if VERYANIMATION_TIMELINE
                        if (va.uAw_2017_1.GetLinkedWithTimeline())
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Toggle(true, Language.GetContent(Language.Help.EditorOptionsFootIK), EditorStyles.miniButton);
                            EditorGUI.EndDisabledGroup();
                        }
                        else
#endif
                        {
                            EditorGUI.BeginChangeCheck();
                            var flag = GUILayout.Toggle(va.autoFootIK, Language.GetContent(Language.Help.EditorOptionsFootIK), EditorStyles.miniButton);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(this, "Change Foot IK");
                                va.autoFootIK = flag;
                                va.SetAnimationWindowSynchroSelection();
                            }
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.mirrorEnable, Language.GetContent(Language.Help.EditorOptionsMirror), EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Mirror");
                            va.mirrorEnable = flag;
                            va.SetAnimationWindowSynchroSelection();
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.collisionEnable, Language.GetContent(Language.Help.EditorOptionsCollision), EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Collision");
                            va.collisionEnable = flag;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(Language.GetContent(Language.Help.EditorRootCorrection), EditorStyles.miniLabel, GUILayout.Width(88f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var mode = (VeryAnimation.RootCorrectionMode)GUILayout.Toolbar((int)va.rootCorrectionMode, RootCorrectionModeString, EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Root Correction Mode");
                            va.rootCorrectionMode = mode;
                            va.SetAnimationWindowSynchroSelection();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);

            EditorGUI_PoseGUI();

            EditorGUI_BlendPoseGUI();

            EditorGUI_MuscleGroupGUI();

            EditorGUI_BlendShapeGUI();

            EditorGUI_SelectionGUI();

            EditorGUILayout.EndScrollView();
        }
        private void GenericEditorGUI()
        {
            Event e = Event.current;
            #region Tools
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Options", GUILayout.Width(52f));
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.mirrorEnable, Language.GetContent(Language.Help.EditorOptionsMirror), EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Mirror");
                            va.mirrorEnable = flag;
                        }
                    }
                    {
                        EditorGUI.BeginChangeCheck();
                        var flag = GUILayout.Toggle(va.collisionEnable, Language.GetContent(Language.Help.EditorOptionsCollision), EditorStyles.miniButton);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, "Change Collision");
                            va.collisionEnable = flag;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);

            EditorGUI_PoseGUI();

            EditorGUI_BlendPoseGUI();

            EditorGUI_BlendShapeGUI();

            EditorGUI_SelectionGUI();

            EditorGUILayout.EndScrollView();
        }
        private void EditorGUI_PoseGUI()
        {
            {
                if (editorPoseFoldout)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        editorPoseFoldout = EditorGUILayout.Foldout(editorPoseFoldout, "Pose", true, vaw.guiStyleBoldFoldout);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorPrefs.SetBool("VeryAnimation_Editor_Pose", editorPoseFoldout);
                        }
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), editorPoseHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        editorPoseHelp = !editorPoseHelp;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (editorPoseHelp)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpPose), MessageType.Info);
                    }

                    EditorGUILayout.BeginVertical(vaw.guiStyleSkinBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        #region Reset
                        if (va.isHuman)
                        {
                            if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseHumanoidReset)))
                            {
                                Undo.RecordObject(this, "Reset Pose");
                                va.SetPoseHumanoidDefault();
                            }
                        }
                        #endregion
                        #region Bind or Start
                        {
                            if (va.transformPoseSave.IsEnableBindTransform())
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseBind)))
                                {
                                    Undo.RecordObject(this, "Bind Pose");
                                    va.SetPoseBind();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseStart)))
                                {
                                    Undo.RecordObject(this, "Edit Start Pose");
                                    va.SetPoseEditStart();
                                }
                            }
                        }
                        #endregion
                        #region Prefab
                        {
                            if (va.transformPoseSave.IsEnablePrefabTransform())
                            {
                                if (GUILayout.Button(Language.GetContent(Language.Help.EditorPosePrefab)))
                                {
                                    Undo.RecordObject(this, "Prefab Pose");
                                    va.SetPosePrefab();
                                }
                            }
                        }
                        #endregion
                        #region Mirror
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseMirror)))
                        {
                            Undo.RecordObject(this, "Mirror Pose");
                            va.SetPoseMirror();
                        }
                        #endregion
                        #region Template
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseTemplate), vaw.guiStyleDropDown))
                        {
                            Dictionary<string, string> poseTemplates = new Dictionary<string, string>();
                            {
                                var guids = AssetDatabase.FindAssets("t:posetemplate");
                                for (int i = 0; i < guids.Length; i++)
                                {
                                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                                    var name = path.Remove(0, "Assets/".Length);
                                    poseTemplates.Add(name, path);
                                }
                            }

                            GenericMenu menu = new GenericMenu();
                            {
                                var enu = poseTemplates.GetEnumerator();
                                while (enu.MoveNext())
                                {
                                    var value = enu.Current.Value;
                                    menu.AddItem(new GUIContent(enu.Current.Key), false, () =>
                                    {
                                        var poseTemplate = AssetDatabase.LoadAssetAtPath<PoseTemplate>(value);
                                        if (poseTemplate != null)
                                        {
                                            Undo.RecordObject(this, "Template Pose");
                                            Undo.RegisterCompleteObjectUndo(va.currentClip, "Template Pose");
                                            va.LoadPoseTemplate(poseTemplate, true);
                                        }
                                        else
                                        {
                                            Debug.LogErrorFormat(Language.GetText(Language.Help.LogFailedLoadPoseError), value);
                                        }
                                    });
                                }
                            }
                            menu.ShowAsContext();
                        }
                        #endregion
                        EditorGUILayout.Space();
                        #region Save as
                        if (GUILayout.Button(Language.GetContent(Language.Help.EditorPoseSaveAs)))
                        {
                            string path = EditorUtility.SaveFilePanel("Save as Pose Template", poseSaveDefaultDirectory, string.Format("{0}.asset", va.currentClip.name), "asset");
                            if (!string.IsNullOrEmpty(path))
                            {
                                if (!path.StartsWith(Application.dataPath))
                                {
                                    EditorCommon.SaveInsideAssetsFolderDisplayDialog();
                                }
                                else
                                {
                                    poseSaveDefaultDirectory = Path.GetDirectoryName(path);
                                    path = FileUtil.GetProjectRelativePath(path);
                                    var poseTemplate = ScriptableObject.CreateInstance<PoseTemplate>();
                                    va.SavePoseTemplate(poseTemplate);
                                    AssetDatabase.CreateAsset(poseTemplate, path);
                                    Focus();
                                }
                            }
                        }
                        #endregion
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(4);
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Quick Load", GUILayout.Width(70));
                            Action<int> QuickLoad = (index) =>
                            {
                                EditorGUI.BeginDisabledGroup(quickSaves == null || index >= quickSaves.Length || quickSaves[index] == null);
                                if (GUILayout.Button((index + 1).ToString()))
                                {
                                    Undo.RecordObject(this, "Quick Load");
                                    Undo.RegisterCompleteObjectUndo(va.currentClip, "Quick Load");
                                    va.LoadPoseTemplate(quickSaves[index], false);
                                }
                                EditorGUI.EndDisabledGroup();
                            };
                            for (int i = 0; i < QuickSaveSize; i++)
                            {
                                QuickLoad(i);
                            }
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Quick Save", GUILayout.Width(70));
                            Action<int> QuickSave = (index) =>
                            {
                                if (GUILayout.Button((index + 1).ToString()))
                                {
                                    Undo.RecordObject(this, "Quick Save");
                                    if (quickSaves == null || quickSaves.Length != QuickSaveSize)
                                        quickSaves = new PoseTemplate[QuickSaveSize];
                                    {
                                        quickSaves[index] = ScriptableObject.CreateInstance<PoseTemplate>();
                                        va.SavePoseTemplate(quickSaves[index]);
                                    }
                                }
                            };
                            for (int i = 0; i < QuickSaveSize; i++)
                            {
                                QuickSave(i);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(3);
                    EditorGUILayout.EndVertical();
                }
            }
        }
        private void EditorGUI_BlendPoseGUI()
        {
            if (editorBlendPoseFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendPoseFoldout = EditorGUILayout.Foldout(editorBlendPoseFoldout, "Blend Pose", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendPose", editorBlendPoseFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    blendPoseTree.BlendPoseTreeToolbarGUI();
                    EditorGUILayout.Space();
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), editorBlendPoseGroupHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        editorBlendPoseGroupHelp = !editorBlendPoseGroupHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorBlendPoseGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpBlendPose), MessageType.Info);
                }

                blendPoseTree.BlendPoseTreeGUI();
            }
        }
        private void EditorGUI_MuscleGroupGUI()
        {
            if (editorMuscleFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorMuscleFoldout = EditorGUILayout.Foldout(editorMuscleFoldout, "Muscle Group", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Muscle", editorMuscleFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    muscleGroupTree.MuscleGroupToolbarGUI();
                    EditorGUILayout.Space();
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), editorMuscleGroupHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        editorMuscleGroupHelp = !editorMuscleGroupHelp;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorMuscleGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpMuscleGroup), MessageType.Info);
                }

                muscleGroupTree.MuscleGroupTreeGUI();
            }
        }
        private void EditorGUI_BlendShapeGUI()
        {
            if (!blendShapeTree.IsHaveBlendShapeNodes())
                return;

            if (editorBlendShapeFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    editorBlendShapeFoldout = EditorGUILayout.Foldout(editorBlendShapeFoldout, "Blend Shape", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_BlendShape", editorBlendShapeFoldout);
                    }
                }
                {
                    EditorGUILayout.Space();
                    blendShapeTree.BlendShapeTreeToolbarGUI();
                    EditorGUILayout.Space();
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), editorBlendShapeGroupHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        editorBlendShapeGroupHelp = !editorBlendShapeGroupHelp;
                    }
                    if (EditorGUILayout.DropdownButton(vaw.uEditorGUI.GetTitleSettingsIcon(), FocusType.Passive, vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        blendShapeTree.BlendShapeTreeSettingsMesh();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (editorBlendShapeGroupHelp)
                {
                    EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpBlendShape), MessageType.Info);
                }

                blendShapeTree.BlendShapeTreeGUI();
            }
        }
        public void EditorGUI_SelectionGUI(bool onScene = false)
        {
            const int FoldoutSpace = 17;
            const int FloatFieldWidth = 44;

            if (editorSelectionFoldout && onScene == editorSelectionOnScene)
            {
                EditorGUILayout.BeginHorizontal();
                if (!onScene)
                {
                    EditorGUI.BeginChangeCheck();
                    editorSelectionFoldout = EditorGUILayout.Foldout(editorSelectionFoldout, "Selection", true, vaw.guiStyleBoldFoldout);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool("VeryAnimation_Editor_Selection", editorSelectionFoldout);
                    }
                }
                if (va.selectionActiveGameObject != null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(va.selectionActiveGameObject, typeof(GameObject), false);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None && va.animatorIK.ikData[(int)va.animatorIK.ikActiveTarget].enable)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Animator IK: " + AnimatorIKCore.IKTargetStrings[(int)va.animatorIK.ikActiveTarget]);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.originalIK.ikActiveTarget >= 0 && va.originalIK.ikData[va.originalIK.ikActiveTarget].enable)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Original IK: " + va.originalIK.ikData[va.originalIK.ikActiveTarget].name);
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.selectionHumanVirtualBones != null && va.selectionHumanVirtualBones.Count > 0)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Virtual: " + va.selectionHumanVirtualBones[0].ToString());
                    EditorGUI.EndDisabledGroup();
                }
                else if (va.selectionMotionTool)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Motion");
                    EditorGUI.EndDisabledGroup();
                }
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(vaw.uEditorGUI.GetHelpIcon(), editorSelectionHelp ? vaw.guiStyleIconActiveButton : vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        editorSelectionHelp = !editorSelectionHelp;
                        vaw.editorWindowSelectionRect.size = Vector2.zero;
                    }
                    if (EditorGUILayout.DropdownButton(vaw.uEditorGUI.GetTitleSettingsIcon(), FocusType.Passive, vaw.guiStyleIconButton, GUILayout.Width(19)))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(Language.GetContent(Language.Help.EditorMenuOnScene), editorSelectionOnScene, () =>
                        {
                            editorSelectionOnScene = !editorSelectionOnScene;
                            EditorPrefs.SetBool("VeryAnimation_Editor_Selection_OnScene", editorSelectionOnScene);
                            vaw.editorWindowSelectionRect.size = Vector2.zero;
                            Repaint();
                            SceneView.RepaintAll();
                        });
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();
                {
                    if (editorSelectionHelp)
                    {
                        EditorGUILayout.HelpBox(Language.GetText(Language.Help.HelpSelection), MessageType.Info);
                    }

                    EditorGUILayout.BeginVertical(vaw.guiStyleSkinBox);
                    {
                        var humanoidIndex = va.SelectionGameObjectHumanoidIndex();
                        var boneIndex = va.selectionActiveBone;
                        if (va.isHuman && (humanoidIndex >= 0 || boneIndex == va.rootMotionBoneIndex))
                        {
                            #region Humanoid
                            if (humanoidIndex == HumanBodyBones.Hips)
                            {
                                EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionHip), EditorStyles.centeredGreyMiniLabel);
                            }
                            else if (humanoidIndex > HumanBodyBones.Hips || va.selectionActiveGameObject == vaw.gameObject)
                            {
                                EditorGUILayout.BeginHorizontal();
                                #region Mirror
                                var mirrorIndex = humanoidIndex >= 0 && va.humanoidIndex2boneIndex[(int)humanoidIndex] >= 0 ? va.mirrorBoneIndexes[va.humanoidIndex2boneIndex[(int)humanoidIndex]] : -1;
                                if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, (mirrorIndex >= 0 ? string.Format("From '{0}'", va.bones[mirrorIndex].name) : "From self")), GUILayout.Width(100)))
                                {
                                    va.SelectionHumanoidMirror();
                                }
                                #endregion
                                EditorGUILayout.Space();
                                #region Reset
                                if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                                {
                                    va.SelectionHumanoidResetAll();
                                }
                                #endregion
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space();
                            }
                            int RowCount = 0;
                            if (boneIndex == va.rootMotionBoneIndex)
                            {
                                #region Root
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent("Position", "RootT"), GUILayout.Width(60)))
                                    {
                                        va.lastTool = Tool.Move;
                                        va.SelectGameObject(vaw.gameObject);
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var rootT = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootT());
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        va.SetAnimationValueAnimatorRootT(rootT);
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                    {
                                        va.SetAnimationValueAnimatorRootTIfNotOriginal(new Vector3(0, 1, 0));
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent("Rotation", "RootQ"), GUILayout.Width(60)))
                                    {
                                        va.lastTool = Tool.Rotate;
                                        va.SelectGameObject(vaw.gameObject);
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var rootQ = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorRootQ().eulerAngles);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        va.SetAnimationValueAnimatorRootQ(Quaternion.Euler(rootQ));
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                    {
                                        va.SetAnimationValueAnimatorRootQIfNotOriginal(Quaternion.identity);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion
                            }
                            else if (humanoidIndex > HumanBodyBones.Hips)
                            {
                                #region Muscle
                                if (vaw.muscleRotationSliderIds == null || vaw.muscleRotationSliderIds.Length != 3)
                                    vaw.muscleRotationSliderIds = new int[3];
                                for (int i = 0; i < vaw.muscleRotationSliderIds.Length; i++)
                                    vaw.muscleRotationSliderIds[i] = -1;
                                for (int i = 0; i < 3; i++)
                                {
                                    var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, i);
                                    if (muscleIndex < 0) continue;
                                    var muscleValue = va.GetAnimationValueAnimatorMuscle(muscleIndex);
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent(va.musclePropertyName.Names[muscleIndex], va.musclePropertyName.Names[muscleIndex]), GUILayout.Width(vaw.editorSettings.settingEditorNameFieldWidth)))
                                    {
                                        va.lastTool = Tool.Rotate;
                                        va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                        va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingAnimatorMuscle(muscleIndex) });
                                    }
                                    {
                                        var mmuscleIndex = va.GetMirrorMuscleIndex(muscleIndex);
                                        if (mmuscleIndex >= 0)
                                        {
                                            if (GUILayout.Button(new GUIContent("", string.Format("Mirror: '{0}'", va.musclePropertyName.Names[mmuscleIndex])), vaw.guiStyleMirrorButton, GUILayout.Width(vaw.mirrorTex.width), GUILayout.Height(vaw.mirrorTex.height)))
                                            {
                                                var mhumanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(mmuscleIndex);
                                                va.SelectHumanoidBones(new HumanBodyBones[] { mhumanoidIndex });
                                                va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingAnimatorMuscle(mmuscleIndex) });
                                            }
                                        }
                                        else
                                        {
                                            GUILayout.Space(FoldoutSpace);
                                        }
                                    }
                                    {
                                        var saveBackgroundColor = GUI.backgroundColor;
                                        switch (i)
                                        {
                                        case 0: GUI.backgroundColor = Handles.xAxisColor; break;
                                        case 1: GUI.backgroundColor = Handles.yAxisColor; break;
                                        case 2: GUI.backgroundColor = Handles.zAxisColor; break;
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        muscleValue = GUILayout.HorizontalSlider(muscleValue, -1f, 1f);
                                        vaw.muscleRotationSliderIds[i] = vaw.uEditorGUIUtility.GetLastControlID();
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                            {
                                                va.SetAnimationValueAnimatorMuscle(mi, muscleValue);
                                            }
                                        }
                                        GUI.backgroundColor = saveBackgroundColor;
                                    }
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        var value2 = EditorGUILayout.FloatField(muscleValue, GUILayout.Width(FloatFieldWidth));
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                            {
                                                va.SetAnimationValueAnimatorMuscleIfNotOriginal(mi, value2);
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion

                                #region Rotation
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent("Rotation", "Muscles"), GUILayout.Width(60)))
                                    {
                                        va.lastTool = Tool.Rotate;
                                        va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                    }
                                    {
                                        var muscleIndex0 = HumanTrait.MuscleFromBone((int)humanoidIndex, 0);
                                        var muscleIndex1 = HumanTrait.MuscleFromBone((int)humanoidIndex, 1);
                                        var muscleIndex2 = HumanTrait.MuscleFromBone((int)humanoidIndex, 2);
                                        var euler = new Vector3(va.Muscle2EulerAngle(muscleIndex0, va.GetAnimationValueAnimatorMuscle(muscleIndex0)),
                                                                va.Muscle2EulerAngle(muscleIndex1, va.GetAnimationValueAnimatorMuscle(muscleIndex1)),
                                                                va.Muscle2EulerAngle(muscleIndex2, va.GetAnimationValueAnimatorMuscle(muscleIndex2)));
                                        EditorGUI.BeginChangeCheck();
                                        euler = EditorGUILayout.Vector3Field("", euler);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            for (int i = 0; i < 3; i++)
                                            {
                                                var muscleValue = va.EulerAngle2Muscle(HumanTrait.MuscleFromBone((int)humanoidIndex, i), euler[i]);
                                                foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                                {
                                                    va.SetAnimationValueAnimatorMuscle(mi, muscleValue);
                                                }
                                            }
                                        }
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            foreach (var mi in va.SelectionGameObjectsMuscleIndex(i))
                                            {
                                                va.SetAnimationValueAnimatorMuscleIfNotOriginal(mi, 0f);
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion

                                #region Position(TDOF)
                                if (va.humanoidHasTDoF && VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex] != null)
                                {
                                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                    if (GUILayout.Button(new GUIContent("Position", "TDOF"), GUILayout.Width(60)))
                                    {
                                        va.lastTool = Tool.Move;
                                        va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    var tdof = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)humanoidIndex].index));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                        {
                                            if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                                            va.SetAnimationValueAnimatorTDOF(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index, tdof);
                                        }
                                    }
                                    if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                    {
                                        foreach (var hi in va.SelectionGameObjectsHumanoidIndex())
                                        {
                                            if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                                            va.SetAnimationValueAnimatorTDOFIfNotOriginal(VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index, Vector3.zero);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else if (boneIndex >= 0)
                        {
                            #region Generic
                            if (va.isHuman && va.humanoidConflict[boneIndex])
                            {
                                EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionHumanoidConflict), EditorStyles.centeredGreyMiniLabel);
                            }
                            else
                            {
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    #region Mirror
                                    if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, (va.mirrorBoneIndexes[boneIndex] >= 0 ? string.Format("From '{0}'", va.bones[va.mirrorBoneIndexes[boneIndex]].name) : "From self")), GUILayout.Width(100)))
                                    {
                                        va.SelectionGenericMirror();
                                    }
                                    #endregion
                                    EditorGUILayout.Space();
                                    #region Reset
                                    if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                                    {
                                        va.SelectionGenericResetAll();
                                    }
                                    #endregion
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUILayout.Space();
                                int RowCount = 0;
                                {
                                    #region Position
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Position", GUILayout.Width(60)))
                                        {
                                            va.lastTool = Tool.Move;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localPosition = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformPosition(boneIndex));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformPosition(bi, localPosition);
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                va.SetAnimationValueTransformPositionIfNotOriginal(bi, va.boneSaveTransforms[bi].localPosition);
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                    #region Rotation
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Rotation", GUILayout.Width(60)))
                                        {
                                            va.lastTool = Tool.Rotate;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localEulerAngles = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformRotation(boneIndex).eulerAngles);
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformRotation(bi, Quaternion.Euler(localEulerAngles));
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                va.SetAnimationValueTransformRotationIfNotOriginal(bi, va.boneSaveTransforms[bi].localRotation);
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                    #region Scale
                                    {
                                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                        if (GUILayout.Button("Scale", GUILayout.Width(60)))
                                        {
                                            va.lastTool = Tool.Scale;
                                            va.SelectGameObject(va.bones[boneIndex]);
                                        }
                                        EditorGUI.BeginChangeCheck();
                                        {
                                            var localScale = EditorGUILayout.Vector3Field("", va.GetAnimationValueTransformScale(boneIndex));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                                {
                                                    va.SetAnimationValueTransformScale(bi, localScale);
                                                }
                                            }
                                        }
                                        if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                        {
                                            foreach (var bi in va.SelectionGameObjectsOtherHumanoidBoneIndex())
                                            {
                                                va.SetAnimationValueTransformScaleIfNotOriginal(boneIndex, va.boneSaveTransforms[bi].localScale);
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else if (va.selectionMotionTool)
                        {
                            #region Motion
                            {
                                EditorGUILayout.BeginHorizontal();
                                #region Mirror
                                if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, "From self"), GUILayout.Width(100)))
                                {
                                    va.SelectionCommonMirror();
                                }
                                #endregion
                                EditorGUILayout.Space();
                                #region Reset
                                if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                                {
                                    va.SelectionCommonResetAll();
                                }
                                #endregion
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space();
                            }
                            int RowCount = 0;
                            {
                                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                if (GUILayout.Button(new GUIContent("Position", "MotionT"), GUILayout.Width(60)))
                                {
                                    va.lastTool = Tool.Move;
                                    va.SelectMotionTool();
                                }
                                EditorGUI.BeginChangeCheck();
                                var motionT = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorMotionT());
                                if (EditorGUI.EndChangeCheck())
                                {
                                    va.SetAnimationValueAnimatorMotionT(motionT);
                                }
                                if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                {
                                    va.SetAnimationValueAnimatorMotionTIfNotOriginal(Vector3.zero);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            {
                                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                                if (GUILayout.Button(new GUIContent("Rotation", "MotionQ"), GUILayout.Width(60)))
                                {
                                    va.lastTool = Tool.Rotate;
                                    va.SelectMotionTool();
                                }
                                EditorGUI.BeginChangeCheck();
                                var motionQ = EditorGUILayout.Vector3Field("", va.GetAnimationValueAnimatorMotionQ().eulerAngles);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    va.SetAnimationValueAnimatorMotionQ(Quaternion.Euler(motionQ));
                                }
                                if (GUILayout.Button("Reset", GUILayout.Width(FloatFieldWidth)))
                                {
                                    va.SetAnimationValueAnimatorMotionQIfNotOriginal(Quaternion.identity);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            #endregion
                        }
                        else if (va.animatorIK.ikActiveTarget != AnimatorIKCore.IKTarget.None)
                        {
                            va.animatorIK.SelectionGUI();
                        }
                        else if (va.originalIK.ikActiveTarget >= 0)
                        {
                            va.originalIK.SelectionGUI();
                        }
                        else
                        {
                            EditorGUILayout.LabelField(Language.GetText(Language.Help.SelectionNothingisselected), EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void UpdateRootCorrectionModeString()
        {
            for (int i = 0; i < (int)VeryAnimation.RootCorrectionMode.Total; i++)
            {
                RootCorrectionModeString[i] = new GUIContent(Language.GetContent(Language.Help.EditorRootCorrectionDisable + i));
            }
        }

        public static void ForceRepaint()
        {
            if (instance == null) return;
            instance.Repaint();
        }
    }
}
