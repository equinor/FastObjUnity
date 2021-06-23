using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace FastObjUnity
{
    public class FastObjConverter
    {
        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr read_obj(string filename);

        [DllImport("fast_obj_unity", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void destroy_mesh(IntPtr mesh);

        
        public static List<(string, Mesh)> TestFastObj(string filename)
        {
            var meshPointer = read_obj(filename);
            if (meshPointer == IntPtr.Zero) 
                throw new Exception($"Failed to import {filename}");
            
            // TODO: non-triangulated meshes support
            // TODO: materials and textures
            var result = new List<(string, Mesh)>();
            var fastObjMesh = Marshal.PtrToStructure<FastObjMesh>(meshPointer);
            var fastObjGroups = fastObjMesh.GetGroups();
            // Mirror on X for Unity coordinate system
            var allVertices  = fastObjMesh.GetPositions().Skip(3).Select((p, i) => (p, i))
                .GroupBy(pi => pi.i / 3).Select(g => g.Select(pi => pi.p).ToArray())
                .Select(a => new Vector3(- a[0], a[1], a[2])).ToArray();
            var allNormals = fastObjMesh.GetNormals().Skip(3).Select((n, i) => (n, i))
                .GroupBy(ni => ni.i / 3).Select(g => g.Select(ni => ni.n).ToArray())
                .Select(a => new Vector3(- a[0], a[1], a[2])).ToArray();
            var vertexCountsPerFace = fastObjMesh.GetFaceVertexCounts();
            // For correct face normals we need to flip second and third vertex in a triangle
            // TODO: this will break on non-triangular meshes
            var allIndices = fastObjMesh.GetIndices(vertexCountsPerFace).Select((f, i) => (f, i)).GroupBy(fi => fi.i / 3)
                .Select(g => g.Select(fi => fi.f).ToArray())
                .SelectMany(f => new []{f[0], f[2], f[1]}).ToArray();

            if (vertexCountsPerFace.Any(c => c != 3))
                throw new NotImplementedException("Only support triangulated meshes for now");
                
            foreach (var fastObjGroup in fastObjGroups)
            {
                var name = fastObjGroup.GetName();
                var vcpf = vertexCountsPerFace.Skip((int) fastObjGroup.face_offset).Take((int) fastObjGroup.face_count).ToArray();
                var indexBufferOffset = vertexCountsPerFace.Take((int)fastObjGroup.face_offset).Sum();
                var indexBufferLength = vcpf.Sum();
                var indexBuffer = allIndices.Skip(indexBufferOffset).Take(indexBufferLength).ToArray();
                
                // TODO: rebuild indexBuffer for non-triangular meshes

                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var triangles = new List<int>();
                var vertexMap = new Dictionary<FastObjIndex, int>();

                foreach (var facePoint in indexBuffer)
                {
                    if (!vertexMap.TryGetValue(facePoint, out var key))
                    {
                        key = vertices.Count;
                        vertexMap.Add(facePoint, key);
                        vertices.Add(allVertices[facePoint.p - 1]);
                        normals.Add(allNormals[facePoint.n - 1]);
                    }
                    triangles.Add(key);
                }
                
                var mesh = new Mesh();
                mesh.indexFormat = triangles.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateBounds();
                result.Add((name, mesh));
            }
                
            destroy_mesh(meshPointer);
            return result;
        }
    }
}