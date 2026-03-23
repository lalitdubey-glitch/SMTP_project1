using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SMTP_project1.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using AspNetCoreGeneratedDocument;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace SMTP_project1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _con;
        private readonly IWebHostEnvironment _env;
        DBLayer db;

        public HomeController(IConfiguration con, IWebHostEnvironment env)
        {
            db = new DBLayer(con);
            _env = env;
            _con = con;
        } 
        public IActionResult Index()
        {
            return View();
        } 

        public IActionResult SignUp(int? id)
        {
            EditForm e = new EditForm();
            if (id != null)
            {
               DataTable dt = db.table("sp_SignUp", new SqlParameter[]
               {
                    new SqlParameter("@action","SelectOne"),
                    new SqlParameter("@id", id),
               });

                if (dt.Rows.Count > 0)
                {
                    e.id = Convert.ToInt32(dt.Rows[0]["id"]);
                    e.name = Convert.ToString(dt.Rows[0]["name"]);
                    e.email = Convert.ToString(dt.Rows[0]["email"]);
                    e.pass = Convert.ToString(dt.Rows[0]["pass"]);
                    e.img = Convert.ToString(dt.Rows[0]["img"]);
                    e.isActive = Convert.ToBoolean(dt.Rows[0]["isActive"]);
                }

            }
            return View(e);
        }

        [HttpPost]
        public IActionResult SignUp(SingUpModel s)
        {
            string action = null;
            string pic = null;
            string HasPass = null;
            MailMessage EmailMsg = null;

            string AdminEmail = _con["EmailSettings:AdminEmail"];
            string AdminPass = _con["EmailSettings:AdminPass"]; 

            if (s.id.HasValue)
            {
                action = "edit";
            }
            else
            {
                action = "add";
            }
          
            if (s.img != null)
            {
                string filename = Path.GetFileName(s.img.FileName);
                string path = Path.Combine(_env.WebRootPath, "UserImages", filename);
                using var stream = new FileStream(path, FileMode.Create);
                s.img.CopyTo(stream);
                pic = filename;
            }
           
            if (s.pass != null)
            {
              HasPass =  BCrypt.Net.BCrypt.HashPassword(s.pass);
            }

            SqlParameter msg = new SqlParameter("@msg", SqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output
            };

            int res = db.ExecuteQuery("sp_SignUp", new SqlParameter[]
             {
                   new SqlParameter("@action", action),
                   new SqlParameter("@name", s.name),
                   new SqlParameter("@email", s.email),
                   new SqlParameter("@pass", HasPass!=null?HasPass:DBNull.Value),
                   new SqlParameter("@img", pic!=null?pic:DBNull.Value),
                   msg
                 });

            string Rmsg = msg.Value.ToString();

            if (Rmsg == "Success")
            {
                if (s.id.HasValue)
                {
                    return Json(new { success = "update" });
                }
                else
                {
                    SmtpClient smtp = new SmtpClient()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        Credentials = new NetworkCredential(AdminEmail, AdminPass)
                    };
                    EmailMsg = new MailMessage
                                        (
                                        new MailAddress(AdminEmail, "Lalit Dubey"),
                                        new MailAddress(s.email)
                                        );


                    EmailMsg.Body = ("<h1>Thanks For Connecting Us</h1>");
                    EmailMsg.Subject = "Registration Form";
                    EmailMsg.IsBodyHtml = true;
                    smtp.Send(EmailMsg);
                    return Json(new { success = true });
                }
                   
            }
            else
            {
                return Json(new { success = false });
            }

        } 
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel l)
        {
            SqlParameter msg = new SqlParameter("@msg", SqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
           DataTable dt = db.table("sp_SignUp", new SqlParameter[]
            {
                new SqlParameter("@action" , "Login"),
                new SqlParameter("@email" , l.email),
                msg
            });

            string rMSG = msg.Value.ToString();

            if(rMSG== "Email Not Exists")
            {
                return Json(new { success = "Email Not Exists" });
            }

            if (dt.Rows.Count > 0)
            {
                bool IsValid = BCrypt.Net.BCrypt.Verify(l.pass, dt.Rows[0]["pass"].ToString());
                if (IsValid)
                {
                    Response.Cookies.Append("email", dt.Rows[0]["email"].ToString(), new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    });
                    Response.Cookies.Append("id", dt.Rows[0]["id"].ToString(), new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    });

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            return View();
        } 
        public IActionResult Dashboard()
        {
            if (Request.Cookies["email"] != null)
            {
                ViewBag.email = Request.Cookies["email"];
                DataTable dt = db.table("sp_SignUp", new SqlParameter[]
                {
                    new SqlParameter("@action","SelectOne"),
                    new SqlParameter("@email",Request.Cookies["email"].ToString()),
                });

                if(dt.Rows.Count > 0)
                {
                    ViewBag.id = dt.Rows[0]["id"];
                    ViewBag.name = dt.Rows[0]["name"];
                    ViewBag.img = dt.Rows[0]["img"];
                    ViewBag.isActive = dt.Rows[0]["isActive"];
                    return View();
                }
                else
                {
                    return RedirectToAction("Login");
                }
             
            }
            else
            {
                return RedirectToAction("Login");
            }
             
        }

        public IActionResult ForgotPass()
        {
            return View();
        }
        

        [HttpPost]
        public IActionResult SendOTP(string? email)
        {
            Random r = new Random();
            int otp = r.Next(111111, 999999);
            HttpContext.Session.SetInt32("otp" , otp);
            HttpContext.Session.SetString("email",email);

           string AdminEmail = _con["EmailSettings:AdminEmail"];
           string AdminPass = _con["EmailSettings:AdminPass"];

            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port =587,
                EnableSsl = true,
                Credentials = new NetworkCredential(AdminEmail, AdminPass)
            };

            MailMessage msg = new MailMessage(
                new MailAddress(AdminEmail, "Lalit Dubey"),
                new MailAddress(email)
                );

            msg.Subject = "OTP";
            msg.Body = $"<h1>Your OTP is : {otp}</h1>";
            msg.IsBodyHtml = true;
            smtp.Send(msg);

            return Json(new {success = true});
        } 
        
        public IActionResult VerifyOTP()
        {
            return View();
        } 

        [HttpPost]
        public IActionResult VerifyOTP(int? otp)
        {
           int? sOTP = HttpContext.Session.GetInt32("otp"); 

            if (sOTP == otp)
            {
                HttpContext.Session.Remove("otp");
                return RedirectToAction("ChangePass");
            }
            else
            {
                return View();
            }
        } 
        public IActionResult ChangePass()
        {
            return View();
        } 

        [HttpPost]
        public IActionResult ChangePass(string? pass)
        {
            string email = HttpContext.Session.GetString("email");

            string HashPass = BCrypt.Net.BCrypt.HashPassword(pass);

            SqlParameter msg = new SqlParameter("@msg", SqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            db.ExecuteQuery("sp_SignUp", new SqlParameter[]
            {
                new SqlParameter("@action" , "ChangePass"),
                new SqlParameter("@pass" , HashPass),
                new SqlParameter("@email" , email),
                msg
            });
            string Rmsg = msg.Value.ToString();
            if (Rmsg == "Success")
            {
                return Json(new { success = true });
            }
            else
            {
                return View();
            }
        }
        public IActionResult Logout()
        {
            Response.Cookies.Delete("email");
            Response.Cookies.Delete("id");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        } 
    }
}
