using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace iMCService
{
    static class Program
    {
        

        public static string serialNo = null;
        
        public static string userDevPort = null;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new RequestService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
