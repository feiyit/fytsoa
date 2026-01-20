using System.Runtime.InteropServices;
using Xabe.FFmpeg;
namespace FytSoa.Common.Utils;

/// <summary>
/// 视频处理
/// </summary>
public class FFmpegUtils
{
    public static async Task SnapshotImage(string inputFileName, string outFileName, TimeSpan position)
    {
        try
        {
            await FFmpeg.Conversions.New()
                .Start($" -i {inputFileName} -y -f image2 -ss {position.TotalSeconds} -t 0.001 {outFileName}");
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
    /// <summary>
    /// 借助ffmpeg生成缩略图
    /// </summary>
    /// <param name="originalFilePath">源文件</param>
    /// <param name="outputFilePath">新源文件</param>
    /// <param name="ffmpegPath">windows系统提供FFmpeg地址</param>
    public static void GenerateThumbnail(string originalFilePath,string outputFilePath,string ffmpegPath)
    {
        try
        {
            //判断系统类型
            //如果是windows，直接使用ffmpeg.exe
            //如果是linux，则使用安装的ffmpeg（需要提前安装）
            /*
              Linux工具调用：ffmpeg -i 333.jpg -q:v 31 -frames:v 1 -y image.jpg
              windows:  ffmpeg.exe -i 333.jpg -q:v 31 -frames:v 1 -y image.jpg

                  -i 333.jpg 是输入文件
                  -q:v 31 是质量，值区间是2-31
                  -frames:v 1 是提取帧必要参数
                  -y 是遇到同名文件则覆盖 
                  image.jpg 输出文件名
                  还可以加 -s 160*100 表示输出宽高比为160*100
             */
            string cmdPath = string.Empty;//ffmpeg工具对象
            string cmdParams = $" -ss 00:00:02 -i {originalFilePath} -f image2 -y {outputFilePath} ";//命令参数
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmdPath = ffmpegPath+"/ffmpeg.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmdPath = "ffmpeg";//安装ffmpeg工具
            }
            else
            {
                throw new Exception("当前操作系统不支持！");
            }

            using (System.Diagnostics.Process ffmpegProcess = new System.Diagnostics.Process())
            {
                // execute the process without opening a shell  
                ffmpegProcess.StartInfo.UseShellExecute = false; 
                //ffmpegProcess.StartInfo.ErrorDialog = false;  
                ffmpegProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                // redirect StandardError so we can parse it  
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                // set the file name of our process, including the full path  
                // (as well as quotes, as if you were calling it from the command-line)  
                ffmpegProcess.StartInfo.FileName = cmdPath;

                // set the command-line arguments of our process, including full paths of any files  
                // (as well as quotes, as if you were passing these arguments on the command-line)  
                ffmpegProcess.StartInfo.Arguments = cmdParams;

                ffmpegProcess.Start();// start the process  

                // now that the process is started, we can redirect output to the StreamReader we defined  

                ffmpegProcess.WaitForExit();// wait until ffmpeg comes back  

                //result = errorreader.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("生成缩略图出错！", ex);
        }
    }
}