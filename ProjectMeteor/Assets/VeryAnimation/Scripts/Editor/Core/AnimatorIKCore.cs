using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace VeryAnimation
{
    [Serializable]
    public class AnimatorIKCore
    {
        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }

        public enum IKTarget
        {
            None = -1,
            Head,
            LeftHand,
            RightHand,
            LeftFoot,
            RightFoot,
            Total,
        }
        public static readonly string[] IKTargetStrings =
        {
            "Head",
            "Left Hand",
            "Right Hand",
            "Left Foot",
            "Right Foot",
        };
        private static readonly IKTarget[] IKTargetMirror =
        {
            IKTarget.None,
            IKTarget.RightHand,
            IKTarget.LeftHand,
            IKTarget.RightFoot,
            IKTarget.LeftFoot,
        };
        private readonly Quaternion[] IKTargetSyncRotation =
        {
            Quaternion.identity,
            Quaternion.Euler(0, 90, 180),
            Quaternion.Euler(0, 90, 0),
            Quaternion.Euler(90, 0, 90),
            Quaternion.Euler(90, 0, 90),
        };

        public static readonly IKTarget[] HumanBonesUpdateAnimatorIK =
        {
            IKTarget.Total, //Hips = 0,
            IKTarget.LeftFoot, //LeftUpperLeg = 1,
            IKTarget.RightFoot, //RightUpperLeg = 2,
            IKTarget.LeftFoot, //LeftLowerLeg = 3,
            IKTarget.RightFoot, //RightLowerLeg = 4,
            IKTarget.LeftFoot, //LeftFoot = 5,
            IKTarget.RightFoot, //RightFoot = 6,
            IKTarget.Total, //Spine = 7,
            IKTarget.Total, //Chest = 8,
            IKTarget.Head, //Neck = 9,
            IKTarget.Head, //Head = 10,
            IKTarget.LeftHand, //LeftShoulder = 11,
            IKTarget.RightHand, //RightShoulder = 12,
            IKTarget.LeftHand, //LeftUpperArm = 13,
            IKTarget.RightHand, //RightUpperArm = 14,
            IKTarget.LeftHand, //LeftLowerArm = 15,
            IKTarget.RightHand, //RightLowerArm = 16,
            IKTarget.LeftHand, //LeftHand = 17,
            IKTarget.RightHand, //RightHand = 18,
            IKTarget.None, //LeftToes = 19,
            IKTarget.None, //RightToes = 20,
            IKTarget.Head, //LeftEye = 21,
            IKTarget.Head, //RightEye = 22,
            IKTarget.None, //Jaw = 23,
            IKTarget.None, //LeftThumbProximal = 24,
            IKTarget.None, //LeftThumbIntermediate = 25,
            IKTarget.None, //LeftThumbDistal = 26,
            IKTarget.None, //LeftIndexProximal = 27,
            IKTarget.None, //LeftIndexIntermediate = 28,
            IKTarget.None, //LeftIndexDistal = 29,
            IKTarget.None, //LeftMiddleProximal = 30,
            IKTarget.None, //LeftMiddleIntermediate = 31,
            IKTarget.None, //LeftMiddleDistal = 32,
            IKTarget.None, //LeftRingProximal = 33,
            IKTarget.None, //LeftRingIntermediate = 34,
            IKTarget.None, //LeftRingDistal = 35,
            IKTarget.None, //LeftLittleProximal = 36,
            IKTarget.None, //LeftLittleIntermediate = 37,
            IKTarget.None, //LeftLittleDistal = 38,
            IKTarget.None, //RightThumbProximal = 39,
            IKTarget.None, //RightThumbIntermediate = 40,
            IKTarget.None, //RightThumbDistal = 41,
            IKTarget.None, //RightIndexProximal = 42,
            IKTarget.None, //RightIndexIntermediate = 43,
            IKTarget.None, //RightIndexDistal = 44,
            IKTarget.None, //RightMiddleProximal = 45,
            IKTarget.None, //RightMiddleIntermediate = 46,
            IKTarget.None, //RightMiddleDistal = 47,
            IKTarget.None, //RightRingProximal = 48,
            IKTarget.None, //RightRingIntermediate = 49,
            IKTarget.None, //RightRingDistal = 50,
            IKTarget.None, //RightLittleProximal = 51,
            IKTarget.None, //RightLittleIntermediate = 52,
            IKTarget.None, //RightLittleDistal = 53,
            IKTarget.Total, //UpperChest = 54,
        };

        public GUIContent[] IKSpaceTypeStrings = new GUIContent[(int)AnimatorIKData.SpaceType.Total];

        [Serializable]
        public class AnimatorIKData
        {
            private VeryAnimation va { get { return VeryAnimation.instance; } }

            public enum SpaceType
            {
                Global,
                Local,
                Parent,
                Total
            }

            public bool enable;
            public bool autoRotation;
            public SpaceType spaceType;
            public GameObject parent;
            public Vector3 position;
            public Quaternion rotation;
            //Head
            public float headWeight = 1f;
            public float eyesWeight = 0f;
            //Swivel
            public float swivelRotation;
            public Vector3 swivelPosition;

            public Vector3 worldPosition
            {
                get
                {
                    var getpos = Vector3.zero;
                    switch (spaceType)
                    {
                    case SpaceType.Global: getpos = position; break;
                    case SpaceType.Local: getpos = root != null && root.transform.parent != null ? root.transform.parent.localToWorldMatrix.MultiplyPoint3x4(position) : position; break;
                    case SpaceType.Parent: getpos = parent != null ? parent.transform.localToWorldMatrix.MultiplyPoint3x4(position) : position; break;
                    default: Assert.IsTrue(false); break;
                    }
                    if (va.dummyObject != null && spaceType == SpaceType.Parent)
                    {
                        var rootBoneIndex = va.EditBonesIndexOf(root);
                        getpos = va.bones[rootBoneIndex].transform.worldToLocalMatrix.MultiplyPoint3x4(getpos);
                        getpos = va.editBones[rootBoneIndex].transform.localToWorldMatrix.MultiplyPoint(getpos);
                    }
                    return getpos;
                }
                set
                {
                    var setpos = value;
                    if (va.dummyObject != null && spaceType == SpaceType.Parent)
                    {
                        var rootBoneIndex = va.EditBonesIndexOf(root);
                        setpos = va.editBones[rootBoneIndex].transform.worldToLocalMatrix.MultiplyPoint(setpos);
                        setpos = va.bones[rootBoneIndex].transform.localToWorldMatrix.MultiplyPoint3x4(setpos);
                    }
                    switch (spaceType)
                    {
                    case SpaceType.Global: position = setpos; break;
                    case SpaceType.Local: position = root != null && root.transform.parent != null ? root.transform.parent.worldToLocalMatrix.MultiplyPoint3x4(setpos) : setpos; break;
                    case SpaceType.Parent: position = parent != null ? parent.transform.worldToLocalMatrix.MultiplyPoint3x4(setpos) : setpos; break;
                    default: Assert.IsTrue(false); break;
                    }
                }
            }
            public Quaternion worldRotation
            {
                get
                {
                    var getrot = Quaternion.identity;
                    switch (spaceType)
                    {
                    case SpaceType.Global: getrot = rotation; break;
                    case SpaceType.Local: getrot = root != null && root.transform.parent != null ? root.transform.parent.rotation * rotation : rotation; break;
                    case SpaceType.Parent: getrot = parent != null ? parent.transform.rotation * rotation : rotation; break;
                    default: Assert.IsTrue(false); getrot = rotation; break;
                    }
                    if (va.dummyObject != null && spaceType == SpaceType.Parent)
                    {
                        var rootBoneIndex = va.EditBonesIndexOf(root);
                        getrot = Quaternion.Inverse(va.bones[rootBoneIndex].transform.rotation) * getrot;
                        getrot = va.editBones[rootBoneIndex].transform.rotation * getrot;
                    }
                    return getrot;
                }
                set
                {
                    var setrot = value;
                    if (va.dummyObject != null && spaceType == SpaceType.Parent)
                    {
                        var rootBoneIndex = va.EditBonesIndexOf(root);
                        setrot = Quaternion.Inverse(va.editBones[rootBoneIndex].transform.rotation) * setrot;
                        setrot = va.bones[rootBoneIndex].transform.rotation * setrot;
                    }
                    switch (spaceType)
                    {
                    case SpaceType.Global: rotation = setrot; break;
                    case SpaceType.Local: rotation = root != null && root.transform.parent != null ? Quaternion.Inverse(root.transform.parent.rotation) * setrot : setrot; break;
                    case SpaceType.Parent: rotation = parent != null ? Quaternion.Inverse(parent.transform.rotation) * setrot : setrot; break;
                    default: Assert.IsTrue(false); break;
                    }
                }
            }

            public bool isUpdate { get { return enable && updateIKtarget && !synchroIKtarget; } }

            [NonSerialized]
            public GameObject root;
            [NonSerialized]
            public bool updateIKtarget;
            [NonSerialized]
            public bool synchroIKtarget;
            [NonSerialized]
            public Vector3 optionPosition;
            [NonSerialized]
            public Quaternion optionRotation;
            [NonSerialized]
            public int parentBoneIndex;
        }
        public AnimatorIKData[] ikData;

        public IKTarget[] ikTargetSelect;
        public IKTarget ikActiveTarget { get { return ikTargetSelect != null && ikTargetSelect.Length > 0 ? ikTargetSelect[0] : IKTarget.None; } }

        private AnimationCurve[] rootTCurves;
        private AnimationCurve[] rootQCurves;
        private AnimationCurve[] muscleCurves;
        private List<int> muscleCurvesUpdated;

        private int[] neckMuscleIndexes;
        private int[] headMuscleIndexes;

        private UDisc uDisc;
        private USnapSettings uSnapSettings;

        private ReorderableList ikReorderableList;
        private bool advancedFoldout;

        private float ikSwivelWeight;
        private Quaternion ikSaveWorldToLocalRotation;
        private Matrix4x4 ikSaveWorldToLocalMatrix;
        private Vector3 ikSaveRootT;
        private Quaternion ikSaveRootQ;

        public Avatar avatarClone { get; private set; }

        public void Initialize()
        {
            Release();

            ikData = new AnimatorIKData[(int)IKTarget.Total];
            for (int i = 0; i < ikData.Length; i++)
            {
                ikData[i] = new AnimatorIKData();
            }
            ikTargetSelect = null;
            
            rootTCurves = new AnimationCurve[3];
            rootQCurves = new AnimationCurve[4];
            muscleCurves = new AnimationCurve[HumanTrait.MuscleCount];
            muscleCurvesUpdated = new List<int>();
            
            neckMuscleIndexes = new int[3];
            for (int i = 0; i < neckMuscleIndexes.Length; i++)
                neckMuscleIndexes[i] = HumanTrait.MuscleFromBone((int)HumanBodyBones.Neck, i);
            headMuscleIndexes = new int[3];
            for (int i = 0; i < headMuscleIndexes.Length; i++)
                headMuscleIndexes[i] = HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, i);

            uDisc = new UDisc();
            uSnapSettings = new USnapSettings();

            UpdateReorderableList();

            UpdateGUIContentStrings();
            Language.OnLanguageChanged += UpdateGUIContentStrings;
        }
        public void Release()
        {
            Language.OnLanguageChanged -= UpdateGUIContentStrings;

            if (avatarClone != null)
            {
                Avatar.DestroyImmediate(avatarClone);
                avatarClone = null;
            }

            ikData = null;
            ikTargetSelect = null;
            rootTCurves = null;
            rootQCurves = null;
            muscleCurves =null;
            muscleCurvesUpdated = null;
            neckMuscleIndexes = null;
            headMuscleIndexes = null;
            uDisc = null;
            uSnapSettings = null;
            ikReorderableList = null;
        }

        public void LoadIKSaveSettings(VeryAnimationSaveSettings saveSettings)
        {
            if (saveSettings == null) return;
            if (va.isHuman)
            {
                if (saveSettings.animatorIkData != null && saveSettings.animatorIkData.Length == ikData.Length)
                {
                    for (int i = 0; i < saveSettings.animatorIkData.Length; i++)
                    {
                        var src = saveSettings.animatorIkData[i];
                        var dst = ikData[i];
                        dst.enable = src.enable;
                        dst.autoRotation = src.autoRotation;
                        dst.spaceType = (AnimatorIKData.SpaceType)src.spaceType;
                        dst.parent = src.parent;
                        dst.position = src.position;
                        dst.rotation = src.rotation;
                        dst.headWeight = src.headWeight;
                        dst.eyesWeight = src.eyesWeight;
                        dst.swivelRotation = src.swivelRotation;
                        dst.swivelPosition = src.swivelPosition;
                    }
                }
            }
        }
        public void SaveIKSaveSettings(VeryAnimationSaveSettings saveSettings)
        {
            if (va.isHuman)
            {
                List<VeryAnimationSaveSettings.AnimatorIKData> saveIkData = new List<VeryAnimationSaveSettings.AnimatorIKData>();
                if (ikData != null)
                {
                    foreach (var d in ikData)
                    {
                        saveIkData.Add(new VeryAnimationSaveSettings.AnimatorIKData()
                        {
                            enable = d.enable,
                            autoRotation = d.autoRotation,
                            spaceType = (int)d.spaceType,
                            parent = d.parent,
                            position = d.position,
                            rotation = d.rotation,
                            headWeight = d.headWeight,
                            eyesWeight = d.eyesWeight,
                            swivelRotation = d.swivelRotation,
                            swivelPosition = d.swivelPosition,
                        });
                    }
                }
                saveSettings.animatorIkData = saveIkData.ToArray();
            }
            else
            {
                saveSettings.animatorIkData = null;
            }
        }

        private void UpdateReorderableList()
        {
            ikReorderableList = null;
            if (ikData == null) return;
            ikReorderableList = new ReorderableList(ikData, typeof(AnimatorIKData), false, true, false, false);
            ikReorderableList.elementHeight = 20;
            ikReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                float x = rect.x;
                {
                    const float Rate = 0.2f;
                    var r = rect;
                    r.x = x;
                    r.y -= 1;
                    r.width = rect.width * Rate;
                    x += r.width;

                    bool flag = true;
                    foreach (var data in ikData)
                    {
                        if (!data.enable)
                        {
                            flag = false;
                            break;
                        }
                    }
                    EditorGUI.BeginChangeCheck();
                    flag = GUI.Toggle(r, flag, Language.GetContent(Language.Help.AnimatorIKAll), EditorStyles.toolbarButton);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change Animator IK Data");
                        for (int target = 0; target < ikData.Length; target++)
                        {
                            ikData[target].enable = flag;
                            SynchroSet((IKTarget)target);
                        }
                    }
                }
            };
            ikReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= ikData.Length)
                    return;

                float x = rect.x;
                {
                    var r = rect;
                    r.x = x;
                    r.y += 2;
                    r.height -= 4;
                    r.width = 16;
                    rect.xMin += r.width;
                    x = rect.x;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.Toggle(r, ikData[index].enable);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ChangeTargetIK((IKTarget)index);
                    }
                }

                EditorGUI.BeginDisabledGroup(!ikData[index].enable);

                {
                    const float Rate = 1f;
                    var r = rect;
                    r.x = x + 2;
                    r.y += 2;
                    r.height -= 4;
                    r.width = rect.width * Rate;
                    x += r.width;
                    r.width -= 4;
                    GUI.Label(r, IKTargetStrings[index]);
                }

                if (!IsValid((IKTarget)index))
                {
                    var tex = vaw.uEditorGUIUtility.GetHelpIcon(MessageType.Warning);
                    var r = rect;
                    r.width = tex.width;
                    r.x = rect.xMax - r.width - 80;
                    GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                }

                {
                    var r = rect;
                    r.width = 60f;
                    r.x = rect.xMax - r.width - 14;
                    EditorGUI.LabelField(r, IKSpaceTypeStrings[(int)ikData[index].spaceType], vaw.guiStyleMiddleRightGreyMiniLabel);
                }

                EditorGUI.EndDisabledGroup();

                if (ikReorderableList.index == index && (IKTarget)index == IKTarget.Head)
                {
                    var r = rect;
                    r.y += 2;
                    r.height -= 2;
                    r.width = 12;
                    r.x = rect.xMax - r.width;
                    advancedFoldout = EditorGUI.Foldout(r, advancedFoldout, new GUIContent("", "Advanced"), true);
                }
            };
            ikReorderableList.onChangedCallback = (ReorderableList list) =>
            {
                Undo.RecordObject(vaw, "Change Animator IK Data");
                ikTargetSelect = null;
                vaw.SetRepaintGUI(VeryAnimationWindow.RepaintGUI.All);
            };
            ikReorderableList.onSelectCallback = (ReorderableList list) =>
            {
                if (list.index >= 0 && list.index < ikData.Length)
                {
                    if (ikData[list.index].enable)
                        va.SelectAnimatorIKTargetPlusKey((IKTarget)list.index);
                    else
                    {
                        var index = list.index;
                        var humanoidIndex = GetEndHumanoidIndex((IKTarget)list.index);
                        va.SelectGameObject(va.humanoidBones[(int)humanoidIndex]);
                        list.index = index;
                    }
                }
            };
        }

        private void UpdateGUIContentStrings()
        {
            for (int i = 0; i < (int)AnimatorIKData.SpaceType.Total; i++)
            {
                IKSpaceTypeStrings[i] = new GUIContent(Language.GetContent(Language.Help.SelectionAnimatorIKSpaceTypeGlobal + i));
            }
        }

        public void UpdateBones()
        {
            if (!va.isHuman)
                return;

            #region Non-Stretch Avatar
            if (avatarClone != null)
            {
                Avatar.DestroyImmediate(avatarClone);
                avatarClone = null;
            }
            if (va.animatorAvatar != null)
            {
                avatarClone = Avatar.Instantiate<Avatar>(va.animatorAvatar);
                avatarClone.hideFlags |= HideFlags.HideAndDontSave;
                va.uAvatar.SetArmStretch(avatarClone, 0.0001f);  //Since it is occasionally wrong value when it is 0
                va.uAvatar.SetLegStretch(avatarClone, 0.0001f);
                va.calcObject.animator.avatar = avatarClone;
            }
            #endregion

            for (int target = 0; target < ikData.Length; target++)
            {
                var hiStart = GetStartHumanoidIndex((IKTarget)target);
                ikData[target].root = va.editHumanoidBones[(int)hiStart];
            }

            va.calcObject.vaEdit.onAnimatorIK -= AnimatorOnAnimatorIK;
            va.calcObject.vaEdit.onAnimatorIK += AnimatorOnAnimatorIK;
        }

        public void OnSelectionChange()
        {
            if (ikReorderableList != null)
            {
                if (ikActiveTarget != IKTarget.None)
                {
                    ikReorderableList.index = (int)ikActiveTarget;
                }
                else
                {
                    ikReorderableList.index = -1;
                }
            }
        }

        public void UpdateSynchroIKSet()
        {
            for (int i = 0; i < ikData.Length; i++)
            {
                if (ikData[i].enable && ikData[i].synchroIKtarget)
                {
                    SynchroSet((IKTarget)i);
                }
                ikData[i].synchroIKtarget = false;
            }
        }
        public void SynchroSet(IKTarget target)
        {
            if (!va.isHuman) return;

            const float DotThreshold = 0.99f;

            var data = ikData[(int)target];
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            switch (target)
            {
            case IKTarget.Head:
                {
                    Func<HumanBodyBones, Vector3, Vector3> CommonizationVector = (humanoidIndex, vec) =>
                    {
                        var t = va.editHumanoidBones[(int)humanoidIndex].transform;
                        return (t.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)humanoidIndex)) * vec;
                    };

                    if (va.editHumanoidBones[(int)HumanBodyBones.LeftEye] != null && va.editHumanoidBones[(int)HumanBodyBones.RightEye])
                    {
                        var tL = va.editHumanoidBones[(int)HumanBodyBones.LeftEye].transform;
                        var tR = va.editHumanoidBones[(int)HumanBodyBones.RightEye].transform;
                        position = Vector3.Lerp(tL.position, tR.position, 0.5f) + CommonizationVector(HumanBodyBones.LeftEye, Vector3.right);
                    }
                    else if (va.editHumanoidBones[(int)HumanBodyBones.Head] != null)
                    {
                        var t = va.editHumanoidBones[(int)HumanBodyBones.Head].transform;
                        position = t.position + CommonizationVector(HumanBodyBones.Head, Vector3.down);
                    }
                    rotation = Quaternion.identity;

                    {
                        var hp = new HumanPose();
                        va.GetHumanPose(ref hp);

                        float angleNeck, angleHead;
                        {
                            var muscle = hp.muscles[neckMuscleIndexes[1]];
                            float angle = (va.humanoidMuscleLimit[(int)HumanBodyBones.Neck].max.y - va.humanoidMuscleLimit[(int)HumanBodyBones.Neck].min.y) / 2f;
                            angleNeck = (-angle * muscle);
                        }
                        {
                            var muscle = hp.muscles[headMuscleIndexes[1]];
                            float angle = (va.humanoidMuscleLimit[(int)HumanBodyBones.Head].max.y - va.humanoidMuscleLimit[(int)HumanBodyBones.Head].min.y) / 2f;
                            angleHead = (-angle * muscle);
                        }
                        data.swivelRotation = angleNeck + angleHead;
                    }
                }
                break;
            case IKTarget.LeftHand:
                if (va.editHumanoidBones[(int)HumanBodyBones.LeftUpperArm] != null && va.editHumanoidBones[(int)HumanBodyBones.LeftHand] != null)
                {
                    var tA = va.editHumanoidBones[(int)HumanBodyBones.LeftUpperArm].transform;
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.LeftHand].transform;
                    var tC = va.editHumanoidBones[(int)HumanBodyBones.LeftLowerArm].transform;
                    position = tB.position;
                    rotation = tB.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.LeftHand) * IKTargetSyncRotation[(int)IKTarget.LeftHand];
                    var axis = tB.position - tA.position;
                    var dot = Vector3.Dot((tC.position - tA.position).normalized, (tB.position - tC.position).normalized);
                    axis.Normalize();
                    if (axis.sqrMagnitude > 0f && Mathf.Abs(dot) < DotThreshold)
                    {
                        var worldToLocalMatrix = Matrix4x4.TRS(va.GetHumanWorldRootPosition(), va.GetHumanWorldRootRotation(), Vector3.one).inverse;
                        axis = worldToLocalMatrix.MultiplyVector(axis);
                        var posA = worldToLocalMatrix.MultiplyPoint3x4(tA.position);
                        var posC = worldToLocalMatrix.MultiplyPoint3x4(tC.position);
                        var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                        var vec = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, axis)) * (posC - posP).normalized;
                        var rot = Quaternion.FromToRotation(Vector3.up, vec);
                        data.swivelRotation = rot.eulerAngles.z;
                    }
                    else
                    {
                        data.swivelRotation = 0f;
                    }
                }
                break;
            case IKTarget.RightHand:
                if (va.editHumanoidBones[(int)HumanBodyBones.RightUpperArm] != null && va.editHumanoidBones[(int)HumanBodyBones.RightHand] != null)
                {
                    var tA = va.editHumanoidBones[(int)HumanBodyBones.RightUpperArm].transform;
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.RightHand].transform;
                    var tC = va.editHumanoidBones[(int)HumanBodyBones.RightLowerArm].transform;
                    position = tB.position;
                    rotation = tB.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.RightHand) * IKTargetSyncRotation[(int)IKTarget.RightHand];
                    var axis = tB.position - tA.position;
                    var dot = Vector3.Dot((tC.position - tA.position).normalized, (tB.position - tC.position).normalized);
                    axis.Normalize();
                    if (axis.sqrMagnitude > 0f && Mathf.Abs(dot) < DotThreshold)
                    {
                        var worldToLocalMatrix = Matrix4x4.TRS(va.GetHumanWorldRootPosition(), va.GetHumanWorldRootRotation(), Vector3.one).inverse;
                        axis = worldToLocalMatrix.MultiplyVector(axis);
                        var posA = worldToLocalMatrix.MultiplyPoint3x4(tA.position);
                        var posC = worldToLocalMatrix.MultiplyPoint3x4(tC.position);
                        var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                        var vec = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, axis)) * (posC - posP).normalized;
                        var rot = Quaternion.FromToRotation(Vector3.up, vec);
                        data.swivelRotation = rot.eulerAngles.z;
                    }
                    else
                    {
                        data.swivelRotation = 0f;
                    }
                }
                break;
            case IKTarget.LeftFoot:
                if (va.editHumanoidBones[(int)HumanBodyBones.LeftUpperLeg] != null && va.editHumanoidBones[(int)HumanBodyBones.LeftFoot] != null)
                {
                    var tA = va.editHumanoidBones[(int)HumanBodyBones.LeftUpperLeg].transform;
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.LeftFoot].transform;
                    var tC = va.editHumanoidBones[(int)HumanBodyBones.LeftLowerLeg].transform;
                    position = tB.position;
                    rotation = tB.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.LeftFoot) * IKTargetSyncRotation[(int)IKTarget.LeftFoot];
                    var axis = tB.position - tA.position;
                    var dot = Vector3.Dot((tC.position - tA.position).normalized, (tB.position - tC.position).normalized);
                    axis.Normalize();
                    if (axis.sqrMagnitude > 0f && Mathf.Abs(dot) < DotThreshold)
                    {
                        var worldToLocalMatrix = Matrix4x4.TRS(va.GetHumanWorldRootPosition(), va.GetHumanWorldRootRotation(), Vector3.one).inverse;
                        axis = worldToLocalMatrix.MultiplyVector(axis);
                        var posA = worldToLocalMatrix.MultiplyPoint3x4(tA.position);
                        var posC = worldToLocalMatrix.MultiplyPoint3x4(tC.position);
                        var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                        var vec = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, axis)) * (posC - posP).normalized;
                        var rot = Quaternion.FromToRotation(Vector3.up, vec);
                        data.swivelRotation = rot.eulerAngles.z;
                    }
                    else
                    {
                        data.swivelRotation = 0f;
                    }
                }
                break;
            case IKTarget.RightFoot:
                if (va.editHumanoidBones[(int)HumanBodyBones.RightUpperLeg] != null && va.editHumanoidBones[(int)HumanBodyBones.RightFoot] != null)
                {
                    var tA = va.editHumanoidBones[(int)HumanBodyBones.RightUpperLeg].transform;
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.RightFoot].transform;
                    var tC = va.editHumanoidBones[(int)HumanBodyBones.RightLowerLeg].transform;
                    position = tB.position;
                    rotation = tB.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.RightFoot) * IKTargetSyncRotation[(int)IKTarget.RightFoot];
                    var axis = tB.position - tA.position;
                    var dot = Vector3.Dot((tC.position - tA.position).normalized, (tB.position - tC.position).normalized);
                    axis.Normalize();
                    if (axis.sqrMagnitude > 0f && Mathf.Abs(dot) < DotThreshold)
                    {
                        var worldToLocalMatrix = Matrix4x4.TRS(va.GetHumanWorldRootPosition(), va.GetHumanWorldRootRotation(), Vector3.one).inverse;
                        axis = worldToLocalMatrix.MultiplyVector(axis);
                        var posA = worldToLocalMatrix.MultiplyPoint3x4(tA.position);
                        var posC = worldToLocalMatrix.MultiplyPoint3x4(tC.position);
                        var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                        var vec = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, axis)) * (posC - posP).normalized;
                        var rot = Quaternion.FromToRotation(Vector3.up, vec);
                        data.swivelRotation = rot.eulerAngles.z;
                    }
                    else
                    {
                        data.swivelRotation = 0f;
                    }
                }
                break;
            }

            switch (data.spaceType)
            {
            case AnimatorIKData.SpaceType.Global:
            case AnimatorIKData.SpaceType.Local:
                data.worldPosition = position;
                data.worldRotation = rotation;
                break;
            case AnimatorIKData.SpaceType.Parent:
                //not update
                if (target == IKTarget.Head)
                    data.rotation = Quaternion.identity;
                break;
            }
            while (data.swivelRotation < -180f || data.swivelRotation > 180f)
            {
                if (data.swivelRotation > 180f)
                    data.swivelRotation -= 360f;
                else if (data.swivelRotation < -180f)
                    data.swivelRotation += 360f;
            }
            data.parentBoneIndex = va.BonesIndexOf(data.parent);

            UpdateOptionData(target);
        }
        public void UpdateOptionData(IKTarget target)
        {
            if (!va.isHuman) return;

            var data = ikData[(int)target];
            switch (target)
            {
            case IKTarget.LeftFoot:
                if (va.editHumanoidBones[(int)HumanBodyBones.LeftToes] != null)
                {
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.LeftFoot].transform;
                    var tD = va.editHumanoidBones[(int)HumanBodyBones.LeftToes].transform;
                    data.optionRotation = data.worldRotation;
                    data.optionPosition = data.worldPosition + (data.optionRotation * Vector3.back) * Vector3.Distance(tD.position, tB.position) * 6f;
                }
                break;
            case IKTarget.RightFoot:
                if (va.editHumanoidBones[(int)HumanBodyBones.RightToes] != null)
                {
                    var tB = va.editHumanoidBones[(int)HumanBodyBones.RightFoot].transform;
                    var tD = va.editHumanoidBones[(int)HumanBodyBones.RightToes].transform;
                    data.optionRotation = data.worldRotation;
                    data.optionPosition = data.worldPosition + (data.optionRotation * Vector3.back) * Vector3.Distance(tD.position, tB.position) * 6f;
                }
                break;
            }
        }

        public bool IsValid(IKTarget target)
        {
            if (!va.isHuman || va.boneDefaultPose == null)
                return false;

            var boneIndex = va.humanoidIndex2boneIndex[(int)GetEndHumanoidIndex(target)];
            boneIndex = va.parentBoneIndexes[boneIndex];
            while (boneIndex >= 0)
            {
                if (va.boneDefaultPose[boneIndex] == null ||
                   va.boneDefaultPose[boneIndex].scale != va.bones[boneIndex].transform.localScale)
                    return false;
                boneIndex = va.parentBoneIndexes[boneIndex];
            }

            return true;
        }

        public void UpdateIK(bool rootUpdated)
        {
            if (!va.isHuman) return;
            if (!GetUpdateIKtargetAll()) return;

            va.UpdateSyncEditorCurveClip();

            var time = va.currentTime;

            {
                for (int i = 0; i < 3; i++)
                    rootTCurves[i] = va.GetAnimationCurveAnimatorRootT(i);
                for (int i = 0; i < 4; i++)
                    rootQCurves[i] = va.GetAnimationCurveAnimatorRootQ(i);
                for (int i = 0; i < muscleCurves.Length; i++)
                    muscleCurves[i] = null;
            }

            #region Reset Head LeftRight
            if (ikData[(int)IKTarget.Head].isUpdate)
            {
                for (int i = 0; i < neckMuscleIndexes.Length; i++)
                {
                    var muscleIndex = neckMuscleIndexes[i];
                    muscleCurves[muscleIndex] = va.GetAnimationCurveAnimatorMuscle(muscleIndex);
                }
                for (int i = 0; i < headMuscleIndexes.Length; i++)
                {
                    var muscleIndex = headMuscleIndexes[i];
                    muscleCurves[muscleIndex] = va.GetAnimationCurveAnimatorMuscle(muscleIndex);
                }

                va.SetKeyframe(muscleCurves[neckMuscleIndexes[0]], time, 0f);
                va.SetKeyframe(muscleCurves[headMuscleIndexes[0]], time, 0f);
                {
                    float angle = (va.humanoidMuscleLimit[(int)HumanBodyBones.Neck].max.y - va.humanoidMuscleLimit[(int)HumanBodyBones.Neck].min.y) / 2f;
                    var rate = (-ikData[(int)IKTarget.Head].swivelRotation / angle) / 2f;
                    va.SetKeyframe(muscleCurves[neckMuscleIndexes[1]], time, rate);
                }
                {
                    float angle = (va.humanoidMuscleLimit[(int)HumanBodyBones.Head].max.y - va.humanoidMuscleLimit[(int)HumanBodyBones.Head].min.y) / 2f;
                    var rate = (-ikData[(int)IKTarget.Head].swivelRotation / angle) / 2f;
                    va.SetKeyframe(muscleCurves[headMuscleIndexes[1]], time, rate);
                }
                {
                    var rate = muscleCurves[neckMuscleIndexes[2]].Evaluate(time);
                    rate = Mathf.Clamp(rate, -1f, 1f);
                    va.SetKeyframe(muscleCurves[neckMuscleIndexes[2]], time, rate);
                }
                {
                    var rate = muscleCurves[headMuscleIndexes[2]].Evaluate(time);
                    rate = Mathf.Clamp(rate, -1f, 1f);
                    va.SetKeyframe(muscleCurves[headMuscleIndexes[2]], time, rate);
                }

                for (int i = 0; i < neckMuscleIndexes.Length; i++)
                {
                    var muscleIndex = neckMuscleIndexes[i];
                    va.SetAnimationCurveAnimatorMuscle(muscleIndex, muscleCurves[muscleIndex]);
                }
                for (int i = 0; i < headMuscleIndexes.Length; i++)
                {
                    var muscleIndex = headMuscleIndexes[i];
                    va.SetAnimationCurveAnimatorMuscle(muscleIndex, muscleCurves[muscleIndex]);
                }

                va.UpdateSyncEditorCurveClip();
            }
            #endregion

            ikSaveWorldToLocalRotation = Quaternion.Inverse(va.editGameObject.transform.rotation);
            ikSaveWorldToLocalMatrix = va.editGameObject.transform.worldToLocalMatrix;

            va.calcObject.vaEdit.SetAnimationClip(va.currentClip);
            va.calcObject.vaEdit.SetIKPass(true);
            va.calcObject.SetOrigin();

            var humanoidBones = va.calcObject.humanoidBones;
            var hp = new HumanPose();

            #region Loop
            int loopCount = 1;
            {
                if (rootUpdated) loopCount += 3;
                else if (va.rootCorrectionMode == VeryAnimation.RootCorrectionMode.Disable) loopCount += 3;

                foreach (var data in ikData)
                {
                    if (data.isUpdate &&
                        data.spaceType == AnimatorIKData.SpaceType.Parent &&
                        data.parentBoneIndex >= 0)
                    {
                        loopCount = Math.Max(loopCount, 2);
                    }
                }
            }
            for (int loop = 0; loop < loopCount; loop++)
            {
                va.UpdateSyncEditorCurveClip();

                muscleCurvesUpdated.Clear();

                #region ikSaveRoot
                {
                    ikSaveRootT = Vector3.zero;
                    for (int i = 0; i < 3; i++)
                    {
                        ikSaveRootT[i] = rootTCurves[i].Evaluate(time);
                    }
                }
                {
                    Vector4 result = new Vector4(0, 0, 0, 1);
                    for (int i = 0; i < 4; i++)
                        result[i] = rootQCurves[i].Evaluate(time);
                    result.Normalize();
                    if (result.sqrMagnitude > 0f)
                    {
                        ikSaveRootQ = new Quaternion(result.x, result.y, result.z, result.w);
                    }
                    else
                    {
                        ikSaveRootQ = Quaternion.identity;
                    }
                }
                #endregion

                #region Update
                {
                    float normalizedTime;
                    {
                        AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(va.currentClip);
                        var totalTime = animationClipSettings.stopTime - animationClipSettings.startTime;
                        var ttime = time;
                        if (ttime > 0f && ttime >= totalTime)
                            ttime = totalTime - 0.0001f;
                        normalizedTime = totalTime == 0.0 ? 0.0f : (float)((ttime - animationClipSettings.startTime) / (totalTime));
                    }

                    ikSwivelWeight = 0f;
                    va.calcObject.AnimatorRebind();
                    va.calcObject.animator.Play(va.calcObject.vaEdit.stateNameHash, 0, normalizedTime);
                    va.calcObject.animator.Update(0f);

                    #region IKSwivel
                    {
                        var rootRotation = ikSaveRootQ;
                        var rootRotationInv = Quaternion.Inverse(rootRotation);
                        {
                            var posA = humanoidBones[(int)HumanBodyBones.LeftUpperArm].transform.position;
                            var posB = humanoidBones[(int)HumanBodyBones.LeftHand].transform.position;
                            var axis = posB - posA;
                            axis.Normalize();
                            if (axis.sqrMagnitude > 0f)
                            {
                                var posC = humanoidBones[(int)HumanBodyBones.LeftLowerArm].transform.position;
                                var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                                float length = Vector3.Distance((posA + axis * Vector3.Dot((posC - posA), axis)), posC);

                                var localAxis = rootRotationInv * axis;
                                var vec = Quaternion.AngleAxis(ikData[(int)IKTarget.LeftHand].swivelRotation, localAxis) * (Quaternion.FromToRotation(Vector3.forward, localAxis) * Vector3.up);
                                vec = rootRotation * vec;

                                ikData[(int)IKTarget.LeftHand].swivelPosition = posP + vec * length;
                            }
                        }
                        {
                            var posA = humanoidBones[(int)HumanBodyBones.RightUpperArm].transform.position;
                            var posB = humanoidBones[(int)HumanBodyBones.RightHand].transform.position;
                            var axis = posB - posA;
                            axis.Normalize();
                            if (axis.sqrMagnitude > 0f)
                            {
                                var posC = humanoidBones[(int)HumanBodyBones.RightLowerArm].transform.position;
                                var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                                float length = Vector3.Distance((posA + axis * Vector3.Dot((posC - posA), axis)), posC);

                                var localAxis = rootRotationInv * axis;
                                var vec = Quaternion.AngleAxis(ikData[(int)IKTarget.RightHand].swivelRotation, localAxis) * (Quaternion.FromToRotation(Vector3.forward, localAxis) * Vector3.up);
                                vec = rootRotation * vec;

                                ikData[(int)IKTarget.RightHand].swivelPosition = posP + vec * length;
                            }
                        }
                        {
                            var posA = humanoidBones[(int)HumanBodyBones.LeftUpperLeg].transform.position;
                            var posB = humanoidBones[(int)HumanBodyBones.LeftFoot].transform.position;
                            var axis = posB - posA;
                            axis.Normalize();
                            if (axis.sqrMagnitude > 0f)
                            {
                                var posC = humanoidBones[(int)HumanBodyBones.LeftLowerLeg].transform.position;
                                var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                                float length = Vector3.Distance((posA + axis * Vector3.Dot((posC - posA), axis)), posC);

                                var localAxis = rootRotationInv * axis;
                                var vec = Quaternion.AngleAxis(ikData[(int)IKTarget.LeftFoot].swivelRotation, localAxis) * (Quaternion.FromToRotation(Vector3.forward, localAxis) * Vector3.up);
                                vec = rootRotation * vec;

                                ikData[(int)IKTarget.LeftFoot].swivelPosition = posP + vec * length;
                            }
                        }
                        {
                            var posA = humanoidBones[(int)HumanBodyBones.RightUpperLeg].transform.position;
                            var posB = humanoidBones[(int)HumanBodyBones.RightFoot].transform.position;
                            var axis = posB - posA;
                            axis.Normalize();
                            if (axis.sqrMagnitude > 0f)
                            {
                                var posC = humanoidBones[(int)HumanBodyBones.RightLowerLeg].transform.position;
                                var posP = posA + axis * Vector3.Dot((posC - posA), axis);
                                float length = Vector3.Distance((posA + axis * Vector3.Dot((posC - posA), axis)), posC);

                                var localAxis = rootRotationInv * axis;
                                var vec = Quaternion.AngleAxis(ikData[(int)IKTarget.RightFoot].swivelRotation, localAxis) * (Quaternion.FromToRotation(Vector3.forward, localAxis) * Vector3.up);
                                vec = rootRotation * vec;

                                ikData[(int)IKTarget.RightFoot].swivelPosition = posP + vec * length;
                            }
                        }
                    }
                    #endregion

                    ikSwivelWeight = 1f;
                    va.calcObject.animator.Update(0f);
                }
                #endregion

                #region Options
                {
                    va.calcObject.humanPoseHandler.GetHumanPose(ref hp);
                    #region Virtual Neck
                    if (ikData[(int)IKTarget.Head].isUpdate && humanoidBones[(int)HumanBodyBones.Neck] == null)
                    {
                        for (int dof = 0; dof < 3; dof++)
                        {
                            var muscleNeck = neckMuscleIndexes[dof];
                            var muscleHead = headMuscleIndexes[dof];
                            if (muscleNeck >= 0 && muscleHead >= 0)
                            {
                                hp.muscles[muscleNeck] = hp.muscles[muscleHead] / 2f;
                                hp.muscles[muscleHead] = hp.muscles[muscleHead] / 2f;
                            }
                        }
                    }
                    #endregion
                    for (int i = 0; i < hp.muscles.Length; i++)
                    {
                        var humanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(i);
                        var target = IsIKBone(humanoidIndex);
                        if (target == IKTarget.None)
                            continue;
                        var data = ikData[(int)target];
                        if (!data.isUpdate)
                            continue;
                        if (data.autoRotation)
                        {
                            if (humanoidIndex == GetEndHumanoidIndex(target))
                                hp.muscles[i] = 0f;
                            else
                            {   //Twist
                                switch (target)
                                {
                                case IKTarget.LeftHand:
                                    if (humanoidIndex == HumanBodyBones.LeftLowerArm && HumanTrait.MuscleFromBone((int)HumanBodyBones.LeftLowerArm, 0) == i)
                                        hp.muscles[i] = 0f;
                                    break;
                                case IKTarget.RightHand:
                                    if (humanoidIndex == HumanBodyBones.RightLowerArm && HumanTrait.MuscleFromBone((int)HumanBodyBones.RightLowerArm, 0) == i)
                                        hp.muscles[i] = 0f;
                                    break;
                                case IKTarget.LeftFoot:
                                    if (humanoidIndex == HumanBodyBones.LeftLowerLeg && HumanTrait.MuscleFromBone((int)HumanBodyBones.LeftLowerLeg, 0) == i)
                                        hp.muscles[i] = 0f;
                                    break;
                                case IKTarget.RightFoot:
                                    if (humanoidIndex == HumanBodyBones.RightLowerLeg && HumanTrait.MuscleFromBone((int)HumanBodyBones.RightLowerLeg, 0) == i)
                                        hp.muscles[i] = 0f;
                                    break;
                                }
                            }
                        }
                        if (va.clampMuscle)
                        {
                            hp.muscles[i] = Mathf.Clamp(hp.muscles[i], -1f, 1f);
                        }
                    }
                    va.calcObject.humanPoseHandler.SetHumanPose(ref hp);
                }
                #endregion

                #region SetKeyframe
                {
                    va.calcObject.humanPoseHandler.GetHumanPose(ref hp);
                    #region Virtual Neck
                    if (ikData[(int)IKTarget.Head].isUpdate && humanoidBones[(int)HumanBodyBones.Neck] == null)
                    {
                        for (int dof = 0; dof < 3; dof++)
                        {
                            var muscleNeck = neckMuscleIndexes[dof];
                            var muscleHead = headMuscleIndexes[dof];
                            if (muscleNeck >= 0 && muscleHead >= 0)
                            {
                                hp.muscles[muscleNeck] = hp.muscles[muscleHead] / 2f;
                                hp.muscles[muscleHead] = hp.muscles[muscleHead] / 2f;
                            }
                        }
                    }
                    #endregion
                    for (int i = 0; i < hp.muscles.Length; i++)
                    {
                        var humanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(i);
                        var target = IsIKBone(humanoidIndex);
                        if (target == IKTarget.None)
                            continue;
                        var data = ikData[(int)target];
                        if (!data.isUpdate)
                            continue;
                        if (muscleCurves[i] == null)
                        {
                            muscleCurves[i] = va.GetAnimationCurveAnimatorMuscle(i);
                        }
                        va.SetKeyframe(muscleCurves[i], time, hp.muscles[i]);
                        muscleCurvesUpdated.Add(i);
                    }
                }
                #endregion

                #region Write
                {
                    foreach (var i in muscleCurvesUpdated)
                    {
                        va.SetAnimationCurveAnimatorMuscle(i, muscleCurves[i]);
                    }
                }
                #endregion
            }
            #endregion

            va.calcObject.SetOutside();
        }

        public void HandleGUI()
        {
            if (!va.isHuman) return;

            if (ikActiveTarget != IKTarget.None && ikData[(int)ikActiveTarget].enable)
            {
                var activeData = ikData[(int)ikActiveTarget];
                var worldPosition = activeData.worldPosition;
                var worldRotation = activeData.worldRotation;
                var hiA = GetStartHumanoidIndex(ikActiveTarget);
                {
                    if (ikActiveTarget == IKTarget.Head)
                    {
                        #region IKSwivel
                        var posA = va.editHumanoidBones[(int)hiA].transform.position;
                        var posB = worldPosition;
                        var axis = posB - posA;
                        axis.Normalize();
                        if (axis.sqrMagnitude > 0f)
                        {
                            var posP = Vector3.Lerp(posA, posB, 0.5f);
                            Vector3 posPC;
                            {
                                var tpos = posP;
                                {
                                    var post = va.uAvatar.GetPostRotation(va.editAnimator.avatar, (int)HumanBodyBones.Head);
                                    var up = (va.editHumanoidBones[(int)HumanBodyBones.Head].transform.rotation * post) * Vector3.right;
                                    tpos += up;
                                }
                                Vector3 vec;
                                vec = tpos - posP;
                                var length = Vector3.Dot(vec, axis);
                                posPC = tpos - axis * length;
                            }
                            {
                                Handles.color = new Color(Handles.centerColor.r, Handles.centerColor.g, Handles.centerColor.b, Handles.centerColor.a * 0.5f);
                                Handles.DrawWireDisc(posP, axis, HandleUtility.GetHandleSize(posP));
                                {
                                    Handles.color = Handles.centerColor;
                                    Handles.DrawLine(posP, posP + (posPC - posP).normalized * HandleUtility.GetHandleSize(posP));
                                }
                            }
                            {
                                EditorGUI.BeginChangeCheck();
                                Handles.color = Handles.yAxisColor;
                                var rotDofDistSave = uDisc.GetRotationDist();
                                Handles.Disc(Quaternion.identity, posP, axis, HandleUtility.GetHandleSize(posP), true, uSnapSettings.rotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(vaw, "Rotate IK Swivel");
                                    var rotDist = uDisc.GetRotationDist() - rotDofDistSave;
                                    foreach (var target in ikTargetSelect)
                                    {
                                        var data = ikData[(int)target];
                                        data.swivelRotation -= rotDist;
                                        while (data.swivelRotation < -180f || data.swivelRotation > 180f)
                                        {
                                            if (data.swivelRotation > 180f)
                                                data.swivelRotation -= 360f;
                                            else if (data.swivelRotation < -180f)
                                                data.swivelRotation += 360f;
                                        }
                                        va.SetUpdateIKtargetAnimatorIK(target);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region IKSwivel
                        var posA = va.editHumanoidBones[(int)hiA].transform.position;
                        var posB = worldPosition;
                        var axis = posB - posA;
                        axis.Normalize();
                        if (axis.sqrMagnitude > 0f)
                        {
                            var posSwivel = va.editGameObject.transform.localToWorldMatrix.MultiplyPoint3x4(activeData.swivelPosition);
                            var posP = Vector3.Lerp(posA, posB, 0.5f);
                            {
                                Handles.color = new Color(Handles.centerColor.r, Handles.centerColor.g, Handles.centerColor.b, Handles.centerColor.a * 0.5f);
                                Handles.DrawWireDisc(posP, axis, HandleUtility.GetHandleSize(posP));
                                if (activeData.swivelPosition != Vector3.zero)
                                {
                                    var posPC = posA + axis * Vector3.Dot((posSwivel - posA), axis);
                                    Handles.color = Handles.centerColor;
                                    Handles.DrawLine(posP, posP + (posSwivel - posPC).normalized * HandleUtility.GetHandleSize(posP));

                                    //DebugSwivel
                                    //Handles.color = Color.red;
                                    //Handles.DrawLine(posP, posSwivel);
                                }
                            }
                            {
                                EditorGUI.BeginChangeCheck();
                                Handles.color = Handles.zAxisColor;
                                var rotDofDistSave = uDisc.GetRotationDist();
                                Handles.Disc(Quaternion.identity, posP, axis, HandleUtility.GetHandleSize(posP), true, uSnapSettings.rotation);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(vaw, "Rotate IK Swivel");
                                    var rotDist = uDisc.GetRotationDist() - rotDofDistSave;
                                    foreach (var target in ikTargetSelect)
                                    {
                                        var data = ikData[(int)target];
                                        data.swivelRotation -= rotDist;
                                        while (data.swivelRotation < -180f || data.swivelRotation > 180f)
                                        {
                                            if (data.swivelRotation > 180f)
                                                data.swivelRotation -= 360f;
                                            else if (data.swivelRotation < -180f)
                                                data.swivelRotation += 360f;
                                        }
                                        va.SetUpdateIKtargetAnimatorIK(target);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    if (ikActiveTarget != IKTarget.Head &&
                        !activeData.autoRotation && va.lastTool != Tool.Move)
                    {
                        #region Rotate
                        EditorGUI.BeginChangeCheck();
                        var rotation = Handles.RotationHandle(Tools.pivotRotation == PivotRotation.Local ? worldRotation : Tools.handleRotation, worldPosition);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Rotate IK Target");
                            Action<Quaternion> RotationAction = (move) =>
                            {
                                foreach (var target in ikTargetSelect)
                                {
                                    var data = ikData[(int)target];
                                    data.worldRotation = data.worldRotation * move;
                                    {   //Handles error -> Quaternion To Matrix conversion failed because input Quaternion is invalid
                                        float angle;
                                        Vector3 axis;
                                        data.worldRotation.ToAngleAxis(out angle, out axis);
                                        data.worldRotation = Quaternion.AngleAxis(angle, axis);
                                    }
                                    va.SetUpdateIKtargetAnimatorIK(target);
                                    UpdateOptionData(target);
                                }
                            };
                            if (Tools.pivotRotation == PivotRotation.Local)
                            {
                                var move = Quaternion.Inverse(worldRotation) * rotation;
                                RotationAction(move);
                            }
                            else
                            {
                                float angle;
                                Vector3 axis;
                                (Quaternion.Inverse(Tools.handleRotation) * rotation).ToAngleAxis(out angle, out axis);
                                var move = Quaternion.Inverse(worldRotation) * Quaternion.AngleAxis(angle, Tools.handleRotation * axis) * worldRotation;
                                RotationAction(move);
                                Tools.handleRotation = rotation;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Move
                        Handles.color = Color.white;
                        EditorGUI.BeginChangeCheck();
                        var position = Handles.PositionHandle(worldPosition, Tools.pivotRotation == PivotRotation.Local ? worldRotation : Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Move IK Target");
                            var move = position - worldPosition;
                            foreach (var target in ikTargetSelect)
                            {
                                ikData[(int)target].worldPosition = ikData[(int)target].worldPosition + move;
                                ikData[(int)target].optionPosition = ikData[(int)target].optionPosition + move;
                                va.SetUpdateIKtargetAnimatorIK(target);
                                UpdateOptionData(target);
                            }
                        }
                        #endregion
                    }
                    if (!activeData.autoRotation &&
                        ((ikActiveTarget == IKTarget.LeftFoot && va.editHumanoidBones[(int)HumanBodyBones.LeftToes] != null) ||
                       (ikActiveTarget == IKTarget.RightFoot && va.editHumanoidBones[(int)HumanBodyBones.RightToes] != null)))
                    {
                        {
                            Handles.color = Handles.centerColor;
                            Handles.DrawLine(worldPosition, activeData.optionPosition);
                        }
                        {
                            Handles.color = Color.white;
                            EditorGUI.BeginChangeCheck();
                            var handlePosition = Handles.PositionHandle(activeData.optionPosition, Tools.pivotRotation == PivotRotation.Local ? activeData.optionRotation : Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                var toesTransform = ikActiveTarget == IKTarget.LeftFoot ? va.editHumanoidBones[(int)HumanBodyBones.LeftToes].transform : va.editHumanoidBones[(int)HumanBodyBones.RightToes].transform;
                                var toesPos = toesTransform.position;
                                var beforeVec = activeData.optionPosition - toesPos;
                                var afterVec = handlePosition - toesPos;
                                beforeVec.Normalize();
                                afterVec.Normalize();
                                if (beforeVec.sqrMagnitude > 0f && afterVec.sqrMagnitude > 0f)
                                {
                                    Quaternion rotationY = Quaternion.identity;
                                    {
                                        var normal = activeData.worldRotation * Vector3.up;
                                        var beforeP = activeData.optionPosition - normal * Vector3.Dot(activeData.optionPosition - worldPosition, normal);
                                        var afterP = handlePosition - normal * Vector3.Dot(handlePosition - worldPosition, normal);
                                        rotationY = Quaternion.AngleAxis(EditorCommon.Vector3SignedAngle((beforeP - toesPos).normalized, (afterP - toesPos).normalized, normal), normal);
                                    }
                                    Quaternion rotationX = Quaternion.identity;
                                    {
                                        var normal = activeData.worldRotation * Vector3.right;
                                        var beforeP = activeData.optionPosition - normal * Vector3.Dot(activeData.optionPosition - worldPosition, normal);
                                        var afterP = handlePosition - normal * Vector3.Dot(handlePosition - worldPosition, normal);
                                        rotationX = Quaternion.AngleAxis(EditorCommon.Vector3SignedAngle((beforeP - toesPos).normalized, (afterP - toesPos).normalized, normal), normal);
                                    }
                                    var rotation = rotationX * rotationY;
                                    var afterPosition = toesPos + rotation * (worldPosition - toesPos);
                                    var movePosition = afterPosition - worldPosition;
                                    var moveRotation = rotation;
                                    foreach (var target in ikTargetSelect)
                                    {
                                        ikData[(int)target].worldPosition = ikData[(int)target].worldPosition + movePosition;
                                        ikData[(int)target].worldRotation = moveRotation * ikData[(int)target].worldRotation;
                                        {   //Handles error -> Quaternion To Matrix conversion failed because input Quaternion is invalid
                                            float angle;
                                            Vector3 axis;
                                            ikData[(int)target].worldRotation.ToAngleAxis(out angle, out axis);
                                            ikData[(int)target].worldRotation = Quaternion.AngleAxis(angle, axis);
                                        }
                                        va.SetUpdateIKtargetAnimatorIK(target);
                                        if (target == IKTarget.LeftFoot || target == IKTarget.RightFoot)
                                        {
                                            toesTransform.rotation = Quaternion.Inverse(moveRotation) * toesTransform.rotation;
                                            HumanPose hpAfter = new HumanPose();
                                            va.GetHumanPose(ref hpAfter);
                                            var muscleIndex = target == IKTarget.LeftFoot ? HumanTrait.MuscleFromBone((int)HumanBodyBones.LeftToes, 2) : HumanTrait.MuscleFromBone((int)HumanBodyBones.RightToes, 2);
                                            var muscle = hpAfter.muscles[muscleIndex];
                                            if (va.clampMuscle)
                                                muscle = Mathf.Clamp(muscle, -1f, 1f);
                                            va.SetAnimationValueAnimatorMuscle(muscleIndex, muscle);
                                        }
                                    }
                                }
                                activeData.optionPosition = handlePosition;
                            }
                        }
                    }
                }
            }
        }
        public void TargetGUI()
        {
            if (!va.isHuman) return;

            var e = Event.current;

            for (int target = 0; target < (int)IKTarget.Total; target++)
            {
                if (!ikData[target].enable) continue;

                var worldPosition = ikData[target].worldPosition;
                var worldRotation = ikData[target].worldRotation;
                var ikTarget = (IKTarget)target;
                var hiA = GetStartHumanoidIndex(ikTarget);
                if (ikTargetSelect != null &&
                    EditorCommon.ArrayContains(ikTargetSelect, ikTarget))
                {
                    #region Active
                    {
                        if (ikTarget == ikActiveTarget)
                        {
                            Handles.color = Color.white;
                            var hiA2 = hiA;
                            if (target == (int)IKTarget.Head)
                            {
                                if (va.editHumanoidBones[(int)HumanBodyBones.Neck] != null)
                                    hiA2 = HumanBodyBones.Neck;
                                else
                                    hiA2 = HumanBodyBones.Head;
                            }
                            Vector3 worldPosition2 = va.editHumanoidBones[(int)hiA2].transform.position;
                            Handles.DrawLine(worldPosition, worldPosition2);
                            if (va.dummyObject != null && ikData[target].spaceType == AnimatorIKData.SpaceType.Parent)
                            {
                                Func<Vector3, Vector3> DummySpace2OriginalSpace = (pos) =>
                                {
                                    pos = va.editHumanoidBones[(int)hiA2].transform.worldToLocalMatrix.MultiplyPoint(pos);
                                    return va.humanoidBones[(int)hiA2].transform.localToWorldMatrix.MultiplyPoint3x4(pos);
                                };
                                var dummyWorldPosition = DummySpace2OriginalSpace(worldPosition);
                                var dummyWorldPosition2 = DummySpace2OriginalSpace(worldPosition2);
                                Handles.DrawLine(dummyWorldPosition, dummyWorldPosition2);
                            }
                        }
                        Handles.color = vaw.editorSettings.settingIKTargetActiveColor;
                        if (ikTarget == IKTarget.Head)
                            Handles.SphereHandleCap(0, worldPosition, worldRotation, HandleUtility.GetHandleSize(worldPosition) * vaw.editorSettings.settingIKTargetSize, EventType.Repaint);
                        else
                            Handles.ConeHandleCap(0, worldPosition, worldRotation, HandleUtility.GetHandleSize(worldPosition) * vaw.editorSettings.settingIKTargetSize, EventType.Repaint);
                    }
                    #endregion
                }
                else
                {
                    #region NonActive
                    var freeMoveHandleControlID = -1;
                    Handles.FreeMoveHandle(worldPosition, worldRotation, HandleUtility.GetHandleSize(worldPosition) * vaw.editorSettings.settingIKTargetSize, uSnapSettings.move, (id, pos, rot, size, eventType) =>
                    {
                        freeMoveHandleControlID = id;
                        Handles.color = vaw.editorSettings.settingIKTargetNormalColor;
                        if (ikTarget == IKTarget.Head)
                            Handles.SphereHandleCap(id, worldPosition, worldRotation, HandleUtility.GetHandleSize(worldPosition) * vaw.editorSettings.settingIKTargetSize, eventType);
                        else
                            Handles.ConeHandleCap(id, worldPosition, worldRotation, HandleUtility.GetHandleSize(worldPosition) * vaw.editorSettings.settingIKTargetSize, eventType);
                    });
                    if (GUIUtility.hotControl == freeMoveHandleControlID)
                    {
                        if (e.type == EventType.Layout)
                        {
                            GUIUtility.hotControl = -1;
                            {
                                var ikTargetTmp = ikTarget;
                                EditorApplication.delayCall += () =>
                                {
                                    va.SelectAnimatorIKTargetPlusKey(ikTargetTmp);
                                };
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        public void SelectionGUI()
        {
            if (!va.isHuman) return;
            if (ikActiveTarget == IKTarget.None) return;
            var activeData = ikData[(int)ikActiveTarget];
            #region Warning
            if (!IsValid(ikActiveTarget))
            {
                EditorGUILayout.HelpBox(Language.GetText(Language.Help.AnimatorIKScaleChangedErrorWarning), MessageType.Warning);
            }
            #endregion
            #region IK
            {
                EditorGUILayout.BeginHorizontal();
                #region Mirror
                {
                    var mirrorTarget = IKTargetMirror[(int)ikActiveTarget];
                    if (GUILayout.Button(Language.GetContentFormat(Language.Help.SelectionMirror, (mirrorTarget != IKTarget.None ? string.Format("From 'IK: {0}'", IKTargetStrings[(int)mirrorTarget]) : "From self"))))
                    {
                        va.SelectionHumanoidMirror();
                    }
                }
                #endregion
                EditorGUILayout.Space();
                #region Update
                if (GUILayout.Button(Language.GetContent(Language.Help.SelectionUpdateIK)))
                {
                    Undo.RecordObject(vaw, "Update IK");
                    foreach (var target in ikTargetSelect)
                    {
                        va.SetUpdateIKtargetAnimatorIK(target);
                    }
                }
                #endregion
                EditorGUILayout.Space();
                #region Sync
                EditorGUI.BeginDisabledGroup(activeData.spaceType == AnimatorIKData.SpaceType.Parent);
                if (GUILayout.Button(Language.GetContent(Language.Help.SelectionSyncIK)))
                {
                    Undo.RecordObject(vaw, "Sync IK");
                    foreach (var target in ikTargetSelect)
                    {
                        va.SetSynchroIKtargetAnimatorIK(target);
                    }
                }
                EditorGUI.EndDisabledGroup();
                #endregion
                EditorGUILayout.Space();
                #region Reset
                if (GUILayout.Button(Language.GetContent(Language.Help.SelectionResetIK)))
                {
                    Undo.RecordObject(vaw, "Reset IK");
                    foreach (var target in ikTargetSelect)
                    {
                        Reset(target);
                    }
                }
                #endregion
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            int RowCount = 0;
            #region SpaceType
            {
                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                EditorGUILayout.LabelField("Space", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                var spaceType = (AnimatorIKData.SpaceType)GUILayout.Toolbar((int)activeData.spaceType, IKSpaceTypeStrings, EditorStyles.miniButton);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vaw, "Change IK Position");
                    foreach (var target in ikTargetSelect)
                    {
                        ChangeSpaceType(target, spaceType);
                    }
                    VeryAnimationControlWindow.instance.Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion
            #region Parent
            if (activeData.spaceType == AnimatorIKData.SpaceType.Local || activeData.spaceType == AnimatorIKData.SpaceType.Parent)
            {
                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                EditorGUILayout.LabelField("Parent", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                if (activeData.spaceType == AnimatorIKData.SpaceType.Local)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    if (activeData.root != null && activeData.root.transform.parent != null)
                    {
                        var boneIndex = va.EditBonesIndexOf(activeData.root.transform.parent.gameObject);
                        EditorGUILayout.ObjectField(boneIndex >= 0 ? va.bones[boneIndex] : null, typeof(GameObject), true);
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(null, typeof(GameObject), true);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else if (activeData.spaceType == AnimatorIKData.SpaceType.Parent)
                {
                    var parent = EditorGUILayout.ObjectField(activeData.parent, typeof(GameObject), true) as GameObject;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(vaw, "Change IK Position");
                        foreach (var target in ikTargetSelect)
                        {
                            var data = ikData[(int)target];
                            var worldPosition = data.worldPosition;
                            var worldRotation = data.worldRotation;
                            data.parent = parent;
                            data.worldPosition = worldPosition;
                            data.worldRotation = worldRotation;
                            va.SetUpdateIKtargetAnimatorIK(target);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion
            #region Position
            {
                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                EditorGUILayout.LabelField("Position", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                var position = EditorGUILayout.Vector3Field("", activeData.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vaw, "Change IK Position");
                    var move = position - activeData.position;
                    foreach (var target in ikTargetSelect)
                    {
                        ikData[(int)target].position += move;
                        va.SetUpdateIKtargetAnimatorIK(target);
                    }
                }
                if (activeData.spaceType == AnimatorIKData.SpaceType.Parent)
                {
                    if (GUILayout.Button("Reset", GUILayout.Width(44)))
                    {
                        Undo.RecordObject(vaw, "Change IK Position");
                        foreach (var target in ikTargetSelect)
                        {
                            ikData[(int)target].position = Vector3.zero;
                            va.SetUpdateIKtargetAnimatorIK(target);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion
            if (ikActiveTarget > IKTarget.Head)
            {
                #region Rotation
                {
                    EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                    {
                        EditorGUI.BeginChangeCheck();
                        var autoRotation = !GUILayout.Toggle(!activeData.autoRotation, "Rotation", EditorStyles.toolbarButton, GUILayout.Width(54));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change IK Rotation");
                            foreach (var target in ikTargetSelect)
                            {
                                ikData[(int)target].autoRotation = autoRotation;
                                SynchroSet(target);
                                va.SetUpdateIKtargetAnimatorIK(target);
                            }
                        }
                    }
                    if (!activeData.autoRotation)
                    {
                        EditorGUI.BeginChangeCheck();
                        var eulerAngles = EditorGUILayout.Vector3Field("", activeData.rotation.eulerAngles);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(vaw, "Change IK Rotation");
                            var move = eulerAngles - activeData.rotation.eulerAngles;
                            foreach (var target in ikTargetSelect)
                            {
                                if (target >= IKTarget.LeftHand && target <= IKTarget.RightFoot)
                                {
                                    ikData[(int)target].rotation.eulerAngles += move;
                                    va.SetUpdateIKtargetAnimatorIK(target);
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Auto", EditorStyles.centeredGreyMiniLabel);
                    }
                    if (activeData.spaceType == AnimatorIKData.SpaceType.Parent)
                    {
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            Undo.RecordObject(vaw, "Change IK Rotation");
                            foreach (var target in ikTargetSelect)
                            {
                                ikData[(int)target].rotation = Quaternion.identity;
                                va.SetUpdateIKtargetAnimatorIK(target);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }
            #region Swivel
            {
                EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                EditorGUILayout.LabelField("Swivel", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                var swivelRotation = EditorGUILayout.Slider(activeData.swivelRotation, -180f, 180f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vaw, "Change IK Swivel");
                    var move = swivelRotation - activeData.swivelRotation;
                    foreach (var target in ikTargetSelect)
                    {
                        var data = ikData[(int)target];
                        data.swivelRotation += move;
                        while (data.swivelRotation < -180f || data.swivelRotation > 180f)
                        {
                            if (data.swivelRotation > 180f)
                                data.swivelRotation -= 360f;
                            else if (data.swivelRotation < -180f)
                                data.swivelRotation += 360f;
                        }
                        va.SetUpdateIKtargetAnimatorIK(target);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion
            #endregion
        }
        public void ControlGUI()
        {
            if (!va.isHuman) return;

            EditorGUILayout.BeginVertical(vaw.guiStyleSkinBox);
            if (ikReorderableList != null)
            {
                ikReorderableList.DoLayoutList();
                GUILayout.Space(-14);
                if (advancedFoldout && ikReorderableList.index >= 0 && ikReorderableList.index < ikData.Length)
                {
                    var target = ikReorderableList.index;
                    if ((IKTarget)target == IKTarget.Head)
                    {
                        advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced", true);
                        #region Head
                        EditorGUILayout.BeginVertical(vaw.guiStyleSkinBox);
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(IKTargetStrings[target]);
                            EditorGUILayout.Space();
                            if (GUILayout.Button("Reset", GUILayout.Width(44)))
                            {
                                Undo.RecordObject(vaw, "Change Animator IK Data");
                                ikData[target].headWeight = 1f;
                                ikData[target].eyesWeight = 0f;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel++;
                        {
                            EditorGUI.BeginChangeCheck();
                            var weight = EditorGUILayout.Slider("Head Weight", ikData[target].headWeight, 0f, 1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vaw, "Change Animator IK Data");
                                ikData[target].headWeight = weight;
                                ikData[target].eyesWeight = 1f - ikData[target].headWeight;
                            }
                        }
                        {
                            EditorGUI.BeginChangeCheck();
                            var weight = EditorGUILayout.Slider("Eyes Weight", ikData[target].eyesWeight, 0f, 1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(vaw, "Change Animator IK Data");
                                ikData[target].eyesWeight = weight;
                                ikData[target].headWeight = 1f - ikData[target].eyesWeight;
                            }
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndVertical();
                        #endregion
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        public void AnimatorOnAnimatorIK(int layerIndex)
        {
            var animator = va.calcObject.animator;

            #region ResetRoot
            {
                animator.rootPosition = Vector3.zero;
                animator.rootRotation = Quaternion.identity;
                animator.bodyPosition = animator.rootPosition + ikSaveRootT * animator.humanScale;
                animator.bodyRotation = animator.rootRotation * ikSaveRootQ;
            }
            #endregion

            {
                var data = ikData[(int)IKTarget.Head];
                if (data.isUpdate)
                {
                    Vector3 position;
                    Quaternion rotation;
                    GetCalcWorldTransform(data, out position, out rotation);

                    animator.SetLookAtPosition(position);
                    animator.SetLookAtWeight(1f, 0f, data.headWeight, data.eyesWeight, 0f);
                }
                else
                {
                    animator.SetLookAtWeight(0f);
                }
            }
            {
                var data = ikData[(int)IKTarget.LeftHand];
                if (data.isUpdate)
                {
                    Vector3 position;
                    Quaternion rotation;
                    GetCalcWorldTransform(data, out position, out rotation);

                    animator.SetIKPosition(AvatarIKGoal.LeftHand, position);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, rotation);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, data.swivelPosition);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, ikSwivelWeight);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0f);
                }
            }
            {
                var data = ikData[(int)IKTarget.RightHand];
                if (data.isUpdate)
                {
                    Vector3 position;
                    Quaternion rotation;
                    GetCalcWorldTransform(data, out position, out rotation);

                    animator.SetIKPosition(AvatarIKGoal.RightHand, position);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rotation);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, data.swivelPosition);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, ikSwivelWeight);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0f);
                }
            }
            {
                var data = ikData[(int)IKTarget.LeftFoot];
                if (data.isUpdate)
                {
                    Vector3 position;
                    Quaternion rotation;
                    GetCalcWorldTransform(data, out position, out rotation);

                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, position);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, rotation);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
                    animator.SetIKHintPosition(AvatarIKHint.LeftKnee, data.swivelPosition);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, ikSwivelWeight);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0f);
                }
            }
            {
                var data = ikData[(int)IKTarget.RightFoot];
                if (data.isUpdate)
                {
                    Vector3 position;
                    Quaternion rotation;
                    GetCalcWorldTransform(data, out position, out rotation);

                    animator.SetIKPosition(AvatarIKGoal.RightFoot, position);
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rotation);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
                    animator.SetIKHintPosition(AvatarIKHint.RightKnee, data.swivelPosition);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, ikSwivelWeight);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0f);
                }
            }
        }
        private void GetCalcWorldTransform(AnimatorIKData data, out Vector3 position, out Quaternion rotation)
        {
            if (data.spaceType == AnimatorIKData.SpaceType.Parent && data.parentBoneIndex >= 0)
            {
                var parent = va.calcObject.bones[data.parentBoneIndex].transform;
                position = parent.localToWorldMatrix.MultiplyPoint3x4(data.position);
                rotation = parent.rotation * data.rotation;

                var hipEdit = va.editHumanoidBones[(int)HumanBodyBones.Hips].transform;
                var hipCalc = va.calcObject.humanoidBones[(int)HumanBodyBones.Hips].transform;
                var hipOffsetMatrix = (ikSaveWorldToLocalMatrix * hipEdit.localToWorldMatrix) * hipCalc.worldToLocalMatrix;
                position = hipOffsetMatrix.MultiplyPoint3x4(position);
                var hipOffsetRotation = (ikSaveWorldToLocalRotation * hipEdit.rotation) * Quaternion.Inverse(hipCalc.rotation);
                rotation = hipOffsetRotation * rotation;
            }
            else
            {
                position = ikSaveWorldToLocalMatrix.MultiplyPoint3x4(data.worldPosition);
                rotation = ikSaveWorldToLocalRotation * data.worldRotation;
            }
        }
        private void Reset(IKTarget target)
        {
            var data = ikData[(int)target];
            switch (target)
            {
            case IKTarget.Head:
                {
                    var t = va.editGameObject.transform;
                    Vector3 vec = data.worldPosition - t.position;
                    var normal = t.rotation * Vector3.right;
                    var dot = Vector3.Dot(vec, normal);
                    data.worldPosition -= normal * dot;
                }
                break;
            case IKTarget.LeftHand:
                {
                    {
                        var posA = va.editHumanoidBones[(int)HumanBodyBones.LeftUpperArm].transform.position;
                        var posB = va.editHumanoidBones[(int)HumanBodyBones.LeftHand].transform.position;
                        var posC = va.editHumanoidBones[(int)HumanBodyBones.LeftLowerArm].transform.position;
                        var up = data.worldPosition - Vector3.Lerp(posA, posB, 0.5f);
                        data.worldRotation = Quaternion.LookRotation(posB - posC, up);
                    }
                }
                break;
            case IKTarget.RightHand:
                {
                    {
                        var posA = va.editHumanoidBones[(int)HumanBodyBones.RightUpperArm].transform.position;
                        var posB = va.editHumanoidBones[(int)HumanBodyBones.RightHand].transform.position;
                        var posC = va.editHumanoidBones[(int)HumanBodyBones.RightLowerArm].transform.position;
                        var up = data.worldPosition - Vector3.Lerp(posA, posB, 0.5f);
                        data.worldRotation = Quaternion.LookRotation(posB - posC, up);
                    }
                }
                break;
            case IKTarget.LeftFoot:
                {
                    var rot = va.editHumanoidBones[(int)HumanBodyBones.Hips].transform.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.Hips) * Quaternion.Euler(90f, 90f, 0);
                    {
                        var vec = rot * Vector3.forward;
                        rot = Quaternion.LookRotation((new Vector3(vec.x, 0f, vec.z)).normalized, Vector3.up);
                    }
                    data.worldRotation = rot;
                }
                break;
            case IKTarget.RightFoot:
                {
                    var rot = va.editHumanoidBones[(int)HumanBodyBones.Hips].transform.rotation * va.uAvatar.GetPostRotation(avatarClone, (int)HumanBodyBones.Hips) * Quaternion.Euler(90f, 90f, 0);
                    {
                        var vec = rot * Vector3.forward;
                        rot = Quaternion.LookRotation((new Vector3(vec.x, 0f, vec.z)).normalized, Vector3.up);
                    }
                    data.worldRotation = rot;
                }
                break;
            }
            va.SetUpdateIKtargetAnimatorIK(target);
            UpdateOptionData(target);
        }
        private void ChangeSpaceType(IKTarget target, AnimatorIKData.SpaceType spaceType)
        {
            if (target < 0 || target >= IKTarget.Total) return;
            var data = ikData[(int)target];
            if (data.spaceType == spaceType) return;
            var position = data.worldPosition;
            var rotation = data.worldRotation;
            data.spaceType = spaceType;
            data.worldPosition = position;
            data.worldRotation = rotation;
            data.synchroIKtarget = true;
        }

        public IKTarget IsIKBone(HumanBodyBones hi)
        {
            if (ikData[(int)IKTarget.Head].enable)
            {
                if (ikData[(int)IKTarget.Head].headWeight > 0f)
                    if (hi == HumanBodyBones.Head || hi == HumanBodyBones.Neck)
                        return IKTarget.Head;
                if (ikData[(int)IKTarget.Head].eyesWeight > 0f)
                    if (hi == HumanBodyBones.LeftEye || hi == HumanBodyBones.RightEye)
                        return IKTarget.Head;
            }
            if (ikData[(int)IKTarget.LeftHand].enable)
            {
                if (hi == HumanBodyBones.LeftHand || hi == HumanBodyBones.LeftLowerArm || hi == HumanBodyBones.LeftUpperArm ||
                    (va.humanoidBones[(int)HumanBodyBones.LeftShoulder] == null && hi == HumanBodyBones.LeftShoulder))
                    return IKTarget.LeftHand;
            }
            if (ikData[(int)IKTarget.RightHand].enable)
            {
                if (hi == HumanBodyBones.RightHand || hi == HumanBodyBones.RightLowerArm || hi == HumanBodyBones.RightUpperArm ||
                    (va.humanoidBones[(int)HumanBodyBones.RightShoulder] == null && hi == HumanBodyBones.RightShoulder))
                    return IKTarget.RightHand;
            }
            if (ikData[(int)IKTarget.LeftFoot].enable)
            {
                if (hi == HumanBodyBones.LeftFoot || hi == HumanBodyBones.LeftLowerLeg || hi == HumanBodyBones.LeftUpperLeg)
                    return IKTarget.LeftFoot;
            }
            if (ikData[(int)IKTarget.RightFoot].enable)
            {
                if (hi == HumanBodyBones.RightFoot || hi == HumanBodyBones.RightLowerLeg || hi == HumanBodyBones.RightUpperLeg)
                    return IKTarget.RightFoot;
            }
            return IKTarget.None;
        }

        public void ChangeTargetIK(IKTarget target)
        {
            Undo.RecordObject(vaw, "Change IK");
            if (ikData[(int)target].enable)
            {
                List<GameObject> selectGameObjects = new List<GameObject>();
                switch (target)
                {
                case IKTarget.Head:
                    ikData[(int)target].enable = false;
                    selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.Head]);
                    break;
                case IKTarget.LeftHand:
                    ikData[(int)target].enable = false;
                    selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.LeftHand]);
                    break;
                case IKTarget.RightHand:
                    ikData[(int)target].enable = false;
                    selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.RightHand]);
                    break;
                case IKTarget.LeftFoot:
                    ikData[(int)target].enable = false;
                    selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.LeftFoot]);
                    break;
                case IKTarget.RightFoot:
                    ikData[(int)target].enable = false;
                    selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.RightFoot]);
                    break;
                }
                va.SelectGameObjects(selectGameObjects.ToArray());
            }
            else
            {
                ikData[(int)target].enable = true;
                SynchroSet(target);
                va.SelectAnimatorIKTargetPlusKey(target);
            }
        }
        public bool ChangeSelectionIK()
        {
            Undo.RecordObject(vaw, "Change IK");
            bool changed = false;
            if (ikTargetSelect != null && ikTargetSelect.Length > 0)
            {
                List<GameObject> selectGameObjects = new List<GameObject>();
                foreach (var target in ikTargetSelect)
                {
                    switch (target)
                    {
                    case IKTarget.Head:
                        ikData[(int)target].enable = false;
                        selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.Head]);
                        changed = true;
                        break;
                    case IKTarget.LeftHand:
                        ikData[(int)target].enable = false;
                        selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.LeftHand]);
                        changed = true;
                        break;
                    case IKTarget.RightHand:
                        ikData[(int)target].enable = false;
                        selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.RightHand]);
                        changed = true;
                        break;
                    case IKTarget.LeftFoot:
                        ikData[(int)target].enable = false;
                        selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.LeftFoot]);
                        changed = true;
                        break;
                    case IKTarget.RightFoot:
                        ikData[(int)target].enable = false;
                        selectGameObjects.Add(va.humanoidBones[(int)HumanBodyBones.RightFoot]);
                        changed = true;
                        break;
                    }
                }
                if (changed)
                    va.SelectGameObjects(selectGameObjects.ToArray());
            }
            else
            {
                HashSet<IKTarget> selectIkTargets = new HashSet<IKTarget>();
                foreach (var humanoidIndex in va.SelectionGameObjectsHumanoidIndex())
                {
                    var target = HumanBonesUpdateAnimatorIK[(int)humanoidIndex];
                    if (target < 0 || target >= IKTarget.Total)
                        continue;
                    selectIkTargets.Add(target);
                    changed = true;
                }
                if (changed)
                {
                    foreach (var target in selectIkTargets)
                    {
                        ikData[(int)target].enable = true;
                        SynchroSet(target);
                    }
                    va.SelectIKTargets(selectIkTargets.ToArray(), null);
                }
            }
            return changed;
        }

        public void SetUpdateIKtargetBone(int boneIndex)
        {
            if (boneIndex < 0)
                return;
            {
                var humanoidIndex = va.boneIndex2humanoidIndex[boneIndex];
                if (humanoidIndex >= 0)
                    SetUpdateIKtargetAnimatorIK(HumanBonesUpdateAnimatorIK[(int)humanoidIndex]);
            }
            SetUpdateLinkedIKTarget(boneIndex);
        }
        public void SetUpdateIKtargetAnimatorIK(IKTarget target)
        {
            if (target <= IKTarget.None || ikData == null) return;
            if (target == IKTarget.Total)
            {
                va.SetUpdateIKtargetAll();
                return;
            }
            if (va.rootCorrectionMode == VeryAnimation.RootCorrectionMode.Disable)
            {
                va.SetUpdateIKtargetAll();
            }
            else
            {
                ikData[(int)target].updateIKtarget = true;

                SetUpdateLinkedIKTarget(va.EditBonesIndexOf(ikData[(int)target].root));
            }
        }
        private void SetUpdateLinkedIKTarget(int boneIndex)
        {
            if (boneIndex < 0)
                return;
            foreach (var data in ikData)
            {
                if (data.updateIKtarget)
                    continue;
                if (data.spaceType == AnimatorIKData.SpaceType.Parent &&
                    data.parentBoneIndex >= 0)
                {
                    var index = data.parentBoneIndex;
                    while (index >= 0)
                    {
                        if (boneIndex == index)
                        {
                            data.updateIKtarget = true;
                            break;
                        }
                        index = va.parentBoneIndexes[index];
                    }
                }
            }
        }
        public void SetUpdateIKtargetAll(bool flag)
        {
            if (ikData == null) return;
            foreach (var data in ikData)
            {
                data.updateIKtarget = flag;
            }
        }
        public bool GetUpdateIKtargetAll()
        {
            if (ikData == null) return false;
            foreach (var data in ikData)
            {
                if (data.isUpdate)
                    return true;
            }
            return false;
        }

        public void SetSynchroIKtargetBone(int boneIndex)
        {
            if (boneIndex < 0) return;
            var humanoidIndex = va.boneIndex2humanoidIndex[boneIndex];
            if (humanoidIndex < 0) return;
            SetSynchroIKtargetAnimatorIK(HumanBonesUpdateAnimatorIK[(int)humanoidIndex]);
        }
        public void SetSynchroIKtargetAnimatorIK(IKTarget target)
        {
            if (target <= IKTarget.None || ikData == null) return;
            if (target == IKTarget.Total)
            {
                SetSynchroIKtargetAll();
                return;
            }
            if (!ikData[(int)target].updateIKtarget)
                ikData[(int)target].synchroIKtarget = true;
        }
        public void ResetSynchroIKtargetAll()
        {
            if (ikData == null) return;
            foreach (var data in ikData)
            {
                data.synchroIKtarget = false;
            }
        }
        public void SetSynchroIKtargetAll()
        {
            if (ikData == null) return;
            foreach (var data in ikData)
            {
                if (!data.updateIKtarget)
                    data.synchroIKtarget = true;
            }
        }
        public bool GetSynchroIKtargetAll()
        {
            if (ikData == null) return false;
            foreach (var data in ikData)
            {
                if (data.enable && data.synchroIKtarget)
                    return true;
            }
            return false;
        }

        private HumanBodyBones GetStartHumanoidIndex(IKTarget target)
        {
            var humanoidIndex = HumanBodyBones.Hips;
            switch ((IKTarget)target)
            {
            case IKTarget.Head: humanoidIndex = va.editHumanoidBones[(int)HumanBodyBones.Neck] != null ? HumanBodyBones.Neck : HumanBodyBones.Head; break;
            case IKTarget.LeftHand: humanoidIndex = HumanBodyBones.LeftUpperArm; break;
            case IKTarget.RightHand: humanoidIndex = HumanBodyBones.RightUpperArm; break;
            case IKTarget.LeftFoot: humanoidIndex = HumanBodyBones.LeftUpperLeg; break;
            case IKTarget.RightFoot: humanoidIndex = HumanBodyBones.RightUpperLeg; break;
            }
            return humanoidIndex;
        }
        private HumanBodyBones GetEndHumanoidIndex(IKTarget target)
        {
            var humanoidIndex = HumanBodyBones.Hips;
            switch ((IKTarget)target)
            {
            case IKTarget.Head: humanoidIndex = HumanBodyBones.Head; break;
            case IKTarget.LeftHand: humanoidIndex = HumanBodyBones.LeftHand; break;
            case IKTarget.RightHand: humanoidIndex = HumanBodyBones.RightHand; break;
            case IKTarget.LeftFoot: humanoidIndex = HumanBodyBones.LeftFoot; break;
            case IKTarget.RightFoot: humanoidIndex = HumanBodyBones.RightFoot; break;
            }
            return humanoidIndex;
        }

        public List<HumanBodyBones> SelectionAnimatorIKTargetsHumanoidIndexes()
        {
            List<HumanBodyBones> list = new List<HumanBodyBones>();
            if (ikTargetSelect != null)
            {
                foreach (var ikTarget in ikTargetSelect)
                {
                    for (int i = 0; i < HumanBonesUpdateAnimatorIK.Length; i++)
                    {
                        if (ikTarget == HumanBonesUpdateAnimatorIK[i])
                            list.Add((HumanBodyBones)i);
                    }
                }
            }
            return list;
        }
    }
}
