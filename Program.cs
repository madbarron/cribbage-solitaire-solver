using System;
using System.Collections.Generic;
using System.Linq;

namespace CribbageSolitaireSolver
{
    struct GameState
    {
        public Stack<byte>[] board;
        public List<byte> stack;

        public GameState(GameState copyFrom)
        {
            this.board = new Stack<byte>[4];
            for (int i = 0; i < 4; i++)
            {
                this.board[i] = new Stack<byte>(copyFrom.board[i].Reverse<byte>());
            }

            this.stack = new List<byte>(copyFrom.stack);
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
            GameState state = solver.GetStateFromConsole();

            GamePlan plan = solver.EvaluateGame(state);

            GameState playState = state;

            while (plan.score > 0)
            {
                playState = new GameState(playState);

                // Display plan to user
                while (plan.moves.Count > 0)
                {
                    byte move = plan.moves.Pop();
                    byte card = playState.board[move].Peek();
                    if (solver.SumStack(playState.stack) + solver.CardValue(card) > 31)
                    {
                        playState.stack.Clear();
                        Console.WriteLine("Clear.");
                        break;
                    }
                    short points = solver.ScoreMove(playState.stack, card);
                    playState.stack.Add(playState.board[move].Pop());

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
