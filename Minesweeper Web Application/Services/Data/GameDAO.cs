using Minesweeper_Web_Application.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using Minesweeper_Web_Application.Services.Utility;

namespace Minesweeper_Web_Application.Services.Data
{
    public class GameDAO
    {
        readonly string connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        public List<object> RetrieveUserScore(List<object> values, UserModel user)
        {
            string retrieveQuery = "SELECT * FROM dbo.GameData WHERE USERNAME = @Username";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(retrieveQuery, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                conn.Open();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        values.Add(reader.GetInt64(1));
                        values.Add(reader.GetInt64(2));
                        values.Add(reader.GetInt64(3));
                        values.Add(reader.GetInt64(4));
                        values.Add(reader.GetInt64(5));
                        values.Add(reader.GetInt64(6));
                        values.Add(reader.GetInt64(7));
                        values.Add(reader.GetInt64(8));
                        values.Add(reader.GetInt32(9));
                        values.Add(reader.GetDecimal(10));
                    }
                }
                reader.Close();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated in retrieval. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred finding: " + user.UserName);
            }
            finally
            {
                conn.Close();
            }

            return values;
        }

        public void DeleteScore(UserModel user)
        {
            string deleteQuery = "DELETE FROM dbo.GameData WHERE USERNAME = @Username";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(deleteQuery, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                conn.Open();
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated in deletion. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred deleting score for: " + user.UserName);
            }
            finally
            {
                conn.Close();
            }
        }

        public void SaveGameBombs(GameBoardModel board, UserModel user, int mouseClicks, decimal finalTime)
        {
            string insertQuery = "INSERT INTO dbo.GameData (USERNAME, BOMB1, BOMB2, BOMB3, BOMB4, VISITED1, VISITED2, VISITED3, VISITED4, MOUSECLICKS, TIME) " +
                "VALUES (@Username, @Bomb1, @Bomb2, @Bomb3, @Bomb4, @Visited1, @Visited2, @Visited3, @Visited4, @MouseClicks, @Time)";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(insertQuery, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                command.Parameters.AddWithValue("@Bomb1", board.SaveBomb1);
                command.Parameters.AddWithValue("@Bomb2", board.SaveBomb2);
                command.Parameters.AddWithValue("@Bomb3", board.SaveBomb3);
                command.Parameters.AddWithValue("@Bomb4", board.SaveBomb4);
                command.Parameters.AddWithValue("@Visited1", board.SaveVisited1);
                command.Parameters.AddWithValue("@Visited2", board.SaveVisited2);
                command.Parameters.AddWithValue("@Visited3", board.SaveVisited3);
                command.Parameters.AddWithValue("@Visited4", board.SaveVisited4);
                command.Parameters.AddWithValue("@MouseClicks", mouseClicks);
                command.Parameters.AddWithValue("@Time", finalTime);

                conn.Open();
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated in saving game data. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred saving game data for: " + user.UserName);
            }
            finally
            {
                conn.Close();
            }
        }
        public bool CheckIfScoreExists(UserModel user)
        {
            bool scoreExists = false;

            string query = "SELECT COUNT(*) FROM dbo.GameData WHERE USERNAME = @Username";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(query, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                conn.Open();
                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count > 0)
                {
                    scoreExists = true;
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred while checking if the score exists for: " + user.UserName);
            }
            finally
            {
                conn.Close();
            }

            return scoreExists;
        }
    }
}
