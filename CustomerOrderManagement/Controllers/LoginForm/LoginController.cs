using BAL;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AspnetCoreMvcFull.Controllers.LoginForm
{
  public class LoginController : Controller
  {

    private readonly LoginBAL LogBal;
    private readonly GlobalSessionBAL _sessionHelper;
    // Constructor injection for LoginBAL
    public LoginController(LoginBAL loginBAL, GlobalSessionBAL sessionHelper)
    {
      LogBal = loginBAL;
      _sessionHelper = sessionHelper ?? throw new ArgumentNullException(nameof(sessionHelper));
    }

    public IActionResult StartPage()
    {
      return View();
      //return RedirectToAction("Index","OrderMaster");
    }
    public IActionResult Login()
    {
      return View();
    }
    /*
        public IActionResult RegisterUser()
        {
          return View();
        }

        public IActionResult SaveUserDetails()
        {
          var form = HttpContext.Request.Form;
          LogBal.saveUserDetails(form);
          return View("Login");
        }
    */
    public IActionResult OTPSendOnNumber()
    {
      var form = HttpContext.Request.Form;
      Random generator = new Random();
      string RandomNumber = generator.Next(0, 1000000).ToString("D6");
     
      string mobileNo = form["MobileNumber"];

      // CHECK MOBILE NO IS EXIT IN USER MAGENEMENT TBALE
      // IF EXIT THEN SEND OTP OTHERWISE REDIRECT TO LOGIN PAGE
      DataSet userDataset = LogBal.CheckUserExistOrNot(mobileNo);
      if (userDataset.Tables.Count > 0 && userDataset.Tables[0].Rows.Count > 0)
      {
        ViewBag.MobileNumber = mobileNo;
        string lastFourDigits = mobileNo.Substring(mobileNo.Length - 4);
        ViewBag.Last4Digit = lastFourDigits; // Passing to the view
        TempData["OTP"] = RandomNumber;
        
        String SettingValue = _sessionHelper.CheckSettingMaster(mobileNo, "is_testing_server");
        if(!string.IsNullOrEmpty(SettingValue))
        {
          if (SettingValue == "0")
          {
            LogBal.SendOTPOnNumber(mobileNo, RandomNumber);
          }
          else
          {
            TempData["OTPHIDE"] = RandomNumber;
          }
        }
        else
        {
          TempData["OTPHIDE"] = RandomNumber;
        }

        return View("OTPValidatePage");
      }
      else
      {
        TempData["Warning"] = "Login Details Not Found...!!";
        return RedirectToAction("startpage", "Login");
      }
    }

    //THIS IS USED FOR CHECK USER NUMBER EXIST OR NOT
    public IActionResult CheckingUserExistOrNot()
    {
      var form = HttpContext.Request.Form;
      // Assuming `CheckUserExistOrNot` returns a DataSet
      DataSet userDataset = LogBal.CheckUserExistOrNot(form["MobileNumber"].ToString());
      // Check if the DataSet contains any tables and rows
      if (userDataset.Tables.Count > 0 && userDataset.Tables[0].Rows.Count > 0)
      {
        // CREATE SESSIONS
        _sessionHelper.SetSessionValue("UserId", userDataset.Tables[0].Rows[0]["id"].ToString());
        _sessionHelper.SetSessionValue("RoleId", userDataset.Tables[0].Rows[0]["role_id"].ToString());
        _sessionHelper.SetSessionValue("MobileNo", userDataset.Tables[0].Rows[0]["mobile_no"].ToString());
        _sessionHelper.SetSessionValue("TenantId", userDataset.Tables[0].Rows[0]["tenant_id"].ToString());
        /*_sessionHelper.GetSessionValue("UserId");*/

        TempData["RoleId"] = userDataset.Tables[0].Rows[0]["role_id"].ToString();

        // Redirect to the dashboard if records exist
        return RedirectToAction("Dashboard", "Dashboard");
      }
      else
      {
        TempData["Warning"] = "Login Details Not Found...!!";
        return RedirectToAction("startpage", "Login");
        // Redirect to the login view if no records exist
        //return RedirectToAction("RedirectUserToRegister", "Login");
      }
    }


    public IActionResult LogOutUser()
    {
      //_sessionHelper.ClearAllSessions(); // Assuming your session helper supports this
      HttpContext.Session.Clear(); // Clears all session values
      //HttpContext.Session.Abandon(); // Terminates the session
      return RedirectToAction("startpage", "Login");
    }

  }
}
