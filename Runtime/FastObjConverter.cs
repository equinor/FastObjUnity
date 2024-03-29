// #define FORCE_PARALLEL_IMPORT // Uncomment to force use of parallel import even outside Unity Editor

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR || FORCE_PARALLEL_IMPORT
using System.Threading.Tasks;
#endif

namespace FastObjUnity.Runtime
{
    public class FastObjConverter
    {
        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr read_obj(string filename);

        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void destroy_mesh(IntPtr mesh);

        // TODO: non-triangulated meshes support
        // TODO: materials and textures
        public static List<(string, Mesh)> ImportFastObj(string filename, bool optimize = true)
        {
            var meshPointer = read_obj(filename);
            if (meshPointer == IntPtr.Zero)
                throw new Exception($"Failed to import {filename}. Meshpointer is IntPtr.Zero");

            var fastObjMesh = Marshal.PtrToStructure<FastObjMesh>(meshPointer);

            // For correct face normals we need to flip second and third vertex in a triangle
            // TODO: this will break on non-triangular meshes
            var faceVertices = fastObjMesh.GetFaceVertices();
            if (faceVertices.Any(c => c != 3))
                throw new NotImplementedException($"Failed to import {filename}. Only support triangulated meshes for now");

            // Mirror on X for Unity coordinate system
            var positions = fastObjMesh.GetPositions();
            var positionsLength = positions.Length;
            var allVertices = new Vector3[(positionsLength / 3) - 1];
            var vertexIndex = 0;
            for (var i = 3; i < positionsLength; i += 3)
            {
                allVertices[vertexIndex++] = new Vector3(-positions[i], positions[i + 1], positions[i + 2]);
            }

            var meshNormals = fastObjMesh.GetNormals();
            var meshNormalsLength = meshNormals.Length;
            var allNormals = new Vector3[(meshNormalsLength / 3) - 1];
            var meshIndex = 0;
            for (var i = 3; i < meshNormalsLength; i += 3)
            {
                allNormals[meshIndex++] = new Vector3(-meshNormals[i], meshNormals[i + 1], meshNormals[i + 2]);
            }

            var indices = fastObjMesh.GetIndices();
            var indicesLength = indices.Length;
            var allIndices = new FastObjIndex[indicesLength];
            var indiceIndex = 0;
            for (var i = 0; i < indicesLength; i += 3)
            {
                allIndices[indiceIndex++] = indices[i];
                allIndices[indiceIndex++] = indices[i + 2];
                allIndices[indiceIndex++] = indices[i + 1];
            }

            var faceVerticesLength = faceVertices.Length;
            var fastObjGroups = fastObjMesh.GetGroups();
            var meshes = new ConcurrentDictionary<long, MeshContainer>();

            // NOTE: Parallel.ForEach is only used in the editor because we did not bother testing it on device. It might be OK to have this enabled at runtime as well.
#if UNITY_EDITOR || FORCE_PARALLEL_IMPORT
            Parallel.ForEach(fastObjGroups, (fastObjGroup, _, index) =>
            {
#else
            for (var index = 0; index < fastObjGroups.Length; index++)
            {
                var fastObjGroup = fastObjGroups[index];
#endif
                // TODO: rebuild indexBuffer for non-triangular meshes
                var faceOffset = fastObjGroup.face_offset;
                var faceCountPlusOffset = fastObjGroup.face_count + faceOffset;
                var indexBufferOffset = 0;
                var indexBufferLength = 0;
                for (var i = 0; i < faceVerticesLength; i++)
                {
                    if (i < faceOffset)
                        indexBufferOffset += faceVertices[i];
                    else if (i < faceCountPlusOffset)
                        indexBufferLength += faceVertices[i];
                    else
                        break;
                }

                var triangleCount = 0;
                var verticeCount = 0;
                var indexBufferOffsetPlusLength = indexBufferOffset + indexBufferLength;
                var vertices = new List<Vector3>(indexBufferLength);
                var normals = new List<Vector3>(indexBufferLength);
                var triangles = new List<int>(indexBufferLength);
                var vertexMap = new Dictionary<FastObjIndex, int>(indexBufferLength);
                for (var j = indexBufferOffset; j < indexBufferOffsetPlusLength; j++)
                {
                    var facePoint = allIndices[j];
                    if (!vertexMap.TryGetValue(facePoint, out var key))
                    {
                        key = verticeCount;
                        vertexMap.Add(facePoint, key);
                        vertices.Add(allVertices[facePoint.p - 1]);
                        normals.Add(allNormals[facePoint.n - 1]);
                        verticeCount++;
                    }

                    triangles.Add(key);
                    triangleCount++;
                }

                var meshContainer = new MeshContainer
                {
                    IndexFormat = triangleCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                    Vertices = vertices.ToArray(),
                    Normals = normals.ToArray(),
                    Triangles = triangles.ToArray(),
                    Name = fastObjGroup.GetName(),
                };

                if (!meshes.TryAdd(index, meshContainer))
                    Debug.LogError($"Mesh index {index} for {meshContainer.Name} already added. Discarding new value.");

#if UNITY_EDITOR || FORCE_PARALLEL_IMPORT
            });
#else
            }
#endif

            var result = new List<(string, Mesh)>();
            for (var i = 0; i < meshes.Count; i++)
            {
                if (!meshes.TryGetValue(i, out var meshContainer))
                    throw new KeyNotFoundException($"Failed to import {filename}. Given index {i} did not exist in {nameof(meshes)} when acquiring {nameof(meshContainer)}.");

                var unityMesh = new Mesh
                {
                    indexFormat = meshContainer.IndexFormat,
                    vertices = meshContainer.Vertices,
                    normals = meshContainer.Normals,
                    triangles = meshContainer.Triangles
                };

                if (optimize)
                {
                    unityMesh.Optimize();
                }

                unityMesh.RecalculateBounds();
                result.Add((meshContainer.Name, unityMesh));
            }

            destroy_mesh(meshPointer);
            return result;
        }

        private class MeshContainer
        {
            public IndexFormat IndexFormat;
            public Vector3[] Vertices;
            public Vector3[] Normals;
            public int[] Triangles;
            public string Name;
        }
    }
}
