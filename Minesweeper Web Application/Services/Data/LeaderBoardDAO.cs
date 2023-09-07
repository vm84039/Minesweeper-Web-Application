using Minesweeper_Web_Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using Minesweeper_Web_Application.Services.Utility;
using System.Linq;

namespace Minesweeper_Web_Application.Services.Data
{
    public class LeaderBoardDAO
    {
        readonly string connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;

        public void InsertHighScore(UserModel user, decimal time, int totalClicks)
        {
            if (CheckUserScore(user, time, totalClicks))
            {
                return;
            }

            string insert = "INSERT INTO dbo.leaderboard (USERNAME, TIME, TOTALCLICKS) VALUES (@Username, @Time, @TotalClicks)";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(insert, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                command.Parameters.AddWithValue("@Time", time);
                command.Parameters.AddWithValue("@TotalClicks", totalClicks);

                conn.Open();
                command.ExecuteNonQuery();
                Debug.WriteLine("Inserted high score for user: " + user.UserName);
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("An error has occurred. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred.");
            }
            finally
            {
                conn.Close();
            }
        }

        public List<LeaderBoard> GetScores(List<LeaderBoard> inc)
        {
            string retrieval = "SELECT * FROM dbo.leaderboard";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(retrieval, conn);

            try
            {
                conn.Open();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        LeaderBoard user = new LeaderBoard();
                        user.Username = reader.GetString(1);
                        user.Time = reader.GetDecimal(2);
                        user.TotalClicks = reader.GetInt32(3);
                        Debug.WriteLine(reader.GetString(1));
                        Debug.WriteLine(reader.GetDecimal(2));

                        inc.Add(user);
                    }
                }

                inc = inc.OrderBy(t => t.Time).ToList();

                for (int i = 0; i < inc.Count; i++)
                {
                    inc[i].Rank = i + 1;
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("An error has occurred. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred.");
            }
            finally
            {
                conn.Close();
            }

            return inc;
        }

        public bool CheckUserScore(UserModel user, decimal time, int totalClicks)
        {
            bool newTimeCheck = false;
            string query = "SELECT * FROM dbo.leaderboard WHERE USERNAME = @Username";

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(query, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", user.UserName);
                conn.Open();
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader.GetDecimal(2) >= time)
                        {
                            UpdateUserScore(user.UserName, time, totalClicks);
                        }
                    }
                    newTimeCheck = true;
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred.");
            }
            finally
            {
                conn.Close();
            }

            return newTimeCheck;
        }

        private void UpdateUserScore(string username, decimal time, int totalClicks)
        {
            string updateQuery = "UPDATE dbo.leaderboard SET TIME = @Time, TOTALCLICKS = @TotalClicks WHERE USERNAME = @Username";
            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(updateQuery, conn);

            try
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Time", time);
                command.Parameters.AddWithValue("@TotalClicks", totalClicks);
                conn.Open();
                command.ExecuteNonQuery();

                MineSweeperLogger.GetInstance().Info("Updated high score for user: " + username + " " + time.ToString());
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred.");
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
