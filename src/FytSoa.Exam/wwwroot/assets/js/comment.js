var vm=new Vue({
   el:"#app",
   data:{
       list:[],
       replyText:''
   },
    methods:{
        replyUser(uid,cid){
            c.replyAdd(uid,cid);
        }
    }
});

$(function(){
    c.init();
    $("#goComment").click(function(){
        c.submit();
    });
    $('.add-focus').click(function(){
        c.addFocus();
    });
    $(".uil-thumbs-up").click(function(){
        c.support(1);
    });
    $(".uil-thumbs-down").click(function(){
        c.support(0);
    });
    
});

var c={
    submit(){
        var commentText=$("#comment-text").val();
        var userId=$(".user-id").val();
        var token=$( "input[name='__RequestVerificationToken']").val();
        var id=$(".cid").val();
        if(!userId){
            swal({
                text: "您必须登录，才可以发表评论!",
                icon: "warning",
                buttons: ["取消", "去登录"],
            })
                .then((willDelete) => {
                    if (willDelete) {
                        window.location.href="/user/login";
                    } 
                });
            return;
        }
        if(commentText.length<3){
            UIkit.notification("评论内容不能为空，长度不能小于3位", {status:'warning'})
            return;
        }
        var data={
            userId:userId,
            categoryId:id,
            text:commentText,
        };
        $.ajax({
            type:"POST",
            url: '/common?handler=AddComment',
            contentType:"application/json; charset=utf-8",
            headers: {
                RequestVerificationToken: token
            },
            data:JSON.stringify(data),
        }).done(function (data) {
            if(data.statusCode===200){
                UIkit.notification({message: '评论添加成功，请等待审核!', status: 'success'})
                $("#comment-text").val('');
                c.init();
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
    },
    replyAdd(uid,cid){
        var userId=$(".user-id").val();
        if(!userId){
            swal({
                text: "您必须登录，才可以发表评论!",
                icon: "warning",
                buttons: ["取消", "去登录"],
            })
                .then((willDelete) => {
                    if (willDelete) {
                        window.location.href="/user/login";
                    }
                });
            return;
        }
        var token=$( "input[name='__RequestVerificationToken']").val();
        var id=$(".cid").val();
        if(vm.replyText.length<3){
            UIkit.notification("评论内容不能为空，长度不能小于3位", {status:'warning'})
            return;
        }
        var data={
            userId:userId,
            categoryId:id,
            text:vm.replyText,
            byUserId:uid,
            commentId:cid
        };
        $.ajax({
            type:"POST",
            url: '/common?handler=AddCommentReply',
            contentType:"application/json; charset=utf-8",
            headers: {
                RequestVerificationToken: token
            },
            data:JSON.stringify(data),
        }).done(function (data) {
            if(data.statusCode===200){
                UIkit.notification({message: '评论添加成功，请等待审核!', status: 'success'})
                $(".comment-reply,a.reply-close").addClass("hidden");
                $("a.reply").removeClass("hidden");
                vm.replyText='';
                c.init();
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
    },
    init(){
        var token=$( "input[name='__RequestVerificationToken']").val();
        var id=$(".cid").val();
        var data={
            id:id,
            status:'1',
            page:1,
            limit:10
        };
        $.ajax({
            type:"POST",
            url: '/common?handler=init',
            contentType:"application/json; charset=utf-8",
            headers: {
                RequestVerificationToken: token
            },
            data:JSON.stringify(data),
        }).done(function (data) {
            vm.list=data;
            vm.$nextTick(function(){
                $("a.reply").click(function(){
                    $(".comment-reply,a.reply-close").addClass("hidden");
                    $("a.reply").removeClass("hidden");
                    $(this).closest("li").find(".comment-reply:first").removeClass("hidden");
                    $(this).closest("li").find("a.reply-close:first").removeClass("hidden");
                    $(this).addClass("hidden");
                });
                $("a.reply-close").click(function(){
                    vm.replyText='';
                    $(this).closest("li").find(".comment-reply:first").addClass("hidden");
                    $(this).closest("li").find("a.reply:first").removeClass("hidden");
                    $(this).addClass("hidden");
                });
            });
        })
    },
    support(type){
        var id=$(".cid").val();
        var userId=$(".user-id").val();
        if(!userId){
            swal({
                text: "您必须登录，才可以发表评论!",
                icon: "warning",
                buttons: ["取消", "去登录"],
            })
                .then((willDelete) => {
                    if (willDelete) {
                        window.location.href="/user/login";
                    }
                });
            return;
        }
        $.ajax({
            type:"POST",
            url: '/common?handler=support',
            contentType:"application/json; charset=utf-8",
            data:JSON.stringify({id:id,type:type}),
            headers: {
                RequestVerificationToken: $( "input[name='__RequestVerificationToken']").val()
            },
        }).done(function (data) {
            if(data.statusCode===200){
                UIkit.notification({message: '操作成功~', status: 'success'})
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
    },
    addFocus(){
        var userId=$(".user-id").val();
        if(!userId){
            swal({
                text: "您必须登录，才可以发表评论!",
                icon: "warning",
                buttons: ["取消", "去登录"],
            })
                .then((willDelete) => {
                    if (willDelete) {
                        window.location.href="/user/login";
                    }
                });
            return;
        }
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
                var sum=$('.subs-amount').html();
                sum=parseInt(sum)+1;
                $('.subs-amount').html(sum);
            }else{
                UIkit.notification({message: '服务端发生错误...', status: 'danger'})
            }
        })
    }
};