namespace FastObjUnity
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
            var nameSet = new HashSet<string>();
            var stopwatch = Stopwatch.StartNew();
            var material = new Material(Shader.Find("Standard"));
            var globalPath = Path.Combine(Application.dataPath, "..", ctx.assetPath);
            var meshes = FastObjConverter.TestFastObj(globalPath);
            var gameObjectName = Path.GetFileName(globalPath);
            var model = new GameObject(gameObjectName);
            ctx.AddObjectToAsset(GetFreeNameIdentifier(gameObjectName, nameSet), model);
            foreach (var child in meshes)
                CreateNode(child, model, ctx, nameSet, material);
            ctx.AddObjectToAsset(GetFreeNameIdentifier("defaultMaterial", nameSet), material);
            ctx.SetMainObject(model);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Import of {ctx.assetPath} completed in {stopwatch.Elapsed}");
        }

        private void CreateNode((string Key, Mesh Value) meshWithName, GameObject model, AssetImportContext ctx,
            HashSet<string> nameSet, Material material)
        {
            var go = new GameObject(meshWithName.Key);
            go.transform.SetParent(model.transform);
            ctx.AddObjectToAsset(GetFreeNameIdentifier(meshWithName.Key, nameSet), go);
            go.AddComponent<MeshFilter>().sharedMesh = meshWithName.Value;
            ctx.AddObjectToAsset(GetFreeNameIdentifier(meshWithName.Key, nameSet), meshWithName.Value);
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