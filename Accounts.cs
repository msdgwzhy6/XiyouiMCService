using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace iMCService
{
    /// <summary>
    /// 存放用户信息
    /// </summary>
    public class Accounts
    {
        public static int i = -1;

        public static List<AccountInfo> AccountsData = new List<AccountInfo>();

        void LoadXML()
        {
            XmlTextReader reader = new XmlTextReader("iMCSettings.xml");
            while (reader.Read())
            {
                if (reader.Name == "item")
                {
                    AccountInfo account = new AccountInfo
                    {
                        UserName = reader.GetAttribute("UserName"),
                        Password = reader.GetAttribute("Password"),
                    };
                    AccountsData.Add(account);
                }
            }
            reader.Close();
        }

        public Accounts()
        {
            try
            {
                LoadXML();
            }
            catch
            {
                //Log.SaveLog(DateTime.Now + " 加载账户信息文件失败");
                //Console.WriteLine("加载账户信息文件失败");
            }
        }
    }
}
