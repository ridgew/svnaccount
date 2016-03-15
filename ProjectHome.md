目前实现：
  * 实现用户在线修改svn密码
  * 实现用户在线创建新帐号
  * 支持Apache自身的Basic文件方式认证和基于windows帐户信息的认证修改。


需求平台软件：
  * Windows OS
  * Apache版本 2.2.9
  * Net Framework 2.0


技术应用:
  * NTLM,LADP
  * WebClient with SSL
  * jQuery
  * XSLT


其他:
  * 配置用于程序使用的系统管理员帐户资料
  * <font color='red'>GAC(Global Assembly Cache)目录中注册Apache的aspdotnet模块</font>



实现图例:

> 创建帐户：
> > <img src='http://svnaccount.googlecode.com/svn/wiki/images/Panel-Create.jpg' />


> 修改帐户：
> > <img src='http://svnaccount.googlecode.com/svn/wiki/images/Panel-Modify.jpg' />