using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CribbageSolitaireSolver
{
    class GameState : IEquatable<GameState>
    {
        // 4 columns of the board, acts like a stack. 4 bits per card. Least significant bits are top card.
        // Eg. 
        // A (1)
        // 5
        // Q (12)
        // 0001 0101 1100
        // 348
        public ulong[] board;

        // Hand also uses the same mechanism as board for storage.
        public ulong hand = 0;

        private int hashCode = 0;
        public bool hashCodeSet = false;

        public GameState()
        { }

        public GameState(GameState copyFrom)
        {
            this.board = new ulong[4];
            for (int i = 0; i < 4; i++)
            {
                this.board[i] = copyFrom.board[i];
            }

            this.hand = copyFrom.hand;
        }

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
            if (other == null)
            {
                return false;
            }

            return hand == other.hand;
        }

        public void SetHashCode()
        {
            // With a standard deck, the hand can hold up to 13 cards (A A A A 2 2 2 2 3 3 3 3 4)
            // Once the game is dealt, each column has 14 possible states (0-13 cards)
            hashCode = LongStack.Count(hand) + LongStack.Count(board[0]) * 14 + LongStack.Count(board[1]) * 14 * 14 + LongStack.Count(board[2]) * 14 * 14 * 14 - LongStack.Count(board[3]) * 14 * 14 * 14 * 14;
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
