# **web.config配置详细说明** #

只有通过正确的配置web.config文件，svnaccount系统才能正常运转起来。目前的配置项中只有一项不需配置，其他都是必需配置项。


# Introduction #

通过阅读本页，一般可以正常配置svnaccount的运行环境。


# Details #
```

<!--// 设置系统管理员模拟帐号 -->
<add key="AnalogueID" value="svn" />
<!--// 设置系统管理员模拟帐号的加密密码 -->
<add key="AnaloguePWD" value="QGqIuSEuGdBnSgkZmJhS6g==" />

---
注：绑定所安装的windows服务器的管理员帐户信息。
以上所示表示：该服务器存在一个全称为svn的管理员帐户,且密码是：svnservice。
因系统内部需要直接操作服务器内部用户帐户（windows认证模式）以及运行相关exe应用辅助工具
(VisualSVN Server自身认证，Apache创建修改密码工具)。


<!--//不允许在线修改的用户名-->
<add key="DisabledModifyName" value="svn,administrator" />
---
注：保护系统内帐户免遭修改。

<!--//SVN系统服务名称-->
<add key="ApacheService" value="VisualSVNServer" />
--- 
注：此项备用

<!--//服务配置文件地址-->
<add key="SvnServerConf" value="C:\\Program Files\\VisualSVN Server\\conf\\httpd.conf"/>
---
注：系统内部判定认证模式使用

<!--//认证测试地址，这个地址所有的账号应该都没有访问并通过认证-->
<add key="AuthURL4Pass" value="https://127.0.0.1/svn/svnAuth"/>
---
注：测试旧有密码帐户信息的有效性地址，可以使用浏览器自己测试。

<!--//密码修改工具地址-->
<add key="htpasswdPath" value="C:\\Program Files\\VisualSVN Server\\htdocs\bin\\htpasswd.exe" />

<!--//自定义认证密码文件地址-->
<add key="htpasswdFile" value="D:\\Repositories\\htpasswd"/>

```