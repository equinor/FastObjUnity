using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjGroup
    {
        private readonly IntPtr name; // ASCII string
        public readonly uint face_count; // Number of faces
        public readonly uint face_offset; // First face in fastObjMesh face_* arrays
        public readonly uint index_offset; // First index in fastObjMesh indices array

        public string GetName()
        {
            return Marshal.PtrToStringAnsi(name);
        }
    }
}
