using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjGroup
    {
        /* ASCII string */
        private readonly IntPtr name;
        /* Number of faces */
        internal readonly uint face_count;
        /* First face in fastObjMesh face_* arrays */
        internal readonly uint face_offset;
        /* First index in fastObjMesh indices array */
        internal readonly uint index_offset;

        public string GetName()
        {
            return Marshal.PtrToStringAnsi(name);
        }
    }
}