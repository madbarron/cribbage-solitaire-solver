using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CribbageSolitaireSolver
{
    class GameState: IEquatable<GameState>
    {
        public Stack<byte>[] board;
        public List<byte> stack;

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

            this.stack = new List<byte>(copyFrom.stack);
        }

        public bool Equals([AllowNull] GameState other)
        {
            if (other == null)
            {
                return false;
            }

            // Try quick judgement
            if (other.GetHashCode() != GetHashCode())
            {
                return false;
            }

            // Compare stacks
            if (stack.Count != other.stack.Count)
            {
                return false;
            }

            for (byte i = 0; i < stack.Count; i++)
            {
                if (stack[i] != other.stack[i])
                {
                    return false;
                }
            }

            // Compare boards
            for (byte col = 0; col < 4; col++)
            {
                if (board[col].Count != other.board[col].Count)
                {
                    return false;
                }

                for (byte i = 0; i < board[col].Count; i++)
                {
                    if (board[col].ElementAt(i) != other.board[col].ElementAt(i))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void SetHashCode()
        {
            hashCode = stack.Count() + board[0].Count * 10 + board[1].Count * 100 + board[2].Count * 1000 - board[3].Count * 10000;
            hashCodeSet = true;
        }

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
