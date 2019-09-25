using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Assertions;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace VeryAnimation
{
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Animator))]
#else
    [ExecuteInEditMode, DisallowMultipleComponent, RequireComponent(typeof(Animator))]
#endif
    public class VeryAnimationEditAnimator : MonoBehaviour
    {
#if !UNITY_EDITOR
        private void Awake()
        {
            Destroy(this);
        }
#else
        public Action<int> onAnimatorIK;
        public int stateNameHash { get; private set; }

        private bool changed;
        private RuntimeAnimatorController originalRuntimeAnimatorController;
        private bool originalAnimatorApplyRootMotion;
        private AnimatorUpdateMode originalAnimatorUpdateMode;
        private AnimatorCullingMode originalAnimatorCullingMode;
        private UnityEditor.Animations.AnimatorController tmpAnimatorController;
        private Animator animatorCache;
        private AnimatorControllerLayer[] layers;
        private AnimatorState state;

        private void Awake()
        {
            animatorCache = GetComponent<Animator>();
            Change(true);
        }
        private void OnDestroy()
        {
            Change(false);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (onAnimatorIK != null)
                onAnimatorIK.Invoke(layerIndex);
        }

        private void Change(bool change)
        {
            var t = gameObject.transform;
            var localPosition = t.localPosition;
            var localRotation = t.localRotation;
            var localScale = t.localScale;

            Undo.RecordObject(this, "Change Animator Controller");
            Undo.RecordObject(animatorCache, "Change Animator Controller");
            if (change)
            {
                Assert.IsFalse(changed);
                animatorCache.hideFlags |= HideFlags.NotEditable;

                originalAnimatorApplyRootMotion = animatorCache.applyRootMotion;
                originalAnimatorUpdateMode = animatorCache.updateMode;
                originalAnimatorCullingMode = animatorCache.cullingMode;

                animatorCache.applyRootMotion = false;
                animatorCache.updateMode = AnimatorUpdateMode.Normal;
                animatorCache.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                #region AnimatorController
                {
                    originalRuntimeAnimatorController = animatorCache.runtimeAnimatorController;
                    tmpAnimatorController = new UnityEditor.Animations.AnimatorController();
                    tmpAnimatorController.name = "Very Animation Temporary Controller";
                    tmpAnimatorController.hideFlags |= HideFlags.HideAndDontSave;
                    {
                        tmpAnimatorController.AddLayer("Very Animation Layer");
                        layers = tmpAnimatorController.layers;
                        foreach (var layer in layers)
                        {
                            layer.iKPass = true;
                            var stateMachine = layer.stateMachine;
                            stateMachine.hideFlags |= HideFlags.HideAndDontSave;
                            {
                                state = stateMachine.AddState("Animation");
                                state.hideFlags |= HideFlags.HideAndDontSave;
                                stateNameHash = state.nameHash;
                            }
                            layer.stateMachine = stateMachine;
                        }
                        tmpAnimatorController.layers = layers;
                    }
                    UnityEditor.Animations.AnimatorController.SetAnimatorController(animatorCache, tmpAnimatorController);
                }
                #endregion
                changed = true;
            }
            else
            {
                Assert.IsTrue(changed);
                animatorCache.hideFlags &= ~HideFlags.NotEditable;

                animatorCache.applyRootMotion = originalAnimatorApplyRootMotion;
                animatorCache.updateMode = originalAnimatorUpdateMode;
                animatorCache.cullingMode = originalAnimatorCullingMode;

                #region AnimatorController
                {
                    {
                        var layerCount = tmpAnimatorController.layers.Length;
                        for (int i = 0; i < layerCount; i++)
                            tmpAnimatorController.RemoveLayer(0);
                    }
                    DestroyImmediate(tmpAnimatorController);
                    tmpAnimatorController = null;
                    animatorCache.runtimeAnimatorController = originalRuntimeAnimatorController;
                }
                #endregion

                originalRuntimeAnimatorController = null;

                changed = false;
            }

            //Cause unknown. It does not allow initialization.
            {
                t.localPosition = localPosition;
                t.localRotation = localRotation;
                t.localScale = localScale;
            }
        }

        public void SetAnimationClip(AnimationClip clip)
        {
            if (tmpAnimatorController == null || state == null) return;
            if (state.motion == clip) return;
            state.motion = clip;
        }

        public void SetIKPass(bool enable)
        {
            if (tmpAnimatorController == null) return;
            if (layers[0].iKPass == enable) return;
            layers = tmpAnimatorController.layers;
            foreach (var layer in layers)
            {
                layer.iKPass = enable;
            }
            tmpAnimatorController.layers = layers;
        }
#endif
    }
}
