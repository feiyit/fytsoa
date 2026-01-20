import CryptoJS from 'crypto-js';

// 默认 AES 加密密钥和偏移量，实际项目中建议从环境变量或后端获取
const DEFAULT_AES_KEY = CryptoJS.enc.Utf8.parse('1XEmjM5b8pU3mOT9'); // 16 字节密钥
const DEFAULT_AES_IV = CryptoJS.enc.Utf8.parse('W9BVuk60EbxBpKo4'); // 16 字节偏移量

type AesKeyInput = CryptoJS.lib.WordArray | string | undefined;

const resolveKeyOrIv = (
  input: AesKeyInput,
  fallback: CryptoJS.lib.WordArray,
): CryptoJS.lib.WordArray => {
  if (!input) return fallback;
  if (typeof input === 'string') {
    return CryptoJS.enc.Utf8.parse(input);
  }
  return input;
};

/**
 * AES 加密（CBC / PKCS7）
 * @param data 待加密纯文本字符串
 * @param key  密钥，可以是 16/24/32 字符串或 WordArray，默认使用内置密钥
 * @param iv   偏移量，可以是 16 字符串或 WordArray，默认使用内置偏移量
 * @returns Base64 编码的密文
 */
export function aesEncrypt(
  data: string,
  key: AesKeyInput = DEFAULT_AES_KEY,
  iv: AesKeyInput = DEFAULT_AES_IV,
): string {
  const aesKey = resolveKeyOrIv(key, DEFAULT_AES_KEY);
  const aesIv = resolveKeyOrIv(iv, DEFAULT_AES_IV);

  const encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(data), aesKey, {
    iv: aesIv,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.Pkcs7,
  });
  return encrypted.toString();
}

/**
 * AES 解密（CBC / PKCS7）
 * @param encryptedData Base64 编码的密文
 * @param key           密钥，可以是 16/24/32 字符串或 WordArray，默认使用内置密钥
 * @param iv            偏移量，可以是 16 字符串或 WordArray，默认使用内置偏移量
 * @returns 解密后的原始字符串（解密失败时返回空字符串）
 */
export function aesDecrypt(
  encryptedData: string,
  key: AesKeyInput = DEFAULT_AES_KEY,
  iv: AesKeyInput = DEFAULT_AES_IV,
): string {
  try {
    const aesKey = resolveKeyOrIv(key, DEFAULT_AES_KEY);
    const aesIv = resolveKeyOrIv(iv, DEFAULT_AES_IV);

    const decrypted = CryptoJS.AES.decrypt(encryptedData, aesKey, {
      iv: aesIv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7,
    });
    return decrypted.toString(CryptoJS.enc.Utf8);
  } catch {
    // 非法密文或密钥错误时，统一返回空字符串，避免抛出异常
    return '';
  }
}

/**
 * AES 加密任意对象（先 JSON.stringify 再加密）
 */
export function aesEncryptObject<T>(
  data: T,
  key?: AesKeyInput,
  iv?: AesKeyInput,
): string {
  const json = JSON.stringify(data ?? null);
  return aesEncrypt(json, key, iv);
}

/**
 * AES 解密为对象
 * @returns 解析失败时返回 null
 */
export function aesDecryptToObject<T = unknown>(
  encryptedData: string,
  key?: AesKeyInput,
  iv?: AesKeyInput,
): T | null {
  const json = aesDecrypt(encryptedData, key, iv);
  if (!json) return null;
  try {
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}

/**
 * MD5加密
 * @param data 待加密数据
 * @returns 加密后的32位字符串
 */
export function md5Encrypt(data: string): string {
  return CryptoJS.MD5(data).toString();
}

/**
 * SHA1加密
 * @param data 待加密数据
 * @returns 加密后的字符串
 */
export function sha1Encrypt(data: string): string {
  return CryptoJS.SHA1(data).toString();
}

/**
 * SHA256加密
 * @param data 待加密数据
 * @returns 加密后的字符串
 */
export function sha256Encrypt(data: string): string {
  return CryptoJS.SHA256(data).toString();
}

/**
 * 字符串Base64编码
 * @param data 待编码字符串
 * @returns Base64编码后字符串
 */
export function base64Encode(data: string): string {
  return CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(data));
}

/**
 * Base64解码
 * @param data 待解码Base64字符串
 * @returns 解码后原始字符串
 */
export function base64Decode(data: string): string {
  return CryptoJS.enc.Base64.parse(data).toString(CryptoJS.enc.Utf8);
}

/**
 * 生成指定长度的随机密钥（仅用于一般用途，不适合作为强密码）
 * @param length 字符长度，默认 16
 * @returns 随机字符串
 */
export function generateRandomKey(length: number = 16): string {
  const chars =
    'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let key = '';
  for (let i = 0; i < length; i++) {
    key += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return key;
}

// 同时提供一个包含所有方法的对象，方便整体导入
export const CryptoUtil = {
  aesEncrypt,
  aesDecrypt,
  aesEncryptObject,
  aesDecryptToObject,
  md5Encrypt,
  sha1Encrypt,
  sha256Encrypt,
  base64Encode,
  base64Decode,
  generateRandomKey,
  // 导出默认密钥和偏移量（谨慎使用，生产环境建议从环境变量获取）
  DEFAULT_AES_KEY,
  DEFAULT_AES_IV,
};

// 兼容之前的实例导出方式
export const cryptoUtil = CryptoUtil;
