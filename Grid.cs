using System;
using System.Collections.Generic;

namespace TicTacToeAI.Lib
{
    public class Grid
    {
        // privates:
        private int[,] _board;
        
        private Player _player1;
        private Player _player2;
        private bool _flagRotation;
        

        public int FilledCellCounter { get; private set; }
        public int Rotation { get; private set; }
        public int Mirror { get; private set; }
        public bool FlagMirror { get; private set; }


        public Grid(Player player1, Player player2)
        {
            _board = new int[3,3];
            FilledCellCounter = 0;
            _player1 = player1;
            _player2 = player2;
            _flagRotation = false;
            Rotation = 0;
            FlagMirror = false;
            Mirror = 0;

        }

        public void printBoard()
        {
            Console.WriteLine();
            for(var i = 0; i < 8; i++)
            {
                printLine(i);
            }
            printLine(0);
        }

        private void printLine(int lineIndex)
        {
            var line = "";
            for (var i = 0; i < 11; i++)
            {
                if (i % 4 == 3)
                    line += "|";
                else if (lineIndex % 3 == 2)
                    line += "_";
                else if ((lineIndex % 3 == 1) && (i % 4 == 1))
                    line += GetPlayerMark(lineIndex / 3, i / 4);
                else
                    line += " ";
            }
            Console.WriteLine(line);
        }

        private char GetPlayerMark(int row, int col)
        {
            if (_board[row, col] == 1)
                return _player1.Mark;
            else if (_board[row, col] == 2)
                return _player2.Mark;
            return ' ';

        }

        public int CheckWinner()
        {
            var result = -1;
            if (FilledCellCounter > 4)
            {
                // row and columns
                for (int i = 0; i < 3; i++)
                {
                    if ((_board[i, i] != 0) && (result == -1))
                    {
                        if (CheckEqual(new int[3] { _board[i, 0], _board[i, 1], _board[i, 2] }))
                            result = _board[i, 0];
                        if (CheckEqual(new int[3] { _board[0, i], _board[1, i], _board[2, i] }))
                            result = _board[0, i];
                    }
                }

                // cross
                if ((_board[1, 1] != 0) && (result == -1))
                {
                    if (CheckEqual(new int[3] { _board[0, 0], _board[1, 1], _board[2, 2] }))
                        result = _board[0, 0];
                    if (CheckEqual(new int[3] { _board[0, 2], _board[1, 1], _board[2, 0] }))
                        result = _board[0, 2];
                }
            }
            // draw
            if((FilledCellCounter == 9) && (result == -1))
                result = 0;

            return result;
        }

        private bool CheckEqual(int[] numbers)
        {
            for(int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i - 1] != numbers[i])
                    return false;
            }
            return true;
        }

        private bool SetBoard(int row, int col, char mark)
        {
            int value;
            if (mark == _player1.Mark)
                value = 1;
            else if (mark == _player2.Mark)
                value = 2;
            else
                return false;

            try
            {
                if (_board[row, col] == 0)
                {
                    _board[row, col] = value;
                    if (_flagRotation == false)
                        SetRotation();
                    else if ((_flagRotation == true) && (FlagMirror == false))
                        SetMirror();
                    FilledCellCounter++;
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

         public bool SetBoard(int cell, char mark)
        {
            int col = (cell - 1) % 3;
            int row = (cell - 1) / 3;

            return SetBoard(row, col, mark);
        }

        /// <summary>
        /// Return a normalized board according to the player's mark.
        /// </summary>
        /// <param name="mark">Player mark</param>
        /// <returns></returns>
        public int[,] GetNormalizedBoard(char mark)
        {
            
            if (mark == _player1.Mark)
                return _board;
            else if (mark == _player2.Mark)
            {
                int[,] normalBoard = new int[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                for (int row = 0; row < 3; row++)
                {
                    for (int col = 0; col < 3; col++)
                    {
                        if (_board[row, col] == 1)
                            normalBoard[row, col] = 2;
                        else if (_board[row, col] == 2)
                            normalBoard[row, col] = 1;
                    }
                }
                return normalBoard;
            }
            else
                return null;
        }

           

        /// <summary>
        /// Set _rotation that define how the board must be rotated.
        /// </summary>
        private void SetRotation()
        {
            // Do not set the rotation in the first move in the center cell.
            if ((FilledCellCounter == 0) && _board[1, 1] != 0)
                return;


            if ((_board[2, 2] != 0) || (_board[2, 1] != 0))
                Rotation = 2;
            else if ((_board[2, 0] != 0) || (_board[1, 0] != 0))
                Rotation = 1;
            else if ((_board[0, 2] != 0) || (_board[1, 2] != 0))
                Rotation = -1;

            _flagRotation = true;
        }

        /// <summary>
        /// Set _mirrorBoard that define how the board must be mirrored.
        /// </summary>
        private void SetMirror()
        {
            int[,] rotBoard = Baccaro.NetCore.Math.Matrix.RotateMatrix90<int>(_board, Rotation);

            if (rotBoard[0, 0] != 0)
            {
                if ((rotBoard[0, 1] != 0) || (rotBoard[0, 2] != 0) || (rotBoard[1, 2] != 0))
                    FlagMirror = true;
                else if ((rotBoard[1, 0] != 0) || (rotBoard[2, 0] != 0) || (rotBoard[2, 1] != 0))
                {
                    Mirror = 1;
                    FlagMirror = true;
                }
            }
            else if (rotBoard[0, 1] != 0)
            {
                if ((rotBoard[0, 0] != 0) || (rotBoard[1, 0] != 0) || (rotBoard[2, 0] != 0))
                    FlagMirror = true;
                else if ((rotBoard[0, 2] != 0) || (rotBoard[1, 2] != 0) || (rotBoard[2, 2] != 0))
                {
                    Mirror = 2;
                    FlagMirror = true;
                }
            }
        }

        /// <summary>
        /// Clear the board.
        /// </summary>
        public void ClearBoard()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                    _board[row, col] = 0;
            }
            _flagRotation = false;
            Rotation = 0;
            FilledCellCounter = 0;
            FlagMirror = false;
            Mirror = 0;
        }

        /// <summary>
        /// Return an array with empty cell indexes.
        /// </summary>
        /// <returns></returns>
        public int[] GetEmptyCells()
        {
            int[] indexList = new int[9 - FilledCellCounter];
            var j = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if(_board[row,col]==0)
                    {
                        indexList[j] = row * 3 + col;
                        j++;
                    }
                }
            }
            return indexList;
        }
    }
}
