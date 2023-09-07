using System;
using System.Web.Mvc;
using Minesweeper_Web_Application.Services.Data;

namespace Minesweeper_Web_Application.Controllers
{
    public class DatabaseController : Controller
    {
        public ActionResult CheckConnection()
        {
            DatabaseHelper dbHelper = new DatabaseHelper("server=localhost;port=3306;database=minesweeper_db;uid=root;password=root;");
            bool isDatabaseConnected = dbHelper.IsDatabaseConnected();

            ViewBag.IsDatabaseConnected = isDatabaseConnected;

            return View();
        }
    }
}
