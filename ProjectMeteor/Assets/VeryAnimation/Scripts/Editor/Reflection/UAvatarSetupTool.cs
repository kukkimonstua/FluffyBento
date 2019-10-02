using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UAvatarSetupTool
    {
        private MethodInfo mi_SampleBindPose;

        public UAvatarSetupTool()
        {
            var asmUnityEditor = Assembly.LoadFrom(InternalEditorUtility.GetEditorAssemblyPath());
            var avatarSetupToolType = asmUnityEditor.GetType("UnityEditor.AvatarSetupTool");
            Assert.IsNotNull(mi_SampleBindPose = avatarSetupToolType.GetMethod("SampleBindPose", BindingFlags.Public | BindingFlags.Static));
        }

        public bool SampleBindPose(GameObject go)
        {
            foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || renderer.sharedMesh == null || renderer.bones == null)
                    return false;
                else if (renderer.bones.Length != renderer.sharedMesh.bindposes.Length)
                {
                    Debug.LogErrorFormat(Language.GetText(Language.Help.LogSampleBindPoseBoneLengthError), renderer.name, renderer.sharedMesh.name);
                    return false;
                }
                {
                    var index = ArrayUtility.IndexOf(renderer.bones, null);
                    if (index >= 0)
                    {
                        Debug.LogErrorFormat(Language.GetText(Language.Help.LogSampleBindPoseBoneNullError), renderer.name, index);
                        return false;
                    }
                }
            }
            try
            {
                mi_SampleBindPose.Invoke(null, new object[] { go });
            }
            catch
            {
                Debug.LogError(Language.GetText(Language.Help.LogSampleBindPoseUnknownError));
                return false;
            }
            return true;
        }
    }
}
