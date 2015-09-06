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

            IntPtr rawString = IntPtr.Zero;
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
    }
}
