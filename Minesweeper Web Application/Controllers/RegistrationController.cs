using Minesweeper_Web_Application.Models;
using Minesweeper_Web_Application.Services.Business;
using System.Web.Mvc;
using NLog;
using Minesweeper_Web_Application.Services.Utility;

namespace Minesweeper_Web_Application.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Registration
        public ActionResult Index()
        {
            return View("Registration");
        }

        [HttpPost]
        public ActionResult RegisterAccount(UserModel model)
        {
            SecurityService service = new SecurityService();

            if (service.CheckUser(model))
            {
                MineSweeperLogger.GetInstance().Warning($"User attempted account creation with {model.UserName} already registered.");
                ModelState.AddModelError("Username", "This username is in use.");
                return View("Registration");
            }

            bool results = service.Register(model);

            if (results)
            {
                MineSweeperLogger.GetInstance().Info($"{model.UserName} created an account.");
                // Clearing fields before entering the Login view
                ModelState.Clear();
                return RedirectToAction("Login");
            }
            else
            {
                MineSweeperLogger.GetInstance().Warning($"{model.UserName} failed to create an account.");
                return View("Registration");
            }
        }

        public ActionResult Login()
        {
            return View("~/Views/Login/Login.cshtml");
        }
    }
}
