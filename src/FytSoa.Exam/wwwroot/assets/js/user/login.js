$(function(){
    $('#login').submit(function() {
        var email=$("#email").val(),password=$("#password").val();
        $('.login-sub').attr("disabled",true);
        $.ajax({
            type:"POST",
            url: '/user/login',
            contentType:"application/json; charset=utf-8",
            data:JSON.stringify({email:email,password:password}),
            headers: {
                RequestVerificationToken: $( "input[name='__RequestVerificationToken']").val()
            },
        }).done(function (data) {
            console.log('data',data)
            $('.login-sub').attr("disabled",false);
            if(data.statusCode===200){
                UIkit.notification({message: '登录成功~', status: 'success'})
                setTimeout(function(){
                    window.location.href="/";
                },1000);
            }else{
                UIkit.notification({message: data.content, status: 'danger'})
            }
        })
        return false;
    });
});