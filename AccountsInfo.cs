using System;
using System.Collections.Generic;
using System.Text;

namespace iMCService
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 经Javascript加密后的密码
        /// </summary>
        public string Password { get; set; }
    }

}
