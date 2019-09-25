using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UAvatarPreview
    {
        public object instance { get; private set; }

        private FieldInfo fi_m_PreviewDir;
        private FieldInfo fi_m_ZoomFactor;
        private Action<object, int> dg_set_fps;
        private Func<object, AnimationClip> dg_get_m_SourcePreviewMotion;
        private Func<bool> dg_get_IKOnFeet;
        private Func<Animator> dg_get_Animator;
        private Func<bool> dg_get_ShowIKOnFeetButton;
        private Action<bool> dg_set_ShowIKOnFeetButton;
        private Func<GameObject> dg_get_PreviewObject;
        private Func<ModelImporterAnimationType> dg_get_animationClipType;
        private Action dg_DoPreviewSettings;
        private Action<Rect, GUIStyle> dg_DoAvatarPreview;
        private Action dg_OnDestroy;
        private Action<GameObject> dg_SetPreview;
        private PropertyInfo pi_OnAvatarChangeFunc;

        private UAnimatorController uAnimatorController;
        private UAnimatorStateMachine uAnimatorStateMachine;
        private UAnimatorState uAnimatorState;

        private UTimeControl uTimeControl;
        private UnityEditor.Animations.AnimatorController m_Controller;
        private AnimatorStateMachine m_StateMachine;
        private AnimatorState m_State;

        private GameObject gameObject;
        private Animator animator;
        private Animation animation;

        private TransformPoseSave transformPoseSave;
        private BlendShapeWeightSave blendShapeWeightSave;

#if !UNITY_2017_3_OR_NEWER
        private bool mode2D;
#endif

        private class UAvatarPreviewSelection
        {
            private Func<ModelImporterAnimationType, GameObject> dg_get_GetPreview;

            public UAvatarPreviewSelection(Assembly asmUnityEditor)
            {
                var avatarPreviewSelectionType = asmUnityEditor.GetType("UnityEditor.AvatarPreviewSelection");
                Assert.IsNotNull(dg_get_GetPreview = (Func<ModelImporterAnimationType, GameObject>)Delegate.CreateDelegate(typeof(Func<ModelImporterAnimationType, GameObject>), null, avatarPreviewSelectionType.GetMethod("GetPreview", BindingFlags.Public | BindingFlags.Static)));
            }

            public GameObject GetPreview(ModelImporterAnimationType type)
            {
                return dg_get_GetPreview(type);
            }
        }
        private UAvatarPreviewSelection uAvatarPreviewSelection;

        public static readonly string EditorPrefs2D = "AvatarpreviewCustom2D";
        public static readonly string EditorPrefsApplyRootMotion = "AvatarpreviewCustomApplyRootMotion";
        
        public UAvatarPreview(AnimationClip clip, GameObject gameObject)
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var avatarPreviewType = asmUnityEditor.GetType("UnityEditor.AvatarPreview");
            Assert.IsNotNull(instance = Activator.CreateInstance(avatarPreviewType, new object[] { null, clip }));
            Assert.IsNotNull(fi_m_PreviewDir = avatarPreviewType.GetField("m_PreviewDir", BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.IsNotNull(fi_m_ZoomFactor = avatarPreviewType.GetField("m_ZoomFactor", BindingFlags.NonPublic | BindingFlags.Instance));
            Assert.IsNotNull(dg_set_fps = EditorCommon.CreateSetFieldDelegate<int>(avatarPreviewType.GetField("fps")));
            Assert.IsNotNull(dg_get_m_SourcePreviewMotion = EditorCommon.CreateGetFieldDelegate<AnimationClip>(avatarPreviewType.GetField("m_SourcePreviewMotion", BindingFlags.NonPublic | BindingFlags.Instance)));
            Assert.IsNotNull(dg_get_IKOnFeet = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, avatarPreviewType.GetProperty("IKOnFeet").GetGetMethod()));
            Assert.IsNotNull(dg_get_Animator = (Func<Animator>)Delegate.CreateDelegate(typeof(Func<Animator>), instance, avatarPreviewType.GetProperty("Animator").GetGetMethod()));
            Assert.IsNotNull(dg_get_ShowIKOnFeetButton = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, avatarPreviewType.GetProperty("ShowIKOnFeetButton").GetGetMethod()));
            Assert.IsNotNull(dg_set_ShowIKOnFeetButton = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), instance, avatarPreviewType.GetProperty("ShowIKOnFeetButton").GetSetMethod()));
            Assert.IsNotNull(dg_get_PreviewObject = (Func<GameObject>)Delegate.CreateDelegate(typeof(Func<GameObject>), instance, avatarPreviewType.GetProperty("PreviewObject").GetGetMethod()));
            Assert.IsNotNull(dg_get_animationClipType = (Func<ModelImporterAnimationType>)Delegate.CreateDelegate(typeof(Func<ModelImporterAnimationType>), instance, avatarPreviewType.GetProperty("animationClipType").GetGetMethod()));
            Assert.IsNotNull(dg_DoPreviewSettings = (Action)Delegate.CreateDelegate(typeof(Action), instance, avatarPreviewType.GetMethod("DoPreviewSettings")));
            Assert.IsNotNull(dg_DoAvatarPreview = (Action<Rect, GUIStyle>)Delegate.CreateDelegate(typeof(Action<Rect, GUIStyle>), instance, avatarPreviewType.GetMethod("DoAvatarPreview")));
