$(function(){
    $('.add-focus').click(function(){
        le.addFocus();
    });
});
var le={
    addFocus(){
        $.ajax({
            type:"POST",
            url: '/lecturer?handler=AddFocus',
            contentType:"application/json; charset=utf-8",
            data:JSON.stringify({id:$('.lecut-id').val()}),
            headers: {
                RequestVerificationToken: $( "input[name='__RequestVerificationToken']").val()
            },
        }).done(function (data) {
            if(data.statusCode===200){
                UIkit.notification({message: '关注成功~', status: 'success'})
                setTimeout(function(){
                    window.location.reload()
                },1000);
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
    }
};