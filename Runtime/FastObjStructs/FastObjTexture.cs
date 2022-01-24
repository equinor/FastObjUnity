using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FastObjTexture
    {
        /* ASCII string */
        private readonly IntPtr name;
        /* ASCII string */
        private readonly IntPtr path;
        
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