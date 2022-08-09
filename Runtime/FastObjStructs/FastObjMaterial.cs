using System;
using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjMaterial
    {
        private readonly IntPtr name; // Material name
        public readonly float[] Ka; // Ambient
        public readonly float[] Kd; // Diffuse
        public readonly float[] Ks; // Specular
        public readonly float[] Ke; // Emission
        public readonly float[] Kt; // Transmittance
        public readonly float Ns; // Shininess
        public readonly float Ni; // Index of refraction
        public readonly float[] Tf; // Transmission filter
        public readonly float d; // Disolve (alpha)
        public readonly float illum; // Illumination model

        // Texture maps
        public readonly FastObjTexture map_Ka;
        public readonly FastObjTexture map_Kd;
        public readonly FastObjTexture map_Ks;
        public readonly FastObjTexture map_Ke;
        public readonly FastObjTexture map_Kt;
        public readonly FastObjTexture map_Ns;
        public readonly FastObjTexture map_Ni;
        public readonly FastObjTexture map_d;
        public readonly FastObjTexture map_bump;

        public string GetName()
        {
            return Marshal.PtrToStringAnsi(name);
        }
    }
}
