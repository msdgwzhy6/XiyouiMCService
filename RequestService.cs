using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace iMCService
{
    partial class RequestService : ServiceBase
    {
        string username, password;
        Thread a;
        public RequestService()
        {
            InitializeComponent();
        }
        public static Accounts accounts = new Accounts();
        protected override void OnStart(string[] args)
        {
            username = Accounts.AccountsData[0].UserName;
            password = Accounts.AccountsData[0].Password;
            
            a = new Thread(DoReq);
            a.Start();
        }

        void DoReq()
        {
            FirstReq(username, password);
            while (true)
            {
                if (string.IsNullOrEmpty(Program.serialNo) || string.IsNullOrEmpty(Program.userDevPort))
                {
                    FirstReq(username,password);
                }
                else if (!RenewReq(username, password))
                {
                    FirstReq(username, password);
                }
                Thread.Sleep(TimeSpan.FromMinutes(10));
            }
        }

        protected override void OnStop()
        {
            // TODO:  在此处添加代码以执行停止服务所需的关闭操作。
            Logout();
            a.Abort();
        }

        /// <summary>
        /// 第一次请求，即登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="despassword">经过Javascript加密后的用户密码</param>
        bool FirstReq(string username, string despassword)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li");
            req.CookieContainer = new CookieContainer();
            CookieContainer cookiecontainer = new CookieContainer();

            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li"), "hello1=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li"), "hello2=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li"), "hello3=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li"), "hello4=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=li"), "i_p_pl=;");

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookiecontainer;
            req.UserAgent = "SB! Fuck You!";
            req.Headers["Accept-Language"] = "Mars Language";

            string Params =
                "userName=" + username + "&userPwd=" + despassword
                + "&serviceType=&isSavePwd=on&userurl=&userip=&basip=&language=Chinese&portalProxyIP=222.24.19.190&portalProxyPort=50200&dcPwdNeedEncrypt=1&assignIpType=0&appRootUrl=http%3A%2F%2F222.24.19.190%3A8080%2Fportal%2F&manualUrl=&manualUrlEncryptKey=rTCZGLy2wJkfobFEj0JF8A%3D%3D";

            using (Stream stream = req.GetRequestStream())
            {
                byte[] bin = Encoding.UTF8.GetBytes(Params);
                stream.Write(bin, 0, bin.Length);
            }

            string resStr = "";
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (Stream stream = res.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    resStr = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return false;
            }
            try
            {
                JObject obj = JObject.Parse(resStr);
                Program.serialNo = (string)obj["serialNo"];
                Program.userDevPort = (string)obj["userDevPort"];
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 后继请求，即续租
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="despassword">经过Javascript加密后的用户密码</param>
        bool RenewReq(string username, string despassword)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb");
            req.CookieContainer = new CookieContainer();
            CookieContainer cookiecontainer = new CookieContainer();

            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb"), "hello1=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb"), "hello2=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb"), "hello3=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb"), "hello4=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=hb"), "i_p_pl=;");

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookiecontainer;
            req.UserAgent = "SB! Fuck You!";
            req.Headers["Accept-Language"] = "Mars Language";

            string Params = 
                "userDevPort=" + Program.userDevPort + "&serialNo=" + Program.serialNo +
                "&userip=&basip=&userStatus=99&language=Chinese&e_d=";

            using (Stream stream = req.GetRequestStream())
            {
                byte[] bin = Encoding.UTF8.GetBytes(Params);
                stream.Write(bin, 0, bin.Length);
            }

            string resStr = "";
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (Stream stream = res.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    resStr = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return false;
            }
            try
            {
                JObject obj = JObject.Parse(resStr);
                if ((int)obj["errorNumber"] == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        void Logout()
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo");
            req.CookieContainer = new CookieContainer();
            CookieContainer cookiecontainer = new CookieContainer();

            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo"), "hello1=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo"), "hello2=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo"), "hello3=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo"), "hello4=;");
            cookiecontainer.SetCookies(new Uri("http://" + IPInfo.GetFullHost() + "/portal/pws?t=lo"), "i_p_pl=;");

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookiecontainer;
            req.UserAgent = "SB! Fuck You!";
            req.Headers["Accept-Language"] = "Mars Language";

            string Params =
                "userName=&userPwd=&serviceType=&isSavePwd=on&userurl=&userip=&basip=&language=Chinese&portalProxyIP=222.24.19.190&portalProxyPort=50200&dcPwdNeedEncrypt=1&assignIpType=0&appRootUrl=http%3A%2F%2F222.24.19.190%3A8080%2Fportal%2F&manualUrl=&manualUrlEncryptKey=rTCZGLy2wJkfobFEj0JF8A%3D%3D";

            using (Stream stream = req.GetRequestStream())
            {
                byte[] bin = Encoding.UTF8.GetBytes(Params);
                stream.Write(bin, 0, bin.Length);
            }

            string resStr = "";
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (Stream stream = res.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    resStr = reader.ReadToEnd();
                }
            }
            catch { }
        }
    }
}
