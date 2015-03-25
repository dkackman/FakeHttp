using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestHelpers
{
    public class DictionaryComparer<TKey, TValue> : EqualityComparer<IDictionary<TKey, TValue>>
    {
        public DictionaryComparer()
        {
        }

        public override bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
        {
            // early-exit checks
            if (object.ReferenceEquals(x, y))
                return true;

            if (null == x || y == null)
                return false;

            if (x.Count != y.Count)
                return false;

            // check values are the same
            foreach (TKey k in x.Keys)
            {
                // check keys are the same
                if (!y.ContainsKey(k))
                    return false;

                // now check values
                TValue vx = x[k];
                TValue vy = y[k];

                // null checks
                if (vx is IList && vy is IList) // values are both lists, same type and length
                {
                    // both are lists, make sure they are lists of the same type
                    if(vx.GetType() != vy.GetType())
                    {
                        return false;
                    }

                    var lx = vx as IList;
                    var ly = vy as IList;
                    if(lx.Count != ly.Count)
                    {
                        return false;
                    }
                }
                else if (!EqualityComparer<TValue>.Default.Equals(vx, vy)) // then check equality
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode(IDictionary<TKey, TValue> obj)
        {
            if (obj == null)
                return 0;

            int hash = 0;

            foreach (KeyValuePair<TKey, TValue> pair in obj)
            {
                int key = pair.Key.GetHashCode(); // key cannot be null
                int value = pair.Value != null ? pair.Value.GetHashCode() : 0;
                hash ^= ShiftAndWrap(key, 2) ^ value;
            }

            return hash;
        }

        private static int ShiftAndWrap(int value, int positions)
        {
            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer. 
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded. 
            uint wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits. 
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
    }

}
