using System;
using System.Linq;
using System.Runtime.InteropServices;
using FastObjUnity.Utils;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjMesh
    {
        private readonly uint position_count;
        private readonly IntPtr positions; // floats

        private readonly uint texcoord_count;
        private readonly IntPtr texcoords; // floats

        private readonly uint normal_count;
        private readonly IntPtr normals; // floats


        private readonly uint face_count;
        private readonly IntPtr face_vertices; // ints
        private readonly IntPtr face_materials; // ints


        private readonly IntPtr indices; // structs


        private readonly uint material_count;
        private readonly IntPtr materials; // structs


        private readonly uint group_count;
        private readonly IntPtr groups; // structs

        public float[] GetPositions()
        {
            return MarshalHelper.RetrieveFloats(positions, (int) position_count * 3);
        }

        public float[] GetTexCoords()
        {
            return MarshalHelper.RetrieveFloats(texcoords, (int) texcoord_count * 2);
        }

        public float[] GetNormals()
        {
            return MarshalHelper.RetrieveFloats(normals, (int) normal_count * 3);
        }

        public int[] GetFaceVertexCounts()
        {
            return MarshalHelper.RetrieveInts(face_vertices, (int) face_count);
        }

        public int[] GetFaceMaterials()
        {
            return MarshalHelper.RetrieveInts(face_materials, (int) face_count);
        }

        public FastObjIndex[] GetIndices(int[] vertexCountsPerFace)
        {
            var totalIndicies = vertexCountsPerFace.Sum();
            return MarshalHelper.RetrieveStructs<FastObjIndex>(indices, totalIndicies);
        }

        public FastObjGroup[] GetGroups()
        {
            return MarshalHelper.RetrieveStructs<FastObjGroup>(groups, (int)group_count);
        }
    }
}