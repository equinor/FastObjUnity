using System.Runtime.InteropServices;

namespace FastObjUnity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct FastObjIndex
    {
        public readonly uint p;
        public readonly uint t;
        public readonly uint n;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) p;
                hashCode = (hashCode * 397) ^ (int) t;
                hashCode = (hashCode * 397) ^ (int) n;
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FastObjIndex other)
            {
                return p == other.p && t == other.t && n == other.n;
            }
            return false;
        }
    }
}
