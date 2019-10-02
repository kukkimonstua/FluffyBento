#if UNITY_2017_3_OR_NEWER
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace VeryAnimation
{
    [ScriptedImporter(VeryAnimationWindow.AsmdefVersion, "asmdefChanger")]
    class AssemblyDefinitionChanger : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var checkNameStartWith = "AloneSoft." + typeof(AssemblyDefinitionChanger).Namespace;

            var path = ctx.assetPath.Remove(ctx.assetPath.Length - Path.GetExtension(ctx.assetPath).Length);
            var name = Path.GetFileNameWithoutExtension(path);
            if (!name.StartsWith(checkNameStartWith))
                return;

            #region VersionCheck
            try
            {
                Func<string, int> ToVersion = (t) =>
                {
                    return Convert.ToInt32(Path.GetExtension(t).Remove(0, 1).Replace('_', '0'));
                };

                List<int> versions = new List<int>();
                {
                    var fullPath = Application.dataPath + ctx.assetPath.Remove(0, "Assets".Length);
                    var directoryPath = Path.GetDirectoryName(fullPath);
                    var dllNameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(ctx.assetPath));
                    foreach (var p in Directory.GetFiles(directoryPath, "*.asmdefChanger"))
                    {
                        var subDllNameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p));
                        if (dllNameWithoutExt != subDllNameWithoutExt)
                            continue;

                        var subPath = p.Remove(p.Length - Path.GetExtension(p).Length);
                        var subVersion = ToVersion(subPath);
                        versions.Add(subVersion);
                    }
                    versions.Sort();
                }

                int targetVersion = -1;
                {
                    var editorVersion = Convert.ToInt32(Path.GetFileNameWithoutExtension(Application.unityVersion).Replace('.', '0'));
                    foreach (var ver in versions)
                    {
                        if (ver <= editorVersion)
                            targetVersion = ver;
                    }
                }
                if (targetVersion < 0)
                    return;

                var currentVersion = ToVersion(path);
                if (currentVersion != targetVersion)
                    return;
            }
            catch
            {
                return;
            }
            #endregion

            #region Change
            var nameExt = name + ".asmdef";
            foreach (var guid in AssetDatabase.FindAssets(name))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(assetPath) != nameExt)
                    continue;

                FileUtil.DeleteFileOrDirectory(assetPath);
                FileUtil.CopyFileOrDirectory(ctx.assetPath, assetPath);

                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                };
                break;
            }
            #endregion
        }
    }
}
#endif
