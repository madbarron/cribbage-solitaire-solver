using System;
using System.Collections.Generic;
using System.Linq;

namespace CribbageSolitaireSolver
{
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
            //GameState state = solver.GetBenchmarkState();
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
                    byte card = solver.GetCard(move, playState);
                    if (solver.SumHand(playState.hand) + solver.CardValue(card) > 31)
                    {
                        playState.hand = 0;
                        Console.WriteLine("Clear.");
                        break;
                    }
                    byte points = solver.ScoreMove(playState.hand, card);
                    playState.hand = LongStack.Push(playState.hand, solver.GetCard(move, playState));
                    playState.IncrementBoardHeight(move);

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

            Console.ReadLine();
        }
    }
}
