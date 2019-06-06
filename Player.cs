using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Baccaro.NetCore.IO;

namespace TicTacToeAI.Lib
{
    public class Player
    {
        public string Name { get; private set; }
        public char Mark { get; private set; }
        public int Score { get; private set; }
        public bool Learn { get;  set; } = true;

        public Player(string name, char playerMark)
        {
            Name = name;
            Mark = playerMark;
            Score = 0;
        }

        public virtual bool MakePlay(Grid board)
        {
            var success = false;
            while (success == false)
            {
                Console.WriteLine();
                UserInput userIn = Input.GetIntNumber("q", this.Name + " select a cell (1 to 9) to play: ");
                if (userIn.ExitFlag)
                    return false;
                else
                    success = board.SetBoard(userIn.IntegerInput, this.Mark);
            }
            return true;
        }

        public virtual void UpdateScore(int score, string strResult = "")
        {
            Score += score;
        }
    }

    // ===================================================================================================================================
    public class TicTacToe_AI_Player : Player
    {
        private int _mode_AI;
        private Random _rnd;
        //private List<List<int[,]>> _historic;
        private List<DataRow> _historicRows;

        private const string _dataBaseFileName = "PlayerIA.sqlite";
        private const int _monteCarloWinCoeficient = 5;
        private const int _monteCarloDrawCoeficient = 1;

       

        public TicTacToe_AI_Player(string name, char playerMark, int mode_AI) : base (name, playerMark)
        {
            _mode_AI = mode_AI;
            _rnd = new Random();
            _historicRows = new List<DataRow>();
        }

        public override bool MakePlay(Grid board)
        {
            // Random AI
            if (_mode_AI == 0)
                RandomPlay(board);
            // Learning AI
            else if (_mode_AI == 1)
                IntelligentPlay(board);
            return true;
        }

        public override void UpdateScore(int score, string strResult)
        {
            base.UpdateScore(score);

            if (Learn)
            {
                var databaseIA = new IAPlayerDB(_dataBaseFileName);

                foreach (DataRow log in _historicRows)
                    databaseIA.UpdateNode(log, strResult.ToUpper());
            }

            _historicRows.Clear();
        }

        private void RandomPlay(Grid board)
        {
            int[] indexList = board.GetEmptyCells();
            int index = indexList[_rnd.Next(indexList.Length)] + 1;
            Console.WriteLine($"{Name} randomly playing at cell {index}.");
            board.SetBoard(index, Mark);
        }

        private void IntelligentPlay(Grid board)
        {
            int[,] normalBoard = board.GetNormalizedBoard(this.Mark);
            ApplyBoardTransforms(board, ref normalBoard);

            // Search coodedinates.
            int[] moveCoordinate = SearchBestMove(board, ref normalBoard);

            Console.WriteLine($"{Name} playing at cell {moveCoordinate[0] * 3 + moveCoordinate[1] + 1}.");
            // Make play.
            board.SetBoard(moveCoordinate[0] * 3 + moveCoordinate[1] + 1, this.Mark);

        }

        private void ApplyBoardTransforms(Grid board, ref int[,] matriz2D)
        {
            matriz2D = Baccaro.NetCore.Math.Matrix.RotateMatrix90<int>(matriz2D, board.Rotation);
            ApplyMirrorTransforms(board.Mirror, ref matriz2D);
        }

        private void RevertBoardTransforms(Grid board, ref int[,] matriz2D)
        {
            ApplyMirrorTransforms(board.Mirror, ref matriz2D);
            matriz2D = Baccaro.NetCore.Math.Matrix.RotateMatrix90<int>(matriz2D, board.Rotation * (-1));
        }

        private void ApplyMirrorTransforms(int mirror, ref int[,] matriz2D)
        {
            if (mirror == 1)
                matriz2D = Baccaro.NetCore.Math.Matrix.Transpose<int>(matriz2D);
            else if (mirror == 2)
                matriz2D = Baccaro.NetCore.Math.Matrix.ReverseRows<int>(matriz2D);
        }


        private int[] SearchBestMove(Grid board, ref int[,] normalBoard)   
        {
            int[,] bestBoard = new int[3, 3];

            // 01 Searching best move.
            var databaseIA = new IAPlayerDB(_dataBaseFileName);
            DataSet moveNodes = databaseIA.SearchNode(normalBoard);

            if(moveNodes.Tables[0].Rows.Count == 0)
            {
                CreateNode(board, normalBoard, ref databaseIA);
                moveNodes = databaseIA.SearchNode(normalBoard);
            }
            bestBoard = SelectNode(board, moveNodes, normalBoard);
           
            //Achar as coordenadas (retirando rot end mirror) da jogada achada em 01
            return DetermineCoords(board, normalBoard, bestBoard);
        }

