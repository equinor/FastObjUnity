using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjTexture
    {
        private readonly IntPtr name; // Texture name from .mtl file
        private readonly IntPtr path; // Resolved path to texture

        public string GetName()
        {
            return Marshal.PtrToStringAnsi(name);
        }

        public string GetPath()
        {
            return Marshal.PtrToStringAnsi(path);
        }
    }
}
