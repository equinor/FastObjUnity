using FastObjUnity.Runtime;

namespace FastObjUnity.Editor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using UnityEditor.AssetImporters;
    using UnityEngine;

    // Unity does not allow us to override the default importer, so we have to unfortunately use something else, than
    // obj.
    [ScriptedImporter(1, "obj_fast", AllowCaching = true)]
    public class FastObjImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var stopwatch = Stopwatch.StartNew();
            var nameSet = new HashSet<string>();
            var material = new Material(Shader.Find("Standard"));
            var globalPath = Path.Combine(Application.dataPath, "..", ctx.assetPath);
            var meshes = FastObjConverter.TestFastObj(globalPath);
            var gameObjectName = Path.GetFileName(globalPath);
            var model = new GameObject(gameObjectName);
            var modelTransform = model.transform;
            ctx.AddObjectToAsset(GetFreeNameIdentifier(gameObjectName, nameSet), model);
            foreach (var child in meshes)
                CreateNode(child, modelTransform, ctx, nameSet, material);
            ctx.AddObjectToAsset(GetFreeNameIdentifier("defaultMaterial", nameSet), material);
            ctx.SetMainObject(model);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Import of {ctx.assetPath} completed in {stopwatch.Elapsed}");
        }

        private void CreateNode((string Key, Mesh Value) meshWithName, Transform parent, AssetImportContext ctx,
            HashSet<string> nameSet, Material material)
        {
            var nodeName = string.IsNullOrWhiteSpace(meshWithName.Key) ? "unnamed" : meshWithName.Key;
            var go = new GameObject(nodeName);
            go.transform.SetParent(parent, false);
            ctx.AddObjectToAsset(GetFreeNameIdentifier(nodeName, nameSet), go);
            go.AddComponent<MeshFilter>().sharedMesh = meshWithName.Value;
            ctx.AddObjectToAsset(GetFreeNameIdentifier(nodeName, nameSet), meshWithName.Value);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static string GetFreeNameIdentifier(string initialName, HashSet<string> usedIdentifiers)
        {
            var targetName = initialName;
            var counter = 1;
            while (usedIdentifiers.Contains(targetName))
                targetName = $"{initialName} {counter++}";
            usedIdentifiers.Add(targetName);
            return targetName;
        }
    }
}