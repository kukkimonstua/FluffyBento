using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Linq;
using System.Collections.Generic;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace VeryAnimation
{
    public class SynchronizeAnimation
    {
        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }

        private class SynchronizeObject
        {
            public GameObject gameObject;
            public Animator animator;
            public Animation animation;
            public AnimationClip clip;
            public PropertyModification[] propertyModifications;
            public TransformPoseSave transformPoseSave;
            public BlendShapeWeightSave blendShapeWeightSave;

            public SynchronizeObject(Animator animator)
            {
                this.animator = animator;
                gameObject = animator.gameObject;
                propertyModifications = PrefabUtility.GetPropertyModifications(gameObject);
                transformPoseSave = new TransformPoseSave(gameObject);
                blendShapeWeightSave = new BlendShapeWeightSave(gameObject);
                #region Clip
                {
                    var saveSettings = gameObject.GetComponent<VeryAnimationSaveSettings>();
                    if (saveSettings != null && saveSettings.lastSelectAnimationClip != null)
                    {
                        if (ArrayUtility.Contains(AnimationUtility.GetAnimationClips(gameObject), saveSettings.lastSelectAnimationClip))
                            clip = saveSettings.lastSelectAnimationClip;
                    }
                    if (clip == null)
                    {
                        var ac = EditorCommon.GetAnimatorController(animator);
                        if (ac != null && ac.layers.Length > 0)
                        {
                            var state = ac.layers[0].stateMachine.defaultState;
                            if (state != null)
                            {
                                if (state.motion is UnityEditor.Animations.BlendTree)
                                {
                                    Action<UnityEditor.Animations.BlendTree> FindBlendTree = null;
                                    FindBlendTree = (blendTree) =>
                                    {
                                        if (blendTree.children == null) return;
                                        var children = blendTree.children;
                                        for (int i = 0; i < children.Length; i++)
                                        {
                                            if (children[i].motion is UnityEditor.Animations.BlendTree)
                                            {
                                                FindBlendTree(children[i].motion as UnityEditor.Animations.BlendTree);
                                            }
                                            else
                                            {
                                                clip = children[i].motion as AnimationClip;
                                            }
                                            if (clip != null) break;
                                        }
                                        blendTree.children = children;
                                    };
                                    FindBlendTree(state.motion as UnityEditor.Animations.BlendTree);
                                }
                                else
                                {
                                    clip = state.motion as AnimationClip;
                                }
                            }
                        }
                        if (clip != null)
                        {
                            var owc = animator.runtimeAnimatorController as AnimatorOverrideController;
                            if (owc != null)
                            {
                                clip = owc[clip];
                            }
                        }
                    }
                }
                #endregion
                animator.enabled = false;   //In order to avoid the mysterious behavior where an event is called from "UnityEditor.Handles: DrawCameraImpl", it is invalid except when updating
            }
            public SynchronizeObject(Animation animation)
            {
                this.animation = animation;
                gameObject = animation.gameObject;
                propertyModifications = PrefabUtility.GetPropertyModifications(gameObject);
                transformPoseSave = new TransformPoseSave(gameObject);
                blendShapeWeightSave = new BlendShapeWeightSave(gameObject);
                #region Clip
                {
                    var saveSettings = gameObject.GetComponent<VeryAnimationSaveSettings>();
                    if (saveSettings != null && saveSettings.lastSelectAnimationClip != null)
                    {
                        if (ArrayUtility.Contains(AnimationUtility.GetAnimationClips(gameObject), saveSettings.lastSelectAnimationClip))
                            clip = saveSettings.lastSelectAnimationClip;
                    }
                    if (clip == null)
                        clip = animation.clip;
                }
                #endregion
            }
        }
        private List<SynchronizeObject> synchronizeObjects;
        private float currentTime;
        private bool enable;

        public SynchronizeAnimation()
        {
            synchronizeObjects = new List<SynchronizeObject>();

            Action<GameObject> AddGameObject = (go) =>
            {
                foreach (var animator in go.GetComponentsInChildren<Animator>(true))
                {
                    if (animator == null || animator == vaw.animator)
                        continue;
                    if (!animator.gameObject.activeInHierarchy || !animator.enabled || animator.runtimeAnimatorController == null)
                        continue;
                    synchronizeObjects.Add(new SynchronizeObject(animator));
                }
                foreach (var animation in go.GetComponentsInChildren<Animation>(true))
                {
                    if (animation == null || animation == vaw.animation)
                        continue;
                    if (!animation.gameObject.activeInHierarchy || !animation.enabled)
                        continue;
                    synchronizeObjects.Add(new SynchronizeObject(animation));
                }
            };

#if UNITY_2018_3_OR_NEWER
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                var scene = PrefabStageUtility.GetCurrentPrefabStage().scene;
                foreach (var go in scene.GetRootGameObjects())
                    AddGameObject(go);
            }
            else
#endif
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    foreach (var go in scene.GetRootGameObjects())
                        AddGameObject(go);
                }
            }
            SetEnable(true);
        }
        ~SynchronizeAnimation()
        {
            Assert.IsNull(synchronizeObjects);
        }

        public void Release()
        {
            SetEnable(false);
            foreach (var data in synchronizeObjects)
            {
                if (data.animator != null)
                    data.animator.enabled = true;
                if (data.propertyModifications != null)
                {
                    PrefabUtility.SetPropertyModifications(data.gameObject, data.propertyModifications);
                }
                else
                {
#if UNITY_2018_3_OR_NEWER
                    if (data.animator != null && data.animator.isInitialized)
                        data.animator.WriteDefaultValues();
#endif
                }
                //In the above "SetPropertyModifications" somehow become the default pose of Humanoid, return to the original information later than not before
                data.transformPoseSave.ResetOriginalTransform();
                data.blendShapeWeightSave.ResetOriginalWeight();
            }
            synchronizeObjects = null;
        }

        public void SetEnable(bool enable)
        {
            foreach (var data in synchronizeObjects)
            {
                data.transformPoseSave.ResetOriginalTransform();
                data.blendShapeWeightSave.ResetOriginalWeight();
                {//RendererForceUpdate
                    data.gameObject.SetActive(false);
                    data.gameObject.SetActive(true);
                }
            }
            currentTime = 0f;
            this.enable = enable;
        }
        public void SetTime(float time)
        {
            if (!enable)
                return;
            currentTime = time;
            foreach (var data in synchronizeObjects)
            {
                SampleAnimation(data);
            }
        }
        private void SampleAnimation(SynchronizeObject data)
        {
            if (data == null || data.clip == null)
                return;
            data.transformPoseSave.ResetOriginalTransform();
            data.blendShapeWeightSave.ResetOriginalWeight();
            if (data.animator != null)
            {
                #region Animator
#if UNITY_2018_3_OR_NEWER
                var changedRootMotion = data.animator.applyRootMotion;
                if (changedRootMotion)
                {
                    data.animator.applyRootMotion = false;
                }
#endif
                data.animator.enabled = true;   //In order to avoid the mysterious behavior where an event is called from "UnityEditor.Handles: DrawCameraImpl", it is invalid except when updating

                if (!data.animator.isInitialized)
                    data.animator.Rebind();
                data.clip.SampleAnimation(data.gameObject, currentTime);

                data.animator.enabled = false;  //In order to avoid the mysterious behavior where an event is called from "UnityEditor.Handles: DrawCameraImpl", it is invalid except when updating

#if UNITY_2018_3_OR_NEWER
                if (changedRootMotion)
                {
                    data.animator.applyRootMotion = true;
                }
#endif
                #endregion
            }
            else if (data.animation != null)
            {
                #region Animation
                data.clip.SampleAnimation(data.gameObject, currentTime);
                #endregion
            }
        }

        public void UpdateSameClip(AnimationClip clip)
        {
            if (!enable)
                return;
            foreach (var data in synchronizeObjects)
            {
                if (data == null || data.clip != clip)
                    continue;
                SampleAnimation(data);
            }
        }
    }
}
