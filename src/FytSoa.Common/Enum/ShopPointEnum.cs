using System.ComponentModel;

namespace FytSoa.Common.Enum;

public enum ShopPointEnum
{
    NewOrders,
    Completed
}

public enum ShopPointLogEnum
{
    [Description("游戏")]
    Game=1,
    [Description("签到")]
    SignIn=2,
    [Description("邀请好友")]
    Invitation=3,
    [Description("购买商品")]
    BuyProduct=4
}