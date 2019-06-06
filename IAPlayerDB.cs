using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Baccaro.NetCore.SQLite;

namespace TicTacToeAI.Lib
{
    class IAPlayerDB
    {
        private string _dataBaseFileName;
        private SQLiteManager _sqlManager;
        public IAPlayerDB(string dataBaseFileName)
        {
            _dataBaseFileName = dataBaseFileName;
            CreateTable();
        }

        /// <summary>
        /// Create the table Nodes in DB if it does not already exist.
        /// </summary>
        private void CreateTable()
        {
            _sqlManager = new SQLiteManager(_dataBaseFileName);
            _sqlManager.ExecuteQuery(@"CREATE TABLE IF NOT EXISTS Nodes (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, CurrentNode NCHAR(9) NOT NULL, MoveNode NCHAR(9) NOT NULL, Wins INTEGER NOT NULL, Draws INTEGER NOT NULL, Total INTEGER NOT NULL);");
        }

        /// <summary>
        /// Search Node in the DB
        /// </summary>
        /// <param name="board">Node to search.</param>
        /// <returns>A Dataset with all results.</returns>
        public DataSet SearchNode(int[,] board)
        {
            DataSet resultData = _sqlManager.LoadData("SELECT * FROM Nodes WHERE CurrentNode = '" + MountSearchString(ref board) + "';");
            return resultData;
        }

        public void UpdateNode(DataRow row, string result)
        {
            long id = (long)row.ItemArray[0];
            string param1 = $"Total = {(long)row.ItemArray[5] + 1}";
            string param2 = "";

            if (result == "WIN")
                param2 = $", Wins = {(long)row.ItemArray[3] + 1}";
            else if (result == "DRAW")
                param2 = $", Draws = {(long)row.ItemArray[4] + 1}";

            _sqlManager.ExecuteQuery($"UPDATE Nodes SET {param1}{param2} WHERE id = {id};");
        }

        /// <summary>
        /// Converts the node (int[,]) to a string.
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Return a string with the pattern '000000000'.</returns>
        public static string MountSearchString(ref int[,] board)
        {
            string searchString = "";
            for (var row = 0; row < 3; row++)
                for (var col = 0; col < 3; col++)
                    searchString += board[row, col];
            return searchString;
        }

        public void AddNodes(int[,] currentBoard, List<int[,]> listOfPossibleNodes)
        {
            string currentNodeStr = MountSearchString(ref currentBoard);
            for(int i = 0; i < listOfPossibleNodes.Count; i++)
            {
                int[,] newNode = listOfPossibleNodes[i];
                string newNodeStr = MountSearchString(ref newNode);
                _sqlManager.ExecuteQuery($@"INSERT INTO Nodes (CurrentNode, MoveNode, Wins, Draws, Total) VALUES ('{currentNodeStr}', '{newNodeStr}', {0}, {0}, {0});");
            }
        }

        public static int[,] ConvertNodeToInt2D(string strNode)
        {
            var intNode = new int[3, 3];
            for (int i = 0; i < 9; i++)
                intNode[i / 3, i % 3] = Convert.ToInt32(strNode.Substring(i, 1));
            return intNode;
        }

    }
}
