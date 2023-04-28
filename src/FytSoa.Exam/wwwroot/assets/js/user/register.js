$(function(){
    $('#register').submit(function() {
        var nickName=$("#nickName").val(),email=$("#email").val(),password=$("#password").val(),confirmpwd=$("#confirmpwd").val();
        if(password.length<6){
            UIkit.notification({message: '密码长度不能小于6位，需要字母+数字+特殊字符组成...', status: 'danger'})
            return false;
        }
        if(password!==confirmpwd){
            UIkit.notification({message: '两次输入的密码不一致...', status: 'danger'})
            return false;
        }
        var data={
            nickName:nickName,
            email:email,
            password:password
        };
        $('.register-sub').attr("disabled",true);
        $.ajax({
            type:"POST",
            url: '/user/register',
            contentType:"application/json; charset=utf-8",
            data:JSON.stringify(data),
            headers: {
                RequestVerificationToken: $( "input[name='__RequestVerificationToken']").val()
            },
        }).done(function (data) {
            $('.register-sub').attr("disabled",false);
            if(data.statusCode===200){
                UIkit.notification({message: '注册成功~', status: 'success'})
                setTimeout(function(){
                   window.location.href="/user/login"; 
                },1000);
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
        return false;
    });
});