using Minesweeper_Web_Application.Models;
using Minesweeper_Web_Application.Services.Business;
using Minesweeper_Web_Application.Services.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Minesweeper_Web_Application.Services.Data
{
    public class SecurityDAO
    {
        string key = "f68a03ab915d4918b6bbd2b400e06364";
        string connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        public bool CheckUsername(UserModel user)
        {
            string testing = "SELECT 1 FROM Users WHERE USERNAME = @Username"; // Assuming the table name is "Users"
            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(testing, conn);

            try
            {
                command.Parameters.Add("@USERNAME", MySqlDbType.VarChar, 25).Value = user.UserName;
                conn.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred checking username: " + user.UserName);
            }
            finally
            {
                conn.Close();
            }

            return false;
        }

        public bool RegisterUser(UserModel user)
        {
            bool registered = false;

            // Check if the user's gender is valid
            if (user.Gender != Gender.Male && user.Gender != Gender.Female)
            {
                // Handle invalid gender value here, log an error, or throw an exception.
                // For example, you can return false or log an error and return false.
                MineSweeperLogger.GetInstance().Error(null, $"Invalid gender value: {user.Gender}");
                return false;
            }
            user.Password = Encrypt(user.Password, key);

            MySqlConnection conn = new MySqlConnection(connectionStr);

            string insertQuery = "INSERT INTO users (FIRSTNAME, LASTNAME, AGE, GENDER, STATE, EMAILADDRESS, USERNAME, PASSWORD) " +
                "VALUES (@FirstName, @LastName, @Age, @Gender, @State, @EmailAddress, @UserName, @Password)";

            using (MySqlCommand command = new MySqlCommand(insertQuery, conn))
            {
                try
                {
                    command.Parameters.AddWithValue("@FIRSTNAME", user.FirstName);
                    command.Parameters.AddWithValue("@LASTNAME", user.LastName);
                    command.Parameters.AddWithValue("@AGE", user.Age);
                    command.Parameters.AddWithValue("@STATE", user.State);
                    command.Parameters.AddWithValue("@EMAILADDRESS", user.EmailAddress);
                    command.Parameters.AddWithValue("@USERNAME", user.UserName);
                    command.Parameters.AddWithValue("@PASSWORD", user.Password);


                    // Use the user.Gender enum value as-is ("Male" or "Female")
                    command.Parameters.AddWithValue("@Gender", user.Gender.ToString());

                    conn.Open();
                    command.ExecuteNonQuery();
                    registered = true;

                    Debug.WriteLine("Records inserted successfully!");
                }
                catch (MySqlException e)
                {
                    Debug.WriteLine("Error generated. Details: " + e.ToString());
                    MineSweeperLogger.GetInstance().Error(e, "An error occurred registering: " + user.UserName);
                }
            }
            return registered;
        }


        public bool FindByUser(UserLoginModel user)
        {
            bool found = false;
            string retrieveQuery = "SELECT * FROM Users WHERE USERNAME = @Username"; // Assuming the table name is "Users"

            MySqlConnection conn = new MySqlConnection(connectionStr);
            MySqlCommand command = new MySqlCommand(retrieveQuery, conn);

            try
            {
                command.Parameters.Add("@USERNAME", MySqlDbType.VarChar, 25).Value = user.Username;
                conn.Open();

                MySqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    found = true;
                    UserModel currUser = new UserModel();
                    while (reader.Read())
                    {
                        if (Decrypt(reader.GetString(8), key) != user.Password)
                        {
                            Debug.WriteLine("Username " + reader.GetString(7) + " Password " + Decrypt(reader.GetString(8), key));
                            return false;
                        }
                        currUser.FirstName = reader.GetString(1);
                        currUser.LastName = reader.GetString(2);
                        currUser.Age = reader.GetInt32(4);
                        if (reader.GetString(3) == "M")
                        {
                            currUser.Gender = Gender.Male;
                        }
                        else
                        {
                            currUser.Gender = Gender.Female;
                        }
                        currUser.State = reader.GetString(5);
                        currUser.EmailAddress = reader.GetString(6);
                        currUser.UserName = reader.GetString(7);
                        UserManagement.Instance._loggedUser = currUser;
                    }
                }
                reader.Close();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Error generated in retrieval. Details: " + e.ToString());
                MineSweeperLogger.GetInstance().Error(e, "An error occurred finding: " + user.Username);
            }
            finally
            {
                conn.Close();
            }

            return found;
        }

        private string CheckUserGender(UserModel user)
        {
            string genderStr;
            if (user.Gender == Gender.Male)
            {
                genderStr = "M";
            }
            else
            {
                genderStr = "F";
            }
            return genderStr;
        }

        public static string Encrypt(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // Initialization Vector

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // Initialization Vector

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
