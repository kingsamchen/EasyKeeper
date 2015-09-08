/*
 @ 0xCCCCCCCC
*/

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EasyKeeper {
    static class SecureStringUtil {
        public static string ConvertToUnsecureString(this SecureString secured)
        {
            if (secured == null) {
                throw new ArgumentNullException(nameof(secured));
            }

            var rawString = IntPtr.Zero;
            try {
                rawString = Marshal.SecureStringToGlobalAllocUnicode(secured);
                return Marshal.PtrToStringUni(rawString);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(rawString);
            }
        }

        public static bool Empty(this SecureString secured)
        {
            return secured.Length == 0;
        }

        public static bool ValueEquals(this SecureString self, SecureString other)
        {
            if (self == null || other == null) {
                throw new ArgumentNullException();
            }

            var selfPtr = IntPtr.Zero;
            var otherPtr = IntPtr.Zero;

            try {
                selfPtr = Marshal.SecureStringToBSTR(self);
                otherPtr = Marshal.SecureStringToBSTR(other);

                // Details leaked by BSTR implementation specification.
                var selfLength = Marshal.ReadInt32(selfPtr, -4);
                var otherLength = Marshal.ReadInt32(otherPtr, -4);
                if (selfLength != otherLength) {
                    return false;
                }

                for (int i = 0; i < selfLength; ++i) {
                    var eq = Marshal.ReadByte(selfPtr, i) == Marshal.ReadByte(otherPtr, i);
                    if (!eq) {
                        return false;
                    }
                }

                return true;
            } finally {
                Marshal.ZeroFreeBSTR(selfPtr);
                Marshal.ZeroFreeBSTR(otherPtr);
            }
        }
    }
}
