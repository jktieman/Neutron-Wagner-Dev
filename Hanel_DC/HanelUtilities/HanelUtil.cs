using System;
using System.Diagnostics;

namespace Hanel_DC.HanelUtilities
{
    internal class HanelUtil
    {
        private static int _objectId;
        private static readonly object ObjectIdLock = new object();

        public static string GetUniqueObjectIdentifier()
        {
            lock (ObjectIdLock)
            {
                if (_objectId < 1000 || _objectId >= 9999)
                    _objectId = 1000;
                ++_objectId;
            }
            return _objectId.ToString();
        }

        public static string FixedLength(string callersString, int fixedLength) => callersString.PadRight(fixedLength).ToUpper().Substring(0, fixedLength);

        public static string StrZero(int myInt, int totalLength)
        {
            string str = "0".PadLeft(totalLength, '0') + myInt.ToString();
            return str.Substring(str.Length - totalLength, totalLength);
        }

        public static int PutInRange(int value, int min, int max) => Math.Max(Math.Min(value, max), min);

        public static string GetCurrentMethodName() => GetMethodName();

        public static string GetMethodName(int howFarBack = 1)
        {
            string methodName = "";
            if (howFarBack >= 0)
            {
                StackTrace stackTrace = new StackTrace();
                if (howFarBack < stackTrace.FrameCount)
                    methodName = stackTrace.GetFrame(howFarBack).GetMethod().Name;
            }
            return methodName;
        }
    }
}