#if UNITY_2018_2_OR_NEWER
            Assert.IsNotNull(dg_OnDestroy = (Action)Delegate.CreateDelegate(typeof(Action), instance, avatarPreviewType.GetMethod("OnDisable")));
#else
            Assert.IsNotNull(dg_OnDestroy = (Action)Delegate.CreateDelegate(typeof(Action), instance, avatarPreviewType.GetMethod("OnDestroy")));
#endif
            Assert.IsNotNull(dg_SetPreview = (Action<GameObject>)Delegate.CreateDelegate(typeof(Action<GameObject>), instance, avatarPreviewType.GetMethod("SetPreview", BindingFlags.NonPublic | BindingFlags.Instance)));
            Assert.IsNotNull(pi_OnAvatarChangeFunc = avatarPreviewType.GetProperty("OnAvatarChangeFunc"));

            uAnimatorController = new UAnimatorController();
            uAnimatorStateMachine = new UAnimatorStateMachine();
            uAnimatorState = new UAnimatorState();

            {
                var fi_timeControl = avatarPreviewType.GetField("timeControl");
                uTimeControl = new UTimeControl(fi_timeControl.GetValue(instance));
                uTimeControl.startTime = 0f;
                uTimeControl.stopTime = clip.length;
                uTimeControl.currentTime = 0f;
            }
            uAvatarPreviewSelection = new UAvatarPreviewSelection(asmUnityEditor);

            pi_OnAvatarChangeFunc.SetValue(instance, Delegate.CreateDelegate(pi_OnAvatarChangeFunc.PropertyType, this, GetType().GetMethod("OnAvatarChangeFunc", BindingFlags.NonPublic | BindingFlags.Instance)), null);
            dg_set_fps(instance, (int)clip.frameRate);
            dg_SetPreview(gameObject);

            AnimationUtility.onCurveWasModified += OnCurveWasModified;
        }
        ~UAvatarPreview()
        {
            AnimationUtility.onCurveWasModified -= OnCurveWasModified;
            Assert.IsNull(m_Controller);
        }

        public void Release()
        {
            AnimationUtility.onCurveWasModified -= OnCurveWasModified;
            pi_OnAvatarChangeFunc.SetValue(instance, null, null);
            dg_SetPreview(null);
            DestroyController();
            dg_OnDestroy();
        }
        
        private void OnAvatarChangeFunc()
        {
            DestroyController();
            InitController();
        }
        
        private void InitController()
        {
            gameObject = dg_get_PreviewObject();
            if (gameObject == null) return;
            var originalGameObject = uAvatarPreviewSelection.GetPreview(dg_get_animationClipType());

            animator = dg_get_Animator();
            animation = gameObject.GetComponent<Animation>();
            if (originalGameObject != null)
            {
                transformPoseSave = new TransformPoseSave(originalGameObject);
                transformPoseSave.SetRootTransform(originalGameObject.transform.position, originalGameObject.transform.rotation, originalGameObject.transform.lossyScale);
                transformPoseSave.ChangeTransforms(gameObject);
            }
            else
            {
                transformPoseSave = new TransformPoseSave(gameObject);
            }
            blendShapeWeightSave = new BlendShapeWeightSave(gameObject);

            var clip = dg_get_m_SourcePreviewMotion(instance);
            if (clip.legacy || instance == null || !((UnityEngine.Object)animator != (UnityEngine.Object)null))
            {
                if (animation != null)
                    animation.enabled = false;  //If vaw.animation.enabled, it is not updated during execution. bug?
            }
            else
            {
                if (m_Controller == null)
                {
                    m_Controller = new UnityEditor.Animations.AnimatorController();
                    m_Controller.name = "Avatar Preview AnimatorController";
                    m_Controller.hideFlags |= HideFlags.HideAndDontSave;
                    uAnimatorController.SetPushUndo(m_Controller, false);
                    m_Controller.AddLayer("preview");
                    m_StateMachine = m_Controller.layers[0].stateMachine;
                    uAnimatorStateMachine.SetPushUndo(m_StateMachine, false);
                    m_StateMachine.hideFlags |= HideFlags.HideAndDontSave;
                }
                if (m_State == null)
                {
                    m_State = m_StateMachine.AddState("preview");
                    uAnimatorState.SetPushUndo(m_State, false);
                    m_State.motion = (Motion)clip;
                    m_State.iKOnFeet = dg_get_ShowIKOnFeetButton() && dg_get_IKOnFeet();
                    m_State.hideFlags |= HideFlags.HideAndDontSave;
                }
                UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, this.m_Controller);

                animator.fireEvents = false;
                animator.applyRootMotion = EditorPrefs.GetBool(EditorPrefsApplyRootMotion, false);
            }

            dg_set_ShowIKOnFeetButton(animator != null && animator.isHuman && clip.isHumanMotion);
