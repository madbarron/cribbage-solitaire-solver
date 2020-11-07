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

            Console.WriteLine("Hello World!");

            // Get starting state
            GameState state = solver.GetStartingState();
            GamePlan plan = solver.EvaluateGame(state);

            Console.WriteLine(string.Format("{0} points will be scored.", plan.score));

            solver.DrawState(state);

            short points = 0;

            while (plan.moves.Count > 0)
            {
                byte move = plan.moves.Pop();
                byte card = state.board[move].Peek();
                if (solver.SumStack(state.stack) + solver.CardValue(card) > 31)
                {
                    state.stack.Clear();
                }
                points += solver.ScoreMove(state.stack, card);
                state.stack.Add(state.board[move].Pop());

                //Console.Write(move);
                solver.DrawState(state);
                Console.WriteLine(String.Format("{0} points.", points));
                Console.ReadKey();
            }
            //foreach(byte b in plan.moves)
            //{
            //    Console.Write(b);
            //}
        }

    }
}
