XiyouiMCService
===============

西邮iMC拨号Windows服务程序，可设置开机自动运行，即不用再打开网页拨号

### 系统要求：

Windows XP、2003、Vista、7、8，Windows Server 2003、2008、2012，32位或64位操作系统

Microsoft .NET Framework 2.0

### 部署方法：

（后期将实现服务管理器，即可自动执行部署、卸载操作！）

1、将iMCSettings.xml中的用户名和密码修改为自己的账名和密码（密码为MD5加密后的密文）。

2、将iMCSettings.xml文件拷贝至X:\Windows\System32\目录下，X为Windows安装盘，下同。

3、运行cmd，进入X:\Windows\Microsoft.NET\Framework64\v2.0.50727\（32位），或X:\Windows\Microsoft.NET\Framework64\v2.0.50727\（64位）

4、执行如下命令：installutil Z:\iMCService\iMCService.exe，Z为iMCService文件夹位置。

5、按回车进行部署，成功后执行net start imcservice即可。

### 卸载方法：

1.完全移除服务：仍需按部署方法中的第3步，进入v2.0.50727文件夹，执行installutil /u Z:\iMCService\iMCService.exe即可。

2.在“管理”中禁用服务即可。

### 仅限西邮校园内使用，其他网络环境不保证可用！

如有疑问请发Email至：yuanguozheng@outlook.com
