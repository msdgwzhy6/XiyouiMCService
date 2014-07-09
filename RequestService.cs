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
using System.Net.NetworkInformation;

namespace iMCService
{
    public enum ResType{
        FIRST,RENEW
    }

    public delegate void ResponseHandler(string resStr);
    partial class RequestService : ServiceBase
    {
        public static ResponseHandler OnResponseFinished;

        public event ResponseHandler ResponseFinished
        {
            add
            {
                OnResponseFinished += new ResponseHandler(value);
            }
            remove
            {
                OnResponseFinished -= new ResponseHandler(value);
            }
        }

        string username, password;
        DateTime startTime, endTime;
        Thread a,b;
        public RequestService()
        {
            InitializeComponent();
            this.ResponseFinished += RequestService_ResponseFinished;
        }

        void RequestService_ResponseFinished(string resStr)
        {
            if (GlobalType == ResType.FIRST)
            {
                try
                {
                    JObject obj = JObject.Parse(resStr);
                    Program.serialNo = (string)obj["serialNo"];
                    Program.userDevPort = (string)obj["userDevPort"];
                }
                catch (Exception e)
                {
                    EventLog.WriteEntry(e.Message);
                }
            }
            else
            {
                JObject obj = JObject.Parse(resStr);
                if ((int)obj["errorNumber"] == 1)
                {
                }
                else
                {
                    FirstReq();
                }
            }
        }

        public static Accounts accounts = new Accounts();
        protected override void OnStart(string[] args)
        {
            username = Accounts.AccountsData[0].UserName;
            password = Accounts.AccountsData[0].Password;
            
            a = new Thread(DoReq);
            a.Start();
            b = new Thread(NetworkTest);
            b.IsBackground = true;
            b.Start();
        }

        void NetworkTest()
        {
            Ping ping = new Ping();
            PingOptions po = new PingOptions();
            po.DontFragment = true;
            string data = "abc";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingReply reply = ping.Send(IPAddress.Parse("218.30.19.50"), timeout, buffer, po);
            if (reply.Status == IPStatus.Success)
            {
                Thread.Sleep(TimeSpan.FromMinutes(3));
            }
            else
            {
                a.Abort();
                a = new Thread(DoReq);
                a.Start();
            }
        }

        void DoReq()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            FirstReq();
            while (true)
            {
                if (string.IsNullOrEmpty(Program.serialNo) || string.IsNullOrEmpty(Program.userDevPort))
                {
                    FirstReq();
                }
                else
                { 
                    RenewReq(); 
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(300000));
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
        void FirstReq()
        {
            startTime = DateTime.Now;
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

            GlobalType = ResType.FIRST;
            req.BeginGetRequestStream(new AsyncCallback(PostParam), req);
        }

        ResType GlobalType;

        string Params;

        void PostParam(IAsyncResult result)
        {
            if (GlobalType == ResType.FIRST)
                Params =
                    "userName=" + username + "&userPwd=" + password
                    + "&serviceType=&isSavePwd=on&userurl=&userip=&basip=&language=Chinese&portalProxyIP=222.24.19.190&portalProxyPort=50200&dcPwdNeedEncrypt=1&assignIpType=0&appRootUrl=http%3A%2F%2F222.24.19.190%3A8080%2Fportal%2F&manualUrl=&manualUrlEncryptKey=rTCZGLy2wJkfobFEj0JF8A%3D%3D";
            else
                Params =
                "userDevPort=" + Program.userDevPort + "&serialNo=" + Program.serialNo +
                "&userip=&basip=&userStatus=99&language=Chinese&e_d=";

            HttpWebRequest req = (HttpWebRequest)result.AsyncState;
            
            using (Stream stream = req.EndGetRequestStream(result))
            {
                byte[] bin = Encoding.UTF8.GetBytes(Params);
                stream.Write(bin, 0, bin.Length);
            }

            req.BeginGetResponse(new AsyncCallback(GetRes), req);
        }

        void GetRes(IAsyncResult result)
        {
            HttpWebRequest req = (HttpWebRequest)result.AsyncState;
            string resStr = "";
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.EndGetResponse(result);
                using (Stream stream = res.GetResponseStream())
                {
                    endTime = DateTime.Now;
                    StreamReader reader = new StreamReader(stream);
                    resStr = reader.ReadToEnd();
                    
                    EventLog.WriteEntry("耗时：" + (endTime - startTime).TotalMilliseconds);
                    OnResponseFinished(resStr); 
                    
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(e.Message);
            }
        }


        /// <summary>
        /// 后继请求，即续租
        /// </summary>
        void RenewReq()
        {
            DateTime startTime = DateTime.Now;
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

            req.BeginGetRequestStream(new AsyncCallback(PostParam), req);
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
