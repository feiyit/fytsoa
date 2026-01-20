<div align="center">
<img src="https://gitee.com/feiyit/fytsoa/raw/master/admin/public/img/logo.png" width="160" height="160" />
</div>

<div align="center"> 
<h1>FytSoa Admin</h1>
</div>

### 前端

[<img src=https://img.shields.io/badge/vue.js-3.x-red>](https://vuejs.org/) [<img src=https://img.shields.io/badge/element--plus-latest-yellow> ](https://element-plus.gitee.io/zh-CN/)

### 服务端

[<img src=https://img.shields.io/badge/netcore-6.x-success>](https://dotnet.microsoft.com/en-us/download) [<img src=https://img.shields.io/badge/orm-sqlsugar-yellow> ](https://www.donet5.com/Home/Doc) [<img src=https://img.shields.io/badge/cache-redis-blue> ](https://redis.io/)

#### 演示地址： [fytsoa](http://103.133.178.241:5100/admin/index.html)

```
账号：admin    密码：admin123
租户：fyadmin 密码：123456
```

#### Linux 部署： [园子](https://www.cnblogs.com/fuyu-blog/p/17367321.html)

### Swagger 增强 UI -关联项目

[fytapi.mui](https://gitee.com/feiyit/fytapi.mui)

### 介绍

FytSoa Admin 是一个快速搭建中后台解决方案，后台基于 NetCore 6 和前端 VUE3+Element+Plus 实现。  
使用最新的前沿技术栈，提供各类使用组件方便在业务开发时调用，并且持续性的提供丰富的业务模块，帮助你快速搭建企业级中后台任务。  
支持数据分离多租户平台（SaaS）

[前端基于 SCUI 搭建](https://gitee.com/lolicode/scui)

> 表格支持右击快捷键菜单  
> 表格自定义列-打印  
> 表格自定义列-导出

### 架构图

<div align="center">
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/jiagou.png width=80% />  
</div>

### 特点

> 模块化：全新的架构和模块化的开发机制，便于灵活扩展和二次开发

> 动态 API

> DDD 模式-领域驱动设计

### 技术点

> NetCore SDK 6.0+

> ORM SqlSugar

> Mysql

### 后台教程

```sh
# 数据库连接

在doc文件夹，通过数据库工具执行fytsoa.sql脚本

# 修改连接字符串

打开FytSoa.ApiService,找到appsetting.json修改链接字符串，如果是开发环境，

可以修改appsettings.Development.json

# 启动项目

打开终端，定位到FytSoa.ApiService目录，执行：dotnet run urls=http://*:5100

# 访问接口

打开浏览器：访问  http://localhost:5100/fytapiui/index.html
如看到Swagger增强FytApi.MUI接口文档说明项目启动成
```

### 前端安装教程

```sh
# 进入项目目录

cd admin

# 安装依赖

cnpm i  或者  npm i

# 启动项目(开发模式)

npm run serve
```

启动完成后浏览器访问 http://localhost:2800

### 项目截图

<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/home.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/22-12-01.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/22-12-02.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/22-12-03.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/dashboard.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/usercenter.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/admin.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/authorize.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/log.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/file.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/menu.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/message.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/site.jpg width=100% />  
<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/vote.jpg width=100% />

## 交流

<img src=https://gitee.com/feiyit/fytsoa/raw/master/doc/img/erweima-wx0509.jpg  />

## 支持

如果觉得本项目还不错或在工作中有所启发，请帮助当前项目点亮星星，这是对开发者最大的支持和鼓励！
