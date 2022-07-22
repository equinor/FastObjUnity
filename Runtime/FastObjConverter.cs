using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FastObjUnity.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class FastObjConverter
    {
        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr read_obj(string filename);

        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void destroy_mesh(IntPtr mesh);

        public static List<(string, Mesh)> TestFastObj(string filename, bool optimize = false)
        {
            var meshPointer = read_obj(filename);
            if (meshPointer == IntPtr.Zero)
                throw new Exception($"Failed to import {filename}");

            // TODO: non-triangulated meshes support
            // TODO: materials and textures
            var result = new List<(string, Mesh)>();
            var fastObjMesh = Marshal.PtrToStructure<FastObjMesh>(meshPointer);

            // Mirror on X for Unity coordinate system
            var vertexPositions = fastObjMesh.GetPositions();
            var verticeLength = vertexPositions.Length;
            var allVertices = new Vector3[(verticeLength / 3) - 1];
            var vertexIndex = 0;
            for (var i = 3; i < verticeLength; i += 3)
            {
                allVertices[vertexIndex++] = new Vector3(-vertexPositions[i], vertexPositions[i + 1], vertexPositions[i + 2]);
            }

            var meshNormals = fastObjMesh.GetNormals();
            var meshNormalsLength = meshNormals.Length;
            var allNormals = new Vector3[(meshNormalsLength / 3) - 1];
            var meshIndex = 0;
            for (var i = 3; i < meshNormalsLength; i += 3)
            {
                allNormals[meshIndex++] = new Vector3(-meshNormals[i], meshNormals[i + 1], meshNormals[i + 2]);
            }

            // For correct face normals we need to flip second and third vertex in a triangle
            // TODO: this will break on non-triangular meshes
            var vertexCountsPerFace = fastObjMesh.GetFaceVertexCounts();

            if (vertexCountsPerFace.Any(c => c != 3))
                throw new NotImplementedException("Only support triangulated meshes for now");

            var indices = fastObjMesh.GetIndices(vertexCountsPerFace);
            var indicesLength = indices.Length;
            var allIndices = new FastObjIndex[indicesLength];
            var indiceIndex = 0;
            for (var i = 0; i < indicesLength; i += 3)
            {
                allIndices[indiceIndex++] = indices[i];
                allIndices[indiceIndex++] = indices[i + 2];
                allIndices[indiceIndex++] = indices[i + 1];
            }

            var vertexCountsPerFaceLength = vertexCountsPerFace.Length;
            var fastObjGroups = fastObjMesh.GetGroups();
            var meshes = new ConcurrentDictionary<long, MeshContainer>();

            Parallel.ForEach(fastObjGroups, (fastObjGroup, _, index) =>
            {
                // TODO: rebuild indexBuffer for non-triangular meshes
                var faceOffset = fastObjGroup.face_offset;
                var faceCountPlusOffset = fastObjGroup.face_count + faceOffset;
                var indexBufferOffset = 0;
                var indexBufferLength = 0;
                for (var i = 0; i < vertexCountsPerFaceLength; i++)
                {
                    if (i < faceOffset)
                        indexBufferOffset += vertexCountsPerFace[i];
                    else if (i < faceCountPlusOffset)
                        indexBufferLength += vertexCountsPerFace[i];
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

                meshes.TryAdd(index, new MeshContainer
                {
                    IndexFormat = triangleCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16,
                    Vertices = vertices.ToArray(),
                    Normals = normals.ToArray(),
                    Triangles = triangles.ToArray(),
                    Name = fastObjGroup.GetName(),
                });
            });

            for (var i = 0; i < meshes.Count; i++)
            {
                if (!meshes.TryGetValue(i, out var meshContainer))
                    continue;

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
    }

    public class MeshContainer
    {
        public IndexFormat IndexFormat;
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[] Triangles;
        public string Name;
    }
}