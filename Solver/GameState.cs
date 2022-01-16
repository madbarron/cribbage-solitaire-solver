using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CribbageSolitaireSolver
{
    class GameState : IEquatable<GameState>
    {
        // 4 values for the height of each board column, each 4 bits of this 16-bit int.
        // 0 means the column is full (0 cards taken)
        // 13 means the column is empty
        public uint boardHeights;

        private static uint boardHeightsMask = 15;
        private uint[] deleteMasks = new uint[4]
        {
            65535 - 15,
            65535 - 255 + 15,
            65535 - 4095 + 255,
            4095
        };

        // Acts like a stack. 4 bits per card. Least significant bits are top card.
        // Eg. 
        // A (1)
        // 5
        // Q (12)
        // 0001 0101 1100
        // 348
        public ulong hand = 0;

        private int hashCode = 0;
        public bool hashCodeSet = false;

        public GameState()
        { }

        public GameState(GameState copyFrom)
        {
            this.boardHeights = copyFrom.boardHeights;
            this.hand = copyFrom.hand;
        }

        public byte GetBoardHeight(int column)
        {
            return (byte)((boardHeights >> column * 4) & boardHeightsMask);
        }

        public void IncrementBoardHeight(int column)
        {
            boardHeights = (boardHeights & deleteMasks[column]) | (uint)((GetBoardHeight(column) + 1) << column * 4);
        }

        // 0110 1010 0011
        // &
        // 1111 0000 1111
        // |
        // 0000 1011 0000
        // =
        // 0110 1011 0011

        /// <summary>
        /// This assumes that the two gamestates are for the same game.
        /// Game states for different board layouts could easily produce false positives.
        ///
        /// This also assumes that GetHashCode() for both objects returns the same hash.
        /// If hashes are the same, assuming a standard deck and 4 columns,
        /// Then the boards are the same and the hands have the same number of cards.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals([AllowNull] GameState other)
        {
            return other != null && hand == other.hand;
        }

        public void SetHashCode()
        {
            // With a standard deck, the hand can hold up to 13 cards (A A A A 2 2 2 2 3 3 3 3 4)
            // Once the game is dealt, each column has 14 possible states (0-13 cards)
            hashCode = (LongStack.Count(hand) + GetBoardHeight(0) * 14 + GetBoardHeight(1) * 14 * 14 + GetBoardHeight(2) * 14 * 14 * 14 - GetBoardHeight(3) * 14 * 14 * 14 * 14);
            hashCodeSet = true;
        }

        /// <summary>
        /// This class assumes that it is not mutated once this method is called.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (!hashCodeSet)
            {
                SetHashCode();
            }
            return hashCode;
        }
    }
}
