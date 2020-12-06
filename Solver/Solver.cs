using System;
using System.Collections.Generic;
using System.Linq;

namespace CribbageSolitaireSolver
{
    class Solver
    {
        private const int MAX_LEVELS = 28;
        private const byte COLUMN_HEIGHT = 13;

        private Dictionary<GameState, GamePlan> bestScores = new Dictionary<GameState, GamePlan>(2000000);
        public long cacheHit = 0;
        public long cacheMiss = 0;

        private List<byte>[] isRunLists = new List<byte>[5]
            {
                new List<byte>(3),
                new List<byte>(4),
                new List<byte>(5),
                new List<byte>(6),
                new List<byte>(7)
            };

        private ulong[] board;

        public float CacheHitRatio { get { return (float)cacheHit / (cacheHit + cacheMiss); } }

        internal Dictionary<GameState, GamePlan> BestScores { get => bestScores; }

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
            
            board = new ulong[4]
            {
                0, 0, 0, 0
            };

            board[0] = LongStack.Push(board[0], 13);
            board[0] = LongStack.Push(board[0], 6);
            board[0] = LongStack.Push(board[0], 3);
            board[0] = LongStack.Push(board[0], 2);
            board[0] = LongStack.Push(board[0], 8);
            board[0] = LongStack.Push(board[0], 8);
            board[0] = LongStack.Push(board[0], 1);
            board[0] = LongStack.Push(board[0], 11);
            board[0] = LongStack.Push(board[0], 12);
            board[0] = LongStack.Push(board[0], 5);
            board[0] = LongStack.Push(board[0], 10);
            board[0] = LongStack.Push(board[0], 12);
            board[0] = LongStack.Push(board[0], 10);

            board[1] = LongStack.Push(board[1], 5);
            board[1] = LongStack.Push(board[1], 11);
            board[1] = LongStack.Push(board[1], 7);
            board[1] = LongStack.Push(board[1], 6);
            board[1] = LongStack.Push(board[1], 5);
            board[1] = LongStack.Push(board[1], 2);
            board[1] = LongStack.Push(board[1], 6);
            board[1] = LongStack.Push(board[1], 13);
            board[1] = LongStack.Push(board[1], 3);
            board[1] = LongStack.Push(board[1], 9);
            board[1] = LongStack.Push(board[1], 8);
            board[1] = LongStack.Push(board[1], 1);
            board[1] = LongStack.Push(board[1], 2);

            board[2] = LongStack.Push(board[2], 6);
            board[2] = LongStack.Push(board[2], 3);
            board[2] = LongStack.Push(board[2], 1);
            board[2] = LongStack.Push(board[2], 3);
            board[2] = LongStack.Push(board[2], 5);
            board[2] = LongStack.Push(board[2], 9);
            board[2] = LongStack.Push(board[2], 11);
            board[2] = LongStack.Push(board[2], 10);
            board[2] = LongStack.Push(board[2], 13);
            board[2] = LongStack.Push(board[2], 4);
            board[2] = LongStack.Push(board[2], 7);
            board[2] = LongStack.Push(board[2], 9);
            board[2] = LongStack.Push(board[2], 9);

            board[3] = LongStack.Push(board[3], 4);
            board[3] = LongStack.Push(board[3], 7);
            board[3] = LongStack.Push(board[3], 12);
            board[3] = LongStack.Push(board[3], 12);
            board[3] = LongStack.Push(board[3], 2);
            board[3] = LongStack.Push(board[3], 1);
            board[3] = LongStack.Push(board[3], 13);
            board[3] = LongStack.Push(board[3], 7);
            board[3] = LongStack.Push(board[3], 10);
            board[3] = LongStack.Push(board[3], 4);
            board[3] = LongStack.Push(board[3], 8);
            board[3] = LongStack.Push(board[3], 4);
            board[3] = LongStack.Push(board[3], 11);

            return state;
        }

        /// <summary>
        /// Reproduces a bug that is now fixed. Output should be 10, 10, A, K; Q, Q
        /// </summary>
        /// <returns></returns>
        public GameState GetTestState()
        {
            GameState state = new GameState();
            board = new ulong[4]
            {
                0, 0, 0, 0
            };

            board[0] = LongStack.Push(board[0], 13);
            board[0] = LongStack.Push(board[0], 1);
            board[0] = LongStack.Push(board[0], 10);

            board[1] = LongStack.Push(board[1], 10);

            board[3] = LongStack.Push(board[3], 12);
            board[3] = LongStack.Push(board[3], 12);

            return state;
        }

