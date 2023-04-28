using System;
using System.Collections.Generic;
using SqlSugar;

namespace FytSoa.Generator
{
    /// <summary>
    /// 连接对象
    /// </summary>
    public class GeneratorDto
    {
        public GeneratorDto(string ip, string port, string name, string passWord, string dbName)
        {
            Ip = ip;
            Port = port;
            Name = name;
            PassWord = passWord;
            DbName = dbName;
        }

        public string Ip { get; set; }

        public string Port { get; set; }

        public string Name { get; set; }

        public string PassWord { get; set; }

        public string DbName { get; set; }
    }

    /// <summary>
    /// 生成的对象
    /// </summary>
    public class GeneratorTableDto
    {
        /// <summary>
        /// 数据库表名字  例如：sys_admin
        /// </summary>
        public string[] TableNames { get; set; }

        /// <summary>
        /// 命名空间，根据不同的业务，分文件夹=命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 生成类型 1=全部表   2=部分表
        /// </summary>
        public int Types { get; set; } = 1;

        /// <summary>
        /// 添加/编辑 是否增加栅格
        /// </summary>
        public bool IsGrid { get; set; } = false;
        
        /// <summary>
        /// Api版本
        /// </summary>
        public string ApiVersion { get; set; } = "v1";

        /// <summary>
        /// 字典属性
        /// </summary>
        public List<GeneratorTable> TableColumnInfo { get; set; }
    }

    public class GeneratorTable : DbColumnInfo
    {
        /// <summary>
        /// 是否列表展示
        /// </summary>
        public bool IsColumn  {get; set;}= false;
        
        /// <summary>
        /// 是否增加搜索条件
        /// </summary>
        public bool IsSearch  {get; set;}= false;
        
        /// <summary>
        /// 是否添加
        /// </summary>
        public bool IsAdd  {get; set;}= false;
        
        /// <summary>
        /// 必填项
        /// </summary>
        public bool Required  {get; set;}= false;

        /// <summary>
        /// 组件类型
        /// </summary>
        public string ComponentType { get; set; }
    }
}
