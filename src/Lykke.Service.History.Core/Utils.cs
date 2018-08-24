using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.History.Core
{
    public class Utils
    {
        public static Guid IncrementGuid(Guid guid, int inc)
        {
            return new Guid(IncrementByteArray(guid.ToByteArray(), inc));
        }

        public static byte[] IncrementByteArray(byte[] array, int inc)
        {
            if (inc < 0)
                throw new ArgumentOutOfRangeException(nameof(inc));

            var index = array.Length - 1;
            while (index >= 0)
            {
                var diff = 255 - array[index];
                if (diff >= inc)
                {
                    array[index] += (byte)inc;
                    break;
                }
                inc -= diff;
                array[index--] = 0;
            }
            return array;
        }
    }
}
