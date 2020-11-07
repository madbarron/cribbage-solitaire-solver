using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CribbageSolitaireSolver
{
    class Solver
    {
        private const int MAX_LEVELS = 12;
        private const byte COLUMN_HEIGHT = 5;

        private Dictionary<GameState, GamePlan> bestScores = new Dictionary<GameState, GamePlan>();

        GamePlan zeroPlan = new GamePlan { moves = new Stack<byte>(), score = 0, id = -1 };

        Dictionary<char, byte> inputMap = new Dictionary<char, byte>()
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
            {'k', 13 }
        };

        public GameState GetStartingState()
        {
            GameState state = new GameState();
            state.board = new Stack<byte>[4]
            {
                new Stack<byte>(),
                new Stack<byte>(),
                new Stack<byte>(),
                new Stack<byte>()
            };

            state.board[0].Push(1);
            state.board[0].Push(2);
            state.board[0].Push(3);
            state.board[0].Push(4);

            state.board[1].Push(10);
            state.board[1].Push(8);
            state.board[1].Push(12);
            state.board[1].Push(13);

            state.board[2].Push(9);
            state.board[2].Push(2);
            state.board[2].Push(4);
            state.board[2].Push(4);

            state.board[3].Push(4);
            state.board[3].Push(7);
            state.board[3].Push(6);
            state.board[3].Push(4);

            state.stack = new List<byte>();

            return state;
        }

        public GameState GetStateFromConsole()
        {
            GameState state = new GameState();

            state.stack = new List<byte>();
            state.board = new Stack<byte>[4]
            {
                new Stack<byte>(),
                new Stack<byte>(),
                new Stack<byte>(),
                new Stack<byte>()
            };

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine(String.Format("Enter column {0} from top to bottom.", i + 1));

                for(int j = 0; j < COLUMN_HEIGHT; j++)
                {
                    char input = Console.ReadKey().KeyChar;
                    state.board[i].Push(inputMap[input]);
                }

                Console.WriteLine();
            }

            Console.WriteLine("OK, I'll think about it!");

            return state;
        }

        public GamePlan EvaluateGame(GameState startingState)
        {
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
                if (state.board[column].Count > 0 && SumStack(state.stack) + state.board[column].Peek() <= 31)
                {
                    possibleMoves.Add(column);
                }
            }

            return possibleMoves;
        }

        public GamePlan EvaluateState(GameState state, int levels)
        {
            if (levels > MAX_LEVELS)
            {
                return zeroPlan;
            }

            // Check for memoization
            if (bestScores.ContainsKey(state))
            {
                return bestScores[state];
            }

            // There is still room in our stack
            // Try each possible move
            GamePlan bestMove = zeroPlan;

            List<byte> possibleMoves = GetPossibleMoves(state);

            // If no moves were possible, the stack must be cleared
            if (possibleMoves.Count == 0)
            {
                state.stack.Clear();
                possibleMoves = GetPossibleMoves(state);
            }

            foreach(byte column in possibleMoves)
            {
                // Try move
                GameState moveState = new GameState(state);
                short moveScore = ScoreMove(state.stack, moveState.board[column].Peek());
                moveState.stack.Add(moveState.board[column].Pop());

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

        public short ScoreMove(List<byte> stack, byte card)
        {
            short pts = 0;

            // If the first card is a Jack, 2 pts
            if (stack.Count == 0 && card == 11)
            {
                pts += 2;
            }

            // If stack is now 15, 2 pts
            if (SumStack(stack) + CardValue(card) == 15)
            {
                pts += 2;
            }

            // If stack is now 31, 2 pts
            if (SumStack(stack) + CardValue(card) == 31)
            {
                pts += 2;
            }

            // Set of 2, 3, or 4
            if (stack.Count >= 1 && stack[stack.Count - 1] == card)
            {
                // Set of at least 2

                if (stack.Count >= 2 && stack[stack.Count - 2] == card)
                {
                    // Set of at least 3

                    if (stack.Count >= 3 && stack[stack.Count - 3] == card)
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
                // Set of 2
                pts += 2;
            }

            return pts;
        }

        public int SumStack(List<byte> stack)
        {
            int sum = 0;

            foreach (byte card in stack)
            {
                sum += CardValue(card);
            }

            return sum;
        }

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
                Console.Write(state.stack.Count > y ? GetCardName(state.stack[y]).PadLeft(2) : "  ");

                // Gap
                Console.Write("  ");

                // Columns
                for (int column = 0; column < 4; column++)
                {
                    Console.Write(state.board[column].Count > y ? GetCardName(state.board[column].ElementAt<byte>(y)).PadLeft(2) : "  ");
                }
                Console.Write("\r\n");
            }
            Console.WriteLine(SumStack(state.stack).ToString());
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
