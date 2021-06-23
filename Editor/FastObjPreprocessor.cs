using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#if !FASTOBJ_AUTORENAME
namespace FastObjUnity.Editor
{
    public class FastObjPreprocessor : AssetPostprocessor
    {
        public void OnPreprocessModel()
        {
            if (assetPath == null)
                return;    var fileExtension = Path.GetExtension(assetPath);
            if (fileExtension.Equals(".obj", StringComparison.OrdinalIgnoreCase)
                && assetImporter.importSettingsMissing)
            {
                var fileBaseName = Path.GetFileNameWithoutExtension(assetPath);
                var directory = Path.GetDirectoryName(assetPath);
                var target = CreateNonConflictingTargetPath(directory, fileBaseName);
                File.Move(assetPath, target);
                File.Delete(assetPath + ".meta"); // Remove temp metafile.
                Debug.LogWarning($"!!!!!!!!{nameof(FastObjPreprocessor)} intentionally renamed {assetPath} to \"{Path.GetFileName(target)}\". To support Fast_Obj reading. The ImportFbxError and missing meta warning should be ignored.");
            }
        }
        
        private static string CreateNonConflictingTargetPath(string directory, string fileBaseName)
        {
            var target = CreateTargetFileName(directory, fileBaseName); // bla/bla/mesh.obj_fast
            var i = 1;
            while (File.Exists(target))
            {
                target = CreateTargetFileName(directory, fileBaseName, $" {i++}");
            }    return target;
        }
        
        private static string CreateTargetFileName(string directory, string fileBaseName, string fileBaseNameSuffix = "")
        {
            return Path.Combine(directory, fileBaseName + fileBaseNameSuffix + ".obj_fast");
        }
    }

}
#endif