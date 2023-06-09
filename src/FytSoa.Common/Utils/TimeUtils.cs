﻿namespace FytSoa.Common.Utils;

public static class TimeUtils
{
    /// <summary>
    /// 分割时间，查询作用
    /// </summary>
    /// <param name="timeStr"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static (string beginTime,string endTime) Splitting(string timeStr, char split = '/')
    {
        var time = timeStr.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
        return (time[0],time[1]);
    }
}