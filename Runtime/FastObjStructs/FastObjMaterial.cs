using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FastObjMaterial
    {
        /* ASCII string */
        private readonly IntPtr name;

        /* Ambient */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Ka;
        /* Diffuse */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Kd;
        /* Specular */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Ks;
        /* Emission */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Ke;
        /* Transmittance */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Kt;
        /* Shininess */
        internal float Ns;
        /* Index of refraction */
        internal float Ni;
        /* Transmission filter */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal readonly float[] Tf;
        /* Dissolve (alpha) */
        internal float d;
        /* Illumination model */
        internal int illum;

        internal FastObjTexture map_Ka;
        internal FastObjTexture map_Kd;
        internal FastObjTexture map_Ks;
        internal FastObjTexture map_Ke;
        internal FastObjTexture map_Kt;
        internal FastObjTexture map_Ns;
        internal FastObjTexture map_Ni;
        internal FastObjTexture map_d;
        internal FastObjTexture map_bump;

        public string GetName()
        {
            return Marshal.PtrToStringAnsi(name);
        }
    }
}