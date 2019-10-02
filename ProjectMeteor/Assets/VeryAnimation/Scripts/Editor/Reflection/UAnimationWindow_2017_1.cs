#if UNITY_2017_1_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define VERYANIMATION_TIMELINE
#endif

using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

#if VERYANIMATION_TIMELINE
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace VeryAnimation
{
#if UNITY_2017_1_OR_NEWER
    public class UAnimationWindow_2017_1 : UAnimationWindow    //2017.1 or later
    {
        protected class UAnimationWindowState_2017_1 : UAnimationWindowState
        {
            protected MethodInfo mi_StartPreview;
            protected MethodInfo mi_StopPreview;

            private Func<object, bool> dg_get_linkedWithSequencer;
            private Func<bool> dg_get_previewing;
            private Func<bool> dg_get_canPreview;

            public UAnimationWindowState_2017_1(Assembly asmUnityEditor) : base(asmUnityEditor)
            {
                Assert.IsNotNull(mi_StartPreview = animationWindowStateType.GetMethod("StartPreview"));
                Assert.IsNotNull(mi_StopPreview = animationWindowStateType.GetMethod("StopPreview"));
                Assert.IsNotNull(dg_get_linkedWithSequencer = EditorCommon.CreateGetFieldDelegate<bool>(animationWindowStateType.GetField("linkedWithSequencer")));
            }

            public override void StopRecording(object instance)
            {
                if (instance == null) return;
                mi_StopRecording.Invoke(instance, null);
                mi_StopPreview.Invoke(instance, null);
            }
            public void StartPreview(object instance)
            {
                if (instance == null) return;
                mi_StartPreview.Invoke(instance, null);
            }
            public void StopPreview(object instance)
            {
                if (instance == null) return;
                mi_StopPreview.Invoke(instance, null);
            }

            public bool GetLinkedWithSequencer(object instance)
            {
                if (instance == null) return false;
                return dg_get_linkedWithSequencer(instance);
            }
            public bool GetPreviewing(object instance)
            {
                if (instance == null) return false;
                if (dg_get_previewing == null || dg_get_previewing.Target != instance)
                    dg_get_previewing = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("previewing").GetGetMethod());
                return dg_get_previewing();
            }
            public bool GetCanPreview(object instance)
            {
                if (instance == null) return false;
                if (dg_get_canPreview == null || dg_get_canPreview.Target != instance)
                    dg_get_canPreview = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, instance.GetType().GetProperty("canPreview").GetGetMethod());
                return dg_get_canPreview();
            }
        }

        protected UAnimationWindowState_2017_1 uAnimationWindowState_2017_1;

#if VERYANIMATION_TIMELINE
        public UTimelineWindow uTimelineWindow { get; private set; }
        protected object animationTimeWindowControlInstance
        {
            get
            {
                var awc = animationWindowControlInstance;
                if (awc != null && awc.GetType() == uTimelineWindow.uTimelineWindowTimeControl.timelineWindowTimeControlType)
                    return awc;
                return null;
            }
        }
#endif

        protected MethodInfo mi_EditSequencerClip;
        protected MethodInfo mi_UnlinkSequencer;

        public UAnimationWindow_2017_1()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var animationWindowType = asmUnityEditor.GetType("UnityEditor.AnimationWindow");
            Assert.IsNotNull(mi_EditSequencerClip = animationWindowType.GetMethod("EditSequencerClip", BindingFlags.Public | BindingFlags.Instance));
            Assert.IsNotNull(mi_UnlinkSequencer = animationWindowType.GetMethod("UnlinkSequencer", BindingFlags.Public | BindingFlags.Instance));

            uAnimationWindowState = uAnimationWindowState_2017_1 = new UAnimationWindowState_2017_1(asmUnityEditor);
#if VERYANIMATION_TIMELINE
#if UNITY_2018_2_OR_NEWER
            uTimelineWindow = new UTimelineWindow_2018_2();
#else
            uTimelineWindow = new UTimelineWindow();
#endif
#endif
        }

        public override GameObject GetActiveRootGameObject()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
#if VERYANIMATION_TIMELINE
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var bindingObject = uTimelineWindow.uTimelineWindowTimeControl.GetGenericBinding(atwc);
                    if (bindingObject != null)
                    {
                        if (bindingObject is GameObject)
                        {
                            return bindingObject as GameObject;
                        }
                        else if (bindingObject is Animator)
                        {
                            var animator = bindingObject as Animator;
                            return animator.gameObject;
                        }
                    }
                }
#endif
                return null;
            }
            else
            {
                return uAnimationWindowState.GetActiveRootGameObject(aws);
            }
        }
        public override Component GetActiveAnimationPlayer()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
#if VERYANIMATION_TIMELINE
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var bindingObject = uTimelineWindow.uTimelineWindowTimeControl.GetGenericBinding(atwc);
                    if (bindingObject != null)
                    {
                        if (bindingObject is GameObject)
                        {
                            var gameObject = bindingObject as GameObject;
                            return gameObject.GetComponent<Animator>();
                        }
                        else if (bindingObject is Animator)
                        {
                            return bindingObject as Animator;
                        }
                    }
                }
#endif
                return null;
            }
            else
            {
                return uAnimationWindowState.GetActiveAnimationPlayer(aws);
            }
        }

        public override void RecordingDisable()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetRecording(aws))
            {
                uAnimationWindowState_2017_1.StopRecording(aws);
            }
            else if (uAnimationWindowState_2017_1.GetPreviewing(aws))
            {
                uAnimationWindowState_2017_1.StopPreview(aws);
            }
        }

        public void PreviewingChange()
        {
            var aws = animationWindowStateInstance;
            var previewing = uAnimationWindowState_2017_1.GetPreviewing(aws);

            previewing = !previewing;
            if (previewing)
                uAnimationWindowState_2017_1.StartPreview(aws);
            else
            {
                uAnimationWindowState_2017_1.StopPreview(aws);
            }
        }
        public bool GetCanPreview()
        {
            return uAnimationWindowState_2017_1.GetCanPreview(animationWindowStateInstance);
        }
        public bool GetPreviewing()
        {
            return uAnimationWindowState_2017_1.GetPreviewing(animationWindowStateInstance);
        }

        public override void MoveToNextFrame()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