        public GameState GetStateFromConsole()
        {
            GameState state = new GameState();

            board = new ulong[4]
            {
                0, 0, 0, 0
            };

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine(String.Format("Enter column {0} from top to bottom.", i + 1));

                for(int j = 0; j < COLUMN_HEIGHT; j++)
                {
                    char input = Console.ReadKey().KeyChar;
                    board[i] = LongStack.Push(board[i], inputMap[input]);
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

        /// <summary>
        /// Return the list of board columns 0-3 from which a card can be drawn to the current hand
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<byte> GetPossibleMoves(GameState state)
        {
            List<byte> possibleMoves = new List<byte>();

            for (byte column = 0; column < 4; column++)
            {
                if (COLUMN_HEIGHT - state.GetBoardHeight(column) > 0 && SumHand(state.hand) + CardValue(GetCard(column, state)) <= 31)
                {
                    possibleMoves.Add(column);
                }
            }

            return possibleMoves;
        }

        public byte GetCard(int columnIndex, GameState state)
        {
            return LongStack.ElementAt(board[columnIndex], state.GetBoardHeight(columnIndex));
        }

        /// <summary>
        /// Recursively find the best game plan for the given game state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="levels">How deep we have gone down the rabbit hole</param>
        /// <returns></returns>
        public GamePlan EvaluateState(GameState state, int levels)
        {
            // There is still room in our hand
            // Try each possible move

            List<byte> possibleMoves = GetPossibleMoves(state);

            // If no moves were possible, the hand must be cleared
            if (possibleMoves.Count == 0)
            {
                state.hand = 0;
                state.SetHashCode();

                if (bestScores.ContainsKey(state))
                {
                    cacheHit++;
                    return bestScores[state];
                }

                possibleMoves = GetPossibleMoves(state);
            }
            else
            {
                if (bestScores.ContainsKey(state))
                {
                    cacheHit++;
                    return bestScores[state];
                }
            }

            cacheMiss++;

            GamePlan bestMove = zeroPlan;
            GameState moveState;
            byte moveScore;
            byte moveCard;

            foreach(byte column in possibleMoves)
            {
                // Try move
                moveState = new GameState(state);
                moveCard = GetCard(column, moveState);
                moveScore = ScoreMove(state.hand, moveCard);
                moveState.hand = LongStack.Push(moveState.hand, moveCard);
                moveState.IncrementBoardHeight(column);                

                // Try all possible future moves
                GamePlan futurePlan = (levels == MAX_LEVELS) ? zeroPlan : EvaluateState(moveState, levels + 1);

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
        public byte ScoreMove(ulong hand, byte card)
        {
            byte pts = 0;
            byte count = LongStack.Count(hand);

            // If the first card is a Jack, 2 pts
            if (count == 0 && card == 11)
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
            if (count >= 1 && LongStack.ElementAt(hand, 0) == card)
            {
                // Set of at least 2

                if (count >= 2 && LongStack.ElementAt(hand, 1) == card)
                {
                    // Set of at least 3

                    if (count >= 3 && LongStack.ElementAt(hand, 2) == card)
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

            for (byte run = 7; run >= 3; run--)
            {
                if (count >= run - 1 && isARun(hand, run - 1, card))
                {
                    pts += run;
                    break;
                }
            }

            return pts;
        }

        /// <summary>
        /// Return true if a run is detected using the given substack and new card
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="count"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private bool isARun(ulong hand, int count, byte card)
        {
            List<byte> numbers = isRunLists[count - 2];
            numbers.Clear();

            for (int i = 0; i < count; i++)
            {
                numbers.Add(LongStack.Peek(hand));
                hand = LongStack.Pop(hand);
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
        public int SumHand(ulong hand)
        {
            int sum = 0;

            for (int i = LongStack.Count(hand) - 1; i >= 0; i--)
            {
                sum += CardValue(LongStack.ElementAt(hand, i));
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

            for (byte y = 0; y < 13; y++)
            {
                // Stack
                Console.Write(LongStack.Count(state.hand) > y ? GetCardName(LongStack.ElementAt(state.hand, y)).PadLeft(2) : "  ");

                // Gap
                Console.Write("  ");

                // Columns
                for (int column = 0; column < 4; column++)
                {
                    Console.Write(COLUMN_HEIGHT - state.GetBoardHeight(column) > y ? GetCardName(LongStack.ElementAt(board[column], y)).PadLeft(2) : "  ");
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