        private void CreateNode (Grid board, int[,] currentBoard, ref IAPlayerDB databaseIA)
        {
            List<int[,]> listOfPossibleNodes = new List<int[,]>();
            int count = board.FilledCellCounter;

            if(count==0)
            {
                int[,] temp = new int[3, 3] { { 1, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                listOfPossibleNodes.Add(temp);
                temp = new int[3, 3] { { 0, 1, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                listOfPossibleNodes.Add(temp);
                temp = new int[3, 3] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
                listOfPossibleNodes.Add(temp);
            }
            else if((count == 1) && (currentBoard[1,1] != 0))
            {
                int[,] temp = new int[3, 3] { { 1, 0, 0 }, { 0, 2, 0 }, { 0, 0, 0 } };
                listOfPossibleNodes.Add(temp);
                temp = new int[3, 3] { { 0, 1, 0 }, { 0, 2, 0 }, { 0, 0, 0 } };
                listOfPossibleNodes.Add(temp);
            }
            else if(board.FlagMirror == false)
            {
                if (currentBoard[0, 0] != 0)
                {
                    for(int row = 0; row < 3; row++)
                    {
                        for(int col = 1; col < 3; col++)
                        {                           
                            if((col >= row) && (currentBoard[row, col] == 0))
                            {
                                int[,] temp = (int[,])currentBoard.Clone();
                                temp[row, col] = 1;
                                listOfPossibleNodes.Add(temp);
                            }
                        }
                    }
                }
                else
                {
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 2; col++)
                        {
                            if (currentBoard[row, col] == 0)
                            {
                                int[,] temp = (int[,])currentBoard.Clone();
                                temp[row, col] = 1;
                                listOfPossibleNodes.Add(temp);
                            }
                        }
                    }
                }
            }
            else
            {
                int[,] temp = (int[,])currentBoard.Clone();
                if (checkWinDefeatSituation(ref temp))
                    listOfPossibleNodes.Add(temp);
                else
                {
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            if (currentBoard[row, col] == 0)
                            {
                                temp = (int[,])currentBoard.Clone();
                                temp[row, col] = 1;
                                listOfPossibleNodes.Add(temp);
                            }
                        }
                    }
                }
            }
            databaseIA.AddNodes(currentBoard, listOfPossibleNodes);
        }

        private int[,] SelectNode(Grid board, DataSet data, int[,] normalBoard)
        {
            // Calculate totalSimulations
            long totalSimulations = 0;
            for (int i = 0; i < data.Tables[0].Rows.Count; i++)
                totalSimulations += (long)data.Tables[0].Rows[i].ItemArray[5];


            var neverTested = new List<int>();
            var monteCarloRatio = new List<double>();


            for (int index = 0; index < data.Tables[0].Rows.Count; index++)
            {
                if ((long)data.Tables[0].Rows[index].ItemArray[5] == 0)
                {
                    neverTested.Add(index);
                    monteCarloRatio.Add(0.0);
                }
                else
                    monteCarloRatio.Add(CalculateMonteCarloRatio(totalSimulations, data.Tables[0].Rows[index]));
            }

            int bestIndex;
            if (neverTested.Count > 0)
                bestIndex = neverTested[_rnd.Next(neverTested.Count)];
            else
            {
                int[] biggestIndexList = Baccaro.NetCore.Math.General.GetAllIndexesOfTheHighestValue<double>(monteCarloRatio.ToArray());
                bestIndex = biggestIndexList[_rnd.Next(biggestIndexList.Length)];
            }

            //for testing its random for now
            //bestIndex = _rnd.Next(data.Tables[0].Rows.Count);
            //end simulating


            int[,] bestNode= IAPlayerDB.ConvertNodeToInt2D((string)data.Tables[0].Rows[bestIndex].ItemArray[2]);

            // log before some randomization 
            Log(data.Tables[0].Rows[bestIndex]);

            //do some randomization if applicable
            bestNode = SelectMove(board, normalBoard, bestNode);

            return bestNode;
        }

        private double CalculateMonteCarloRatio(double totalSimulations, DataRow node)
        {
            double winScore = _monteCarloWinCoeficient * (long)node.ItemArray[3];
            double drawScore = _monteCarloDrawCoeficient * (long)node.ItemArray[4];
            double totalScore = _monteCarloWinCoeficient * (long)node.ItemArray[5];

            if (totalScore == 0)
                return 0.0;

            double exploitationFactor = (winScore + drawScore) / totalScore;
            double explorationFactor = 0;
            if(Learn)
                explorationFactor = Math.Sqrt(2.0) * Math.Sqrt(Math.Log(totalSimulations * _monteCarloWinCoeficient) / totalScore);


            return exploitationFactor + explorationFactor;
        }

