using System;
using System.Collections.Generic;
using System.Linq;

namespace CribbageSolitaireSolver
{
    class Solver
    {
        private const int MAX_LEVELS = 26;
        private const byte COLUMN_HEIGHT = 13;

        private Dictionary<GameState, GamePlan> bestScores = new Dictionary<GameState, GamePlan>();
        public long cacheHit = 0;
        public long cacheMiss = 0;

        public float CacheHitRatio { get { return (float)cacheHit / (cacheHit + cacheMiss); } }

        GamePlan zeroPlan = new GamePlan { moves = new Stack<byte>(), score = 0, id = -1 };
        readonly Dictionary<char, byte> inputMap = new Dictionary<char, byte>()
        {
            {'a', 1 },
            {'2', 2 },
            {'3', 3 },
            {'4', 4 },
            {'5', 5 },
            {'6', 6 },
            {'7', 7 },
            {'8', 8 },
            {'9', 9 },
            {'1', 10 },
            {'j', 11 },
            {'q', 12 },
            {'k', 13 },

            // Home row shortcuts
            {'s', 11 },
            {'d', 12 },
            {'f', 13 },
        };

        public GameState GetBenchmarkState()
        {
            GameState state = new GameState();
            state.board = new ulong[4]
            {
                0, 0, 0, 0
            };

            state.board[0] = LongStack.Push(state.board[0], 1);
            state.board[0] = LongStack.Push(state.board[0], 2);
            state.board[0] = LongStack.Push(state.board[0], 3);
            state.board[0] = LongStack.Push(state.board[0], 4);
            state.board[0] = LongStack.Push(state.board[0], 5);
            state.board[0] = LongStack.Push(state.board[0], 4);
            state.board[0] = LongStack.Push(state.board[0], 9);
            state.board[0] = LongStack.Push(state.board[0], 11);

            state.board[1] = LongStack.Push(state.board[1], 10);
            state.board[1] = LongStack.Push(state.board[1], 8);
            state.board[1] = LongStack.Push(state.board[1], 12);
            state.board[1] = LongStack.Push(state.board[1], 13);
            state.board[1] = LongStack.Push(state.board[1], 6);
            state.board[1] = LongStack.Push(state.board[1], 4);
            state.board[1] = LongStack.Push(state.board[1], 10);
            state.board[1] = LongStack.Push(state.board[1], 1);

            state.board[2] = LongStack.Push(state.board[2], 9);
            state.board[2] = LongStack.Push(state.board[2], 2);
            state.board[2] = LongStack.Push(state.board[2], 4);
            state.board[2] = LongStack.Push(state.board[2], 4);
            state.board[2] = LongStack.Push(state.board[2], 6);
            state.board[2] = LongStack.Push(state.board[2], 3);
            state.board[2] = LongStack.Push(state.board[2], 1);
            state.board[2] = LongStack.Push(state.board[2], 12);

            state.board[3] = LongStack.Push(state.board[3], 4);
            state.board[3] = LongStack.Push(state.board[3], 7);
            state.board[3] = LongStack.Push(state.board[3], 6);
            state.board[3] = LongStack.Push(state.board[3], 4);
            state.board[3] = LongStack.Push(state.board[3], 13);
            state.board[3] = LongStack.Push(state.board[3], 11);
            state.board[3] = LongStack.Push(state.board[3], 8);
            state.board[3] = LongStack.Push(state.board[3], 7);

            state.hand = new List<byte>();

            return state;
        }

        /// <summary>
        /// Reproduces a bug that is now fixed. Output should be 10, 10, A, K; Q, Q
        /// </summary>
        /// <returns></returns>
        public GameState GetTestState()
        {
            GameState state = new GameState();
            state.board = new ulong[4]
            {
                0, 0, 0, 0
            };

            state.board[0] = LongStack.Push(state.board[0], 13);
            state.board[0] = LongStack.Push(state.board[0], 1);
            state.board[0] = LongStack.Push(state.board[0], 10);

            state.board[1] = LongStack.Push(state.board[1], 10);

            state.board[3] = LongStack.Push(state.board[3], 12);
            state.board[3] = LongStack.Push(state.board[3], 12);

            state.hand = new List<byte>();

            return state;
        }

        public GameState GetStateFromConsole()
        {
            GameState state = new GameState();

            state.hand = new List<byte>();
            state.board = new ulong[4]
            {
                0, 0, 0, 0
            };

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine(String.Format("Enter column {0} from top to bottom.", i + 1));

                for(int j = 0; j < COLUMN_HEIGHT; j++)
                {
                    char input = Console.ReadKey().KeyChar;
                    state.board[i] = LongStack.Push(state.board[i], inputMap[input]);
                }

                Console.WriteLine();
            }

            Console.WriteLine("OK, I'll think about it!");

            return state;
        }

        public GamePlan EvaluateGame(GameState startingState)
        {
            // Make sure we are not using cache that did not look as far ahead as we want to.
            bestScores.Clear();

            // Find and enqueue possible moves
            return EvaluateState(startingState, 0);

            // Evaluate all moves until the queue is empty
            //while (evaluationQueue.Count > 0)
            //{
            //    GameMove move = evaluationQueue.Dequeue();
            //    EvaluateMove(move);
            //}

            // Find best game plan (using memoization)
            //return possibleScores[startingState];
        }

