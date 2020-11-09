using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CribbageSolitaireSolver
{
    class GameState: IEquatable<GameState>
    {
        public static long numEquals = 0;
        public Stack<byte>[] board;
        public List<byte> hand;
        public ulong handBinary;

        private int hashCode = 0;
        public bool hashCodeSet = false;

        public GameState()
        {

        }

        public GameState(GameState copyFrom)
        {
            this.board = new Stack<byte>[4];
            for (int i = 0; i < 4; i++)
            {
                this.board[i] = new Stack<byte>(copyFrom.board[i].Reverse<byte>());
            }

            this.hand = new List<byte>(copyFrom.hand);
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

            // Compare the first 16 cards. (only 13 cards per hand are possible given a standard deck)
            return handBinary == other.handBinary;
        }

        public void SetHashCode()
        {
            // With a standard deck, the hand can hold up to 13 cards (A A A A 2 2 2 2 3 3 3 3 4)
            // Once the game is dealt, each column has 14 possible states (0-13 cards)
            hashCode = hand.Count() + board[0].Count * 14 + board[1].Count * 14 * 14 + board[2].Count * 14 * 14 * 14 - board[3].Count * 14 * 14 * 14 * 14;
            SetHandBinary();
            hashCodeSet = true;
        }

        /// <summary>
        /// Each card is a nibble 1-13.
        /// This stores up to 16 cards in a long int.
        /// </summary>
        private void SetHandBinary()
        {
            handBinary = 0;

            foreach(byte card in hand)
            {
                handBinary <<= 4;
                handBinary |= card;
            }
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

    struct GamePlan
    {
        public static long nextId = 1;

        public Stack<byte> moves;
        public short score;
        public long id;

        public GamePlan(GamePlan copyFrom)
        {
            this.moves = new Stack<byte>(copyFrom.moves.Reverse<byte>());
            this.score = copyFrom.score;
            id = GamePlan.nextId++;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Solver solver = new Solver();

            // Get starting state from keyboard
            //GameState state = solver.GetStateFromConsole();
            GameState state = solver.GetBenchmarkState();
            //GameState state = solver.GetTestState();

            GamePlan plan = solver.EvaluateGame(state);

            Console.WriteLine(String.Format("Equality was evaluated {0} times.", GameState.numEquals));
            Console.WriteLine(String.Format("Cache hit: {0} / {1} = {2}", solver.cacheHit, solver.cacheHit + solver.cacheMiss, solver.CacheHitRatio));

            GameState playState = state;

            while (plan.score > 0)
            {
                playState = new GameState(playState);

                // Display plan to user
                while (plan.moves.Count > 0)
                {
                    byte move = plan.moves.Pop();
                    byte card = playState.board[move].Peek();
                    if (solver.SumStack(playState.hand) + solver.CardValue(card) > 31)
                    {
                        playState.hand.Clear();
                        Console.WriteLine("Clear.");
                        break;
                    }
                    short points = solver.ScoreMove(playState.hand, card);
                    playState.hand.Add(playState.board[move].Pop());

                    Console.WriteLine(String.Format("Take the {0} from column {1}. {2} points.", solver.GetCardName(card), move + 1, points));
                }

                // Plan ahead
                plan = solver.EvaluateGame(playState);
                if (plan.score > 0)
                {
                    Console.WriteLine("...");
                    Console.ReadLine();
                    Console.Clear();
                }                
            }
        }
    }
}
