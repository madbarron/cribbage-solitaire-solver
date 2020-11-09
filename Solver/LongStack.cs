using System;
using System.Collections.Generic;
using System.Text;

namespace CribbageSolitaireSolver
{
    /// <summary>
    /// Class to perform stack operations, where the elements are nibbles with values 1-13 and the storage is a ulong
    /// </summary>
    public static class LongStack
    {
        // Size of element in bits
        private const byte EL_SIZE = 4;
        private const byte MASK = 15;

        public static ulong Push(ulong stack, byte item)
        {
            stack <<= EL_SIZE;
            stack |= item;
            return stack;
        }

        /// <summary>
        /// Remove the top item from the stack and return the resulting stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static ulong Pop(ulong stack)
        {
            return stack >> EL_SIZE;
        }

        /// <summary>
        /// Return the top item of the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static byte Peek(ulong stack)
        {
            return (byte)(stack & MASK);
        }

        /// <summary>
        /// Return the number of elements in the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public static byte Count(ulong stack)
        {
            byte count = 0;

            // Because there is no 0 card, 0 means empty.
            // check how many nibbles we have to shift before reaching all 0s
            while (stack != 0)
            {
                stack >>= EL_SIZE;
                count++;
            }

            return count;
        }

        public static byte ElementAt(ulong stack, byte index)
        {
            return (byte)((stack >> index * EL_SIZE) & MASK);
        }

        public static byte ElementAt(ulong stack, int index)
        {
            return (byte)((stack >> index * EL_SIZE) & MASK);
        }
    }
}
