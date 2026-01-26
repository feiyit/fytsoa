/**
 * 处理URL地址，自动拼接服务器地址（如果需要）
 * @param url 待处理的URL地址
 * @param envKey 环境变量中服务器地址的键名，默认为VITE_APP_API_BASE_URL
 * @returns 处理后的完整URL
 */
export function resolveUrl(
  url: string,
  envKey: string = 'VITE_API_BASE_URL',
): string {
  // 检查URL是否以http://或https://开头
  const isAbsoluteUrl = /^https?:\/\//i.test(url);

  if (isAbsoluteUrl) {
    // 如果是绝对URL，直接返回
    return url;
  }

  // 从环境变量获取服务器基础地址
  let baseUrl = import.meta.env[envKey] as string;
  baseUrl = baseUrl.replace(/\/api/gi, '');
  if (!baseUrl) {
    console.warn(`环境变量${envKey}未配置，将直接返回相对路径: ${url}`);
    return url;
  }

  // 处理基础地址和相对路径的拼接，避免重复的斜杠
  const normalizedBaseUrl = baseUrl.replace(/\/$/, '');
  const normalizedUrl = url.replace(/^\//, '');

  return `${normalizedBaseUrl}/${normalizedUrl}`;
}

export function uuid(length = 32) {
  const num = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890';
  let str = '';
  for (let i = 0; i < length; i++) {
    str += num.charAt(Math.floor(Math.random() * num.length));
  }
  return str;
}

/**
 * 生成业务编码（通用）
 * 示例：generateCode("CK-") => CK-1A2B3C4D
 * - suffix 仅包含字母+数字（大写）
 * - 适用于：仓库/单据/计划等需要“前缀 + 随机码”的场景
 */
export function generateCode(prefix: string, length: number = 8) {
  return `${prefix || ''}${uuid(length).toUpperCase()}`;
}

/**
 * 列表转树形结构
 */
export function changeTree(data: any) {
  if (data.length > 0) {
    data.forEach((item: any) => {
      const parentId = item.parentId;
      if (parentId) {
        data.forEach((ele: any) => {
          if (ele.id === parentId) {
            let childArray = ele.children;
            if (!childArray) {
              childArray = [];
            }
            childArray.push(item);
            ele.children = childArray;
          }
        });
      }
    });
  }
  return data.filter((item: any) => item.parentId == '0');
}

/**
 * 增强版：根据服务器返回的日期，返回精细化的人性化时间描述
 * 支持：刚刚/XX秒前/XX分钟前/XX小时前/今天 XX:XX/昨天/XX天前/具体日期
 * @param serverDate 服务器返回的日期（字符串/时间戳/Date对象）
 * @param dateFormat 非近期日期的格式化模板（默认：yyyy-MM-dd）
 * @param timeFormat 当天时间的格式化模板（默认：HH:mm）
 * @returns 精细化的时间描述
 */
export function formatRelativeDateDetail(
  serverDate: string | number | Date,
  dateFormat: string = 'yyyy-MM-dd',
  timeFormat: string = 'HH:mm'
): string {
  try {
    // 1. 转换并校验目标日期
    const targetDate = new Date(serverDate);
    if (isNaN(targetDate.getTime())) {
      throw new Error('无效的日期格式');
    }

    // 2. 获取当前时间戳（毫秒）
    const nowTime = Date.now();
    // 目标时间戳（毫秒）
    const targetTime = targetDate.getTime();

    // 3. 计算时间差（秒）
    const diffSeconds = Math.floor((nowTime - targetTime) / 1000);

    // ------------- 精细化时间判断 -------------
    // 小于10秒：刚刚
    if (diffSeconds < 10) {
      return '刚刚';
    }
    // 10秒~1分钟：XX秒前
    else if (diffSeconds < 60) {
      return `${diffSeconds}秒前`;
    }
    // 1分钟~1小时：XX分钟前
    else if (diffSeconds < 3600) {
      const minutes = Math.floor(diffSeconds / 60);
      return `${minutes}分钟前`;
    }

    // ------------- 小时/天级判断 -------------
    // 生成当前日期的凌晨零点（消除时分秒）
    const now = new Date();
    const todayZero = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
    // 生成目标日期的凌晨零点
    const targetZero = new Date(
      targetDate.getFullYear(),
      targetDate.getMonth(),
      targetDate.getDate()
    ).getTime();

    // 计算天数差
    const diffDays = Math.floor((todayZero - targetZero) / (1000 * 60 * 60 * 24));

    // 今天（1小时~24小时）：今天 XX:XX
    if (diffDays === 0) {
      const hours = Math.floor(diffSeconds / 3600);
      // 1~6小时可显示“XX小时前”，6小时后显示“今天 XX:XX”（可根据需求调整）
      if (hours < 6) {
        return `${hours}小时前`;
      } else {
        return `今天 ${formatTime(targetDate, timeFormat)}`;
      }
    }
    // 昨天
    else if (diffDays === 1) {
      return '昨天';
    }
    // 2~7天：XX天前
    else if (diffDays > 1 && diffDays <= 10) {
      return `${diffDays}天前`;
    }
    // 未来日期（服务器时间超前）
    else if (diffDays < 0) {
      const futureDays = Math.abs(diffDays);
      return futureDays === 1 ? '明天' : `${futureDays}天后`;
    }
    // 超过7天：格式化日期
    else {
      return formatDate(targetDate, dateFormat);
    }
  } catch (error) {
    console.error('日期格式化失败：', error);
    return '未知时间';
  }
}


/**
 * 辅助函数：格式化日期（yyyy-MM-dd）
 */
function formatDate(date: Date, format: string): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return format.replace('yyyy', year.toString()).replace('MM', month).replace('dd', day);
}

/**
 * 辅助函数：格式化时间（HH:mm）
 */
function formatTime(date: Date, format: string): string {
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  return format.replace('HH', hours).replace('mm', minutes);
}