        private int[,] SelectMove(Grid board, int[,] normalBoard, int[,] selectedNode)
        {
            int count = board.FilledCellCounter;

            if ((count == 0) || ((count == 1) && (normalBoard[1, 1] != 0)))
            {
                if (selectedNode[0, 0] != 0)
                {
                    var indexes = new int[] { 0, 2, 6, 8 };
                    var index = indexes[_rnd.Next(indexes.Length)];
                    selectedNode = (int[,])normalBoard.Clone();
                    selectedNode[index / 3, index % 3] = 1;
                }
                else if (selectedNode[0, 1] != 0)
                {
                    var indexes = new int[] { 1, 3, 5, 7 };
                    var index = indexes[_rnd.Next(indexes.Length)];
                    selectedNode = (int[,])normalBoard.Clone();
                    selectedNode[index / 3, index % 3] = 1;
                }
            }
            else if (board.FlagMirror == false)
            {
                var mirror = _rnd.Next(2);
                if (mirror == 1)
                {
                    if (normalBoard[0, 0] != 0)
                    { 
                        for (int row = 0; row < 3; row++)
                        {
                            for (int col = 1; col < 3; col++)
                            {
                                if ((col > row) && (selectedNode[row, col] != normalBoard[row, col]))
                                {
                                    selectedNode[row, col] = normalBoard[row, col];
                                    selectedNode[col, row] = 1;
                                    return selectedNode;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int row = 0; row < 3; row++)
                        {
                            if (selectedNode[row, 0] != normalBoard[row, 0])
                            {
                                selectedNode[row, 0] = normalBoard[row, 0];
                                selectedNode[row, 2] = 1;
                                return selectedNode;
                            }
                        }
                    }
                }

            }
            return selectedNode;
        }

        private void Log(DataRow row)
        {
            _historicRows.Add(row);
        }


        private bool checkWinDefeatSituation(ref int[,] newNode)
        {
            if (checkWinDefeatSituation(ref newNode, 1))
                return true;
            return checkWinDefeatSituation(ref newNode, 2);
        }

        private bool checkWinDefeatSituation(ref int[,] newNode, int condition)
        {
            List<int> lista = new List<int>();
            int[] array;
            for (int i = 0; i < 3; i++)
            {
                lista.Clear();
                lista.Add(newNode[i, 0]);
                lista.Add(newNode[i, 1]);
                lista.Add(newNode[i, 2]);

                array = lista.ToArray();
                if (Baccaro.NetCore.Math.General.CountValue(0, array) == 1 && (Baccaro.NetCore.Math.General.CountValue(condition, array) == 2))
                {
                    newNode[i, lista.IndexOf(0)] = 1;
                    return true;
                }
                lista.Clear();
                lista.Add(newNode[0, i]);
                lista.Add(newNode[1, i]);
                lista.Add(newNode[2, i]);

                array = lista.ToArray();
                if (Baccaro.NetCore.Math.General.CountValue(0, array) == 1 && (Baccaro.NetCore.Math.General.CountValue(condition, array) == 2))
                {
                    newNode[lista.IndexOf(0), i] = 1;
                    return true;
                }                
            }
            //cross
            lista.Clear();
            lista.Add(newNode[0, 0]);
            lista.Add(newNode[1, 1]);
            lista.Add(newNode[2, 2]);

            array = lista.ToArray();
            if (Baccaro.NetCore.Math.General.CountValue(0, array) == 1 && (Baccaro.NetCore.Math.General.CountValue(condition, array) == 2))
            {
                int i = lista.IndexOf(0);
                newNode[i, i] = 1;
                return true;
            }
            lista.Clear();
            lista.Add(newNode[0, 2]);
            lista.Add(newNode[1, 1]);
            lista.Add(newNode[2, 0]);

            array = lista.ToArray();
            if (Baccaro.NetCore.Math.General.CountValue(0, array) == 1 && (Baccaro.NetCore.Math.General.CountValue(condition, array) == 2))
            {
                int i = lista.IndexOf(0);
                newNode[i, 2-i] = 1;
                return true;
            }

            return false;
        }
        private int[] DetermineCoords(Grid board, int[,] normalBoard, int[,] moveBoard)
        {
            int[] coords = new int[2];
            int[,] diffMatrix = new int[3, 3];
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    if (normalBoard[row, col] == moveBoard[row, col])
                        diffMatrix[row, col] = 0;
                    else
                        diffMatrix[row, col] = 1;
                }
            }

            RevertBoardTransforms(board, ref diffMatrix);

            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    if(diffMatrix[row, col] == 1)
                    {
                        coords[0] = row;
                        coords[1] = col;
                        return coords;
                    }
                }
            }
            return null;
        }

    }

}
