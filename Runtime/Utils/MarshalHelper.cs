using System;
using System.Runtime.InteropServices;

namespace FastObjUnity.Utils
{
    internal static class MarshalHelper
    {
        internal static float[] RetrieveFloats(IntPtr floatArray, int length)
        {
            var toReturn = new float[length];
            Marshal.Copy(floatArray, toReturn, 0, length);
            return toReturn;
        }

        internal static int[] RetrieveInts(IntPtr intArray, int length)
        {
            var toReturn = new int[length];
            Marshal.Copy(intArray, toReturn, 0, length);
            return toReturn;
        }
        
        internal static T[] RetrieveStructs<T>(IntPtr structArrayPtr, int length) where T:struct
        {
            var result = new T[length];
            var pos = structArrayPtr;
            var step = Marshal.SizeOf<T>();
            for (var i = 0; i < length; i++)
            {
                result[i] = Marshal.PtrToStructure<T>(pos);
                pos += step;
            }

            return result;
        }
    }
}