        public List<byte> GetPossibleMoves(GameState state)
        {
            List<byte> possibleMoves = new List<byte>();

            for (byte column = 0; column < 4; column++)
            {
                if (LongStack.Count(state.board[column]) > 0 && SumHand(state.hand) + CardValue(LongStack.Peek(state.board[column])) <= 31)
                {
                    possibleMoves.Add(column);
                }
            }

            return possibleMoves;
        }

        /// <summary>
        /// Recursively find the best game plan for the given game state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="levels">How deep we have gone down the rabbit hole</param>
        /// <returns></returns>
        public GamePlan EvaluateState(GameState state, int levels)
        {
            if (levels > MAX_LEVELS)
            {
                return zeroPlan;
            }

            // Check for memoization
            if (bestScores.ContainsKey(state))
            {
                cacheHit++;
                return bestScores[state];
            }
            else
            {
                cacheMiss++;
            }

            // There is still room in our hand
            // Try each possible move
            GamePlan bestMove = zeroPlan;

            List<byte> possibleMoves = GetPossibleMoves(state);

            // If no moves were possible, the hand must be cleared
            if (possibleMoves.Count == 0)
            {
                state.hand.Clear();
                state.SetHashCode();

                if (bestScores.ContainsKey(state))
                {
                    cacheHit++;
                    cacheMiss--;
                    return bestScores[state];
                }

                possibleMoves = GetPossibleMoves(state);
            }

            foreach(byte column in possibleMoves)
            {
                // Try move
                GameState moveState = new GameState(state);
                short moveScore = ScoreMove(state.hand, LongStack.Peek(moveState.board[column]));
                moveState.hand.Add(LongStack.Peek(moveState.board[column]));
                moveState.board[column] = LongStack.Pop(moveState.board[column]);

                // Try all possible future moves
                GamePlan futurePlan = EvaluateState(moveState, levels + 1);

                // See if it is best so far
                if (futurePlan.score + moveScore > bestMove.score)
                {
                    // Write down what we did
                    GamePlan movePlan = new GamePlan(futurePlan);
                    movePlan.score += moveScore;
                    movePlan.moves.Push(column);

                    // Save it as the new best
                    bestMove = movePlan;
                }
            }

            // Save for later so we don't have to do this again
            bestScores[state] = bestMove;

            return bestMove;
        }

        /// <summary>
        /// Return the score generated by adding the given card to the hand
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public short ScoreMove(List<byte> hand, byte card)
        {
            short pts = 0;

            // If the first card is a Jack, 2 pts
            if (hand.Count == 0 && card == 11)
            {
                pts += 2;
            }

            // If hand is now 15, 2 pts
            if (SumHand(hand) + CardValue(card) == 15)
            {
                pts += 2;
            }

            // If hand is now 31, 2 pts
            if (SumHand(hand) + CardValue(card) == 31)
            {
                pts += 2;
            }

            // Set of 2, 3, or 4
            if (hand.Count >= 1 && hand[hand.Count - 1] == card)
            {
                // Set of at least 2

                if (hand.Count >= 2 && hand[hand.Count - 2] == card)
                {
                    // Set of at least 3

                    if (hand.Count >= 3 && hand[hand.Count - 3] == card)
                    {
                        // Set of 4! Wow!!
                        pts += 12;
                    }
                    else
                    {
                        // Set of 3
                        pts += 6;
                    }
                }
                else
                {
                    // Set of 2
                    pts += 2;
                }
            }

            for (short run = 7; run >= 3; run--)
            {
                if (hand.Count >= run - 1 && isARun(hand, hand.Count - run + 1, run - 1, card))
                {
                    pts += (short)(run);
                    break;
                }
            }

            return pts;
        }

        /// <summary>
        /// Return true if a run is detected using the given substack and new card
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private bool isARun(List<byte> hand, int index, int count, byte card)
        {
            List<byte> numbers = new List<byte>();

            for (int i = index; i < index + count; i++)
            {
                numbers.Add(hand[i]);
            }
            numbers.Add(card);

            numbers.Sort();

            int n = numbers[0];
            for (int i = 1; i < count + 1; i++)
            {
                if (numbers[i] != n + 1)
                {
                    return false;
                }
                n = numbers[i];
            }
            return true;
        }

        /// <summary>
        /// Add up all the card values in the hand
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public int SumHand(List<byte> hand)
        {
            int sum = 0;

            foreach (byte card in hand)
            {
                sum += CardValue(card);
            }

            return sum;
        }

        /// <summary>
        /// Return the value of the given card (Face cards count as 10)
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public int CardValue(byte card)
        {
            return card > 10 ? 10 : card;
        }


        public void DrawState(GameState state)
        {
            Console.WriteLine();

            for (int y = 0; y < 13; y++)
            {
                // Stack
                Console.Write(state.hand.Count > y ? GetCardName(state.hand[y]).PadLeft(2) : "  ");

                // Gap
                Console.Write("  ");

                // Columns
                for (int column = 0; column < 4; column++)
                {
                    Console.Write(LongStack.Count(state.board[column]) > y ? GetCardName(LongStack.ElementAt(state.board[column], y)).PadLeft(2) : "  ");
                }
                Console.Write("\r\n");
            }
            Console.WriteLine(SumHand(state.hand).ToString());
        }

        public string GetCardName(byte card)
        {
            switch (card)
            {
                case 1:
                    return "A";
                case 11:
                    return "J";
                case 12:
                    return "Q";
                case 13:
                    return "K";
                default:
                    return card.ToString();
            }
        }
    }
}
