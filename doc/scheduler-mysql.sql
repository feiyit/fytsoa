-- MySQL tables for FytSoa.Common.Scheduler (StoreType = MySql)

CREATE TABLE IF NOT EXISTS `sys_scheduler_task` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TaskName` varchar(100) NOT NULL,
  `GroupName` varchar(100) NOT NULL,
  `Interval` varchar(100) NOT NULL,
  `ApiUrl` varchar(500) DEFAULT NULL,
  `Describe` varchar(200) DEFAULT NULL,
  `LastRunTime` datetime DEFAULT NULL,
  `Status` int NOT NULL,
  `TaskType` int NOT NULL,
  `ApiRequestType` varchar(20) DEFAULT NULL,
  `ApiAuthKey` varchar(50) DEFAULT NULL,
  `ApiAuthValue` varchar(500) DEFAULT NULL,
  `ApiParameter` text DEFAULT NULL,
  `DllClassName` varchar(300) DEFAULT NULL,
  `DllActionName` varchar(100) DEFAULT NULL,
  `TimeFlag` datetime DEFAULT NULL,
  `ChangeTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_task_identity` (`GroupName`, `TaskName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Scheduler tasks';

CREATE TABLE IF NOT EXISTS `sys_scheduler_task_log` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TaskName` varchar(100) NOT NULL,
  `GroupName` varchar(100) NOT NULL,
  `BeginDate` datetime NOT NULL,
  `EndDate` datetime DEFAULT NULL,
  `Msg` text DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `idx_task_begin` (`GroupName`, `TaskName`, `BeginDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Scheduler task logs';