#if !UNITY_2017_3_OR_NEWER
            mode2D = EditorPrefs.GetBool(EditorPrefs2D, false);
#endif
            SetTime(uTimeControl.currentTime);
            ForceUpdate();
        }
        private void DestroyController()
        {
            if (instance != null && animator != null)
                UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, null);
            if (m_Controller != null)
                UnityEditor.Animations.AnimatorController.DestroyImmediate(m_Controller);
            if (m_StateMachine != null)
                AnimatorStateMachine.DestroyImmediate(m_StateMachine);
            if (m_State != null)
                AnimatorState.DestroyImmediate(m_State);
            m_Controller = null;
            m_StateMachine = null;
            m_State = null;
        }

        private void OnCurveWasModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType deleted)
        {
            if (instance == null) return;
            if (clip != dg_get_m_SourcePreviewMotion(instance)) return;

            Reset();
        }

        public void OnPreviewSettings()
        {
            var clip = dg_get_m_SourcePreviewMotion(instance);
#if !UNITY_2017_3_OR_NEWER
            {
                EditorGUI.BeginChangeCheck();
                var flag = GUILayout.Toggle(mode2D, "2D", "preButton");
                if (EditorGUI.EndChangeCheck())
                {
                    mode2D = flag;
                    if (mode2D) PreviewDir = Vector2.zero;
                    else PreviewDir = new Vector2(120f, -20f);
                    EditorPrefs.SetBool(EditorPrefs2D, mode2D);
                }
            }
#endif
            if (!clip.legacy && animator != null && animator.runtimeAnimatorController != null)
            {
                var flag = EditorPrefs.GetBool(EditorPrefsApplyRootMotion, false);
                EditorGUI.BeginChangeCheck();
                flag = GUILayout.Toggle(flag, "Apply Root Motion", "preButton");
                if (EditorGUI.EndChangeCheck())
                {
                    animator.applyRootMotion = flag;
                    EditorPrefs.SetBool(EditorPrefsApplyRootMotion, flag);
                    Reset();
                }
            }
            GUILayout.Space(20);
            dg_DoPreviewSettings();
        }

        public void OnGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type == EventType.Repaint)
            {
                uTimeControl.Update();

#if !UNITY_2017_3_OR_NEWER
                if (mode2D)
                {
                    PreviewDir = Vector2.zero;
                }
#endif

                {
                    var clip = dg_get_m_SourcePreviewMotion(instance);
                    uTimeControl.loop = true;
                    uTimeControl.startTime = 0f;
                    uTimeControl.stopTime = clip.length;
                    dg_set_fps(instance, (int)clip.frameRate);
                    if (!clip.legacy && animator != null && animator.runtimeAnimatorController != null)
                    {
                        dg_set_ShowIKOnFeetButton(animator.isHuman && clip.isHumanMotion);
                        AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                        if (m_State != null)
                            m_State.iKOnFeet = dg_get_ShowIKOnFeetButton() && dg_get_IKOnFeet();

                        var normalizedTime = animationClipSettings.stopTime - animationClipSettings.startTime == 0.0 ? 0.0f : (float)((uTimeControl.currentTime - animationClipSettings.startTime) / (animationClipSettings.stopTime - animationClipSettings.startTime));
                        animator.Play(0, 0, normalizedTime);
                        animator.Update(uTimeControl.deltaTime);
                    }
                    else if (animation != null)
                    {
                        dg_set_ShowIKOnFeetButton(false);
                        clip.SampleAnimation(gameObject, uTimeControl.currentTime);
                    }
                }
            }

            dg_DoAvatarPreview(r, background);

            if (animator.applyRootMotion && transformPoseSave != null)
            {
                var rect = r;
                rect.yMin = rect.yMax - 40f;
                rect.yMax -= 15f;
                var invRot = Quaternion.Inverse(transformPoseSave.originalRotation);
                var pos = invRot * (gameObject.transform.position - transformPoseSave.originalPosition);
                var rot = (invRot * gameObject.transform.rotation).eulerAngles;
                EditorGUI.DropShadowLabel(rect, string.Format("Root Motion Position {0}\nRoot Motion Rotation {1}", pos, rot));
            }
        }

        public void SetTime(float time)
        {
            uTimeControl.currentTime = 0f;
            uTimeControl.nextCurrentTime = time;

            Reset();
        }
        public float GetTime()
        {
            return uTimeControl.currentTime;
        }

        public void Reset()
        {
            {
                var time = uTimeControl.currentTime + (uTimeControl.GetDeltaTimeSet() ? uTimeControl.deltaTime : 0f);
                uTimeControl.currentTime = 0f;
                uTimeControl.nextCurrentTime = time;
            }

            if (transformPoseSave != null)
            {
                transformPoseSave.ResetOriginalTransform();
            }
            if (blendShapeWeightSave != null)
            {
                blendShapeWeightSave.ResetOriginalWeight();
            }
        }

        public void ForceUpdate()
        {
            var clip = dg_get_m_SourcePreviewMotion(instance);
            if (!clip.legacy && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.Update(0f);
            }
            else if (animation != null)
            {
                clip.SampleAnimation(gameObject, uTimeControl.currentTime);
            }
        }
        
        public Vector2 PreviewDir
        {
            get
            {
                return (Vector2)fi_m_PreviewDir.GetValue(instance);
            }
            set
            {
                fi_m_PreviewDir.SetValue(instance, value);
            }
        }

        public float ZoomFactor
        {
            get
            {
                return (float)fi_m_ZoomFactor.GetValue(instance);
            }
            set
            {
                fi_m_ZoomFactor.SetValue(instance, value);
            }
        }

        public bool playing
        {
            get
            {
                return uTimeControl.playing;
            }
            set
            {
                uTimeControl.playing = value;
            }
        }
    }
}