#if VERYANIMATION_TIMELINE
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var state = uTimelineWindow.uTimelineWindowTimeControl.GetTimelineState(atwc);
                    var frame = uTimelineWindow.uTimelineState.GetFrame(state);
                    uTimelineWindow.uTimelineState.SetFrame(state, ++frame);
                    Repaint();
                }
#endif
            }
            else
            {
                base.MoveToNextFrame();
            }
        }
        public override void MoveToPrevFrame()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
#if VERYANIMATION_TIMELINE
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var state = uTimelineWindow.uTimelineWindowTimeControl.GetTimelineState(atwc);
                    var frame = uTimelineWindow.uTimelineState.GetFrame(state);
                    uTimelineWindow.uTimelineState.SetFrame(state, --frame);
                    Repaint();
                }
#endif
            }
            else
            {
                base.MoveToPrevFrame();
            }
        }

#if VERYANIMATION_TIMELINE
        public bool GetLinkedWithTimeline()
        {
            return uAnimationWindowState_2017_1.GetLinkedWithSequencer(animationWindowStateInstance);
        }
        public bool GetLinkedWithTimelineEditable()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var trackAsset = uTimelineWindow.uTimelineWindowTimeControl.GetTrackAsset(atwc);
                    if (trackAsset != null && !trackAsset.muted)
                    {
                        var locked = uTimelineWindow.uTrackAsset.GetLocked(trackAsset);
                        if (!locked)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool GetTimelineRecording()
        {
            return uTimelineWindow.GetRecording();
        }
        public void SetTimelineRecording(bool enable)
        {
            uTimelineWindow.SetRecording(enable);
        }

        public bool GetTimelinePreviewMode()
        {
            return uTimelineWindow.GetPreviewMode();
        }
        public void SetTimelinePreviewMode(bool enable)
        {
            uTimelineWindow.SetPreviewMode(enable);
        }

        public AnimationClip GetTimelineAnimationClip()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    return uTimelineWindow.uTimelineWindowTimeControl.GetAnimationClip(atwc);
                }
            }
            return null;
        }
        public void SetTimelineAnimationClip(AnimationClip clip, string undoName = null)
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    uTimelineWindow.uTimelineWindowTimeControl.SetAnimationClip(atwc, clip, undoName);
                }
            }
        }

        public virtual void GetRootMotionOffsets(Vector3 startPosition, Quaternion startRotation, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
#if !UNITY_2018_3_OR_NEWER
            //Track Offsets
            {
                var animtionTrack = GetAnimationTrack();
                if (animtionTrack != null && animtionTrack.applyOffsets)
                {
                    position = animtionTrack.position;
                    rotation = animtionTrack.rotation;
                }
            }
            //Clip Offsets
            {
                var animationPlayableAsset = GetAnimationPlayableAsset();
                if (animationPlayableAsset != null)
                {
                    position += rotation * animationPlayableAsset.position;
                    rotation *= animationPlayableAsset.rotation;
                }
            }
#endif
        }

        public float GetTimelineFrameRate()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    var state = uTimelineWindow.uTimelineWindowTimeControl.GetTimelineState(atwc);
                    return uTimelineWindow.uTimelineState.GetFrameRate(state);
                }
            }
            return 0f;
        }

        public bool IsTimelineArmedForRecord()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var awc = animationWindowControlInstance;
                if (awc != null && awc.GetType() == uTimelineWindow.uTimelineWindowTimeControl.timelineWindowTimeControlType)
                {
                    return uTimelineWindow.uTimelineWindowTimeControl.IsArmedForRecord(awc);
                }
            }
            return false;
        }

        public bool EditSequencerClip(TimelineClip timelineClip)
        {
            var sourceObject = GetActiveRootGameObject();
            object controlInterface = uTimelineWindow.uTimelineAnimationUtilities.CreateTimeController(uTimelineWindow.state, timelineClip);
            return (bool)mi_EditSequencerClip.Invoke(instance, new object[] { timelineClip.animationClip != null ? timelineClip.animationClip : timelineClip.curves, sourceObject, controlInterface });
        }
        public void UnlinkSequencer()
        {
            mi_UnlinkSequencer.Invoke(instance, null);
        }

        public PlayableDirector GetTimelineCurrentDirector()
        {
            return uTimelineWindow.GetCurrentDirector();
        }

        public AnimationTrack GetAnimationTrack()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    return uTimelineWindow.uTimelineWindowTimeControl.GetTrackAsset(atwc) as AnimationTrack;
                }
            }
            return null;
        }
        public TimelineClip GetTimelineClip()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    return uTimelineWindow.uTimelineWindowTimeControl.GetTimelineClip(atwc);
                }
            }
            return null;
        }
        public AnimationPlayableAsset GetAnimationPlayableAsset()
        {
            var aws = animationWindowStateInstance;
            if (uAnimationWindowState_2017_1.GetLinkedWithSequencer(aws))
            {
                var atwc = animationTimeWindowControlInstance;
                if (atwc != null)
                {
                    return uTimelineWindow.uTimelineWindowTimeControl.GetPlayableAsset(atwc) as AnimationPlayableAsset;
                }
            }
            return null;
        }
#endif
    }
#endif
}
