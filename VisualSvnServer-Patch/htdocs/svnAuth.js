String.prototype.encodeURI=function() {var rS; rS=escape(this); return rS.replace(/\+/g,"%2B");};
function doChange() 
{ 
	if (document.getElementById("creatInfo")) $("#creatInfo").remove();
	if (!document.getElementById("pwInfo"))
	{
		$('<div id="pwInfo" style="background-color:#f3f3f3;padding:5px;"> '
		+'用户名：<input type="text" id="username" size="10" /> '
		+'密 码：<input type="password" id="oldpwd" size="8" /> '
		+'新密码：<input type="password" id="newpwd" size="8" /> '
		+'新密码确认：<input type="password" id="newpwdCfm" size="8" /> '
		+'<input type="button" id="btnSubmit" value="确认修改" /> '
		+'<input type="button" id="btnCancel" value="取消修改" /> '
		+'</div>').insertBefore("#header");
		$("#btnSubmit").bind("click", execChange);
		$("#btnCancel").bind("click", function() { $("#pwInfo").remove(); } );
	}
}

function execChange()
{
	var u=function(txt) 
	{ 
		if (txt=="0")
		{
			alert("操作成功！");
			$("#pwInfo").remove();
		}
		else
		{
			alert(txt);
			$("#btnSubmit").attr("value", "确认修改").removeAttr("disabled");
		}
	};
	$("#btnSubmit").attr("value", "请稍等...").attr("disabled","disabled");
	jQuery.post("/ApacheChange.aspx",
		"username="+$("#username").val().encodeURI()
		+"&oldpwd="+$("#oldpwd").val().encodeURI()
		+"&newpwd="+$("#newpwd").val().encodeURI()
		+"&newpwdcfm="+$("#newpwdCfm").val().encodeURI(),
	u)
}

function doCreate() 
{ 
	if (document.getElementById("pwInfo")) $("#pwInfo").remove();
	if (!document.getElementById("creatInfo"))
	{
		$('<div id="creatInfo" style="background-color:#D1EDD1;padding:5px;"> '
		+'用户名：<input type="text" id="usernameApply" size="10" /> '
		+'登录密码：<input type="password" id="pwd" size="8" /> '
		+'密码确认：<input type="password" id="pwdCfm" size="8" /> '
		+'<input type="button" id="btnApply" value="确认申请" /> '
		+'<input type="button" id="btnApplyCancel" value="取消申请" /> '
		+'</div>').insertBefore("#header");
		$("#btnApply").bind("click", execCreat);
		$("#btnApplyCancel").bind("click", function() { $("#creatInfo").remove(); } );
	}
}

function execCreat()
{
	var u=function(txt) 
	{ 
		if (txt=="0")
		{
			alert("操作成功！");
			$("#creatInfo").remove();
		}
		else
		{
			alert(txt);
			$("#btnApply").attr("value", "确认申请").removeAttr("disabled");
		}
	};
	$("#btnApply").attr("value", "请稍等...").attr("disabled","disabled");
	jQuery.post("/ApacheChange.aspx",
		"username="+$("#usernameApply").val().encodeURI()
		+"&pwd="+$("#pwd").val().encodeURI()
		+"&pwdcfm="+$("#pwdCfm").val().encodeURI(),
	u)
}


$(document).bind('ready',function(){
	if (document.location.href.toLowerCase().indexOf("/svnauth/")!=-1)
	{
		$("#lnkModify").before('<a href="javascript:doCreate();void(0);" style="color:green;text-decoration:none;">创建新用户</a>&nbsp;&nbsp;'); 
	}
});