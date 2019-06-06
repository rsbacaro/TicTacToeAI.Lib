using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeAI.Lib
{
    public class Game
    {
        private static int _victoryScore = 5;
        private static int _drawScore = 1;
        private static int _defeatScore = 0;
        private Player _player1;
        private Player _player2;

        public Game (Player player1, Player player2)
        {
            _player1 = player1;
            _player2 = player2;
        }

        public void Start( int numberOfGames)
        {
            var gameCounter = 0;
            var board = new Grid(_player1, _player2);
            var firstPlayer = true;

            while (gameCounter < numberOfGames)
            {
                bool nextPlayer = firstPlayer;
                int result = -1;
                Console.WriteLine($"Game number {gameCounter + 1}.\n");
                board.printBoard();
                while (result == -1)
                {
                    if (nextPlayer)
                        _player1.MakePlay(board);
                    else
                        _player2.MakePlay(board);
                    nextPlayer = !nextPlayer;
                    board.printBoard();
                    result = board.CheckWinner();
                }

                EndGame(result);
                board.ClearBoard();
                firstPlayer = !firstPlayer;
                gameCounter++;
            }
        }

        public void EndGame(int result)
        {
            if (result == 1)
            {
                Console.WriteLine(_player1.Name + " wins.");
                _player1.UpdateScore(_victoryScore, "WIN");
                _player2.UpdateScore(_defeatScore, "DEFEAT");               
            }
            else if (result == 2)
            {
                Console.WriteLine(_player2.Name + " wins.");
                _player2.UpdateScore(_victoryScore, "WIN");
                _player1.UpdateScore(_defeatScore, "DEFEAT");
            }
            else if (result == 0)
            {
                Console.WriteLine("Draw.");
                _player1.UpdateScore(_drawScore, "DRAW");
                _player2.UpdateScore(_drawScore, "DRAW");
            }

            Console.WriteLine(_player1.Name + " score: " + _player1.Score);
            Console.WriteLine(_player2.Name + " score: " + _player2.Score + "\n");
        }
    }
}
