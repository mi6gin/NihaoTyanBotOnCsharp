using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace NihaoTyan.Bot.commandsList.userCommands.YtDlp.Settings.Main
{
    public interface IVideoDownloader
    {
        Task DownloadVideoAsync(string videoUrl, Stream outputStream);
    }

    public static class VideoDownloaderHelper
    {
        // Определение пути к yt-dlp в зависимости от ОС
        private static string YtDlpPath => Path.Combine(
            "DreamGirl", "commandsList", "userCommands", "YtDlp", "Downloader", "Utilities",
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "yt-dlp_linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "yt-dlp_macos" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "yt-dlp.exe" :
            throw new PlatformNotSupportedException("Unsupported OS platform."));

        public static async Task DownloadAndSendVideoAsync(
            string videoUrl, TelegramBotClient botClient, Message message,
            int messageToDeleteId, Stream outputStream)
        {
            var chatId = message.Chat.Id;
            var caption = VideoUtils.FormatVideoCaption(
                VideoUtils.GetUsername(message),
                VideoUtils.DetermineVideoSource(videoUrl),
                videoUrl);

            try
            {
                await new YtDlpVideoDownloader(YtDlpPath).DownloadVideoAsync(videoUrl, outputStream);
                outputStream.Position = 0;
                await botClient.DeleteMessageAsync(chatId, messageToDeleteId);
                await botClient.SendVideoAsync(chatId,
                    new InputOnlineFile(outputStream, "video.mp4"),
                    caption: caption, parseMode: ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                await VideoUtils.SendErrorMessageAsync(botClient, chatId,
                    $"Не удалось скачать видео {videoUrl}\nОшибка: {ex.Message}");
            }
        }
    }

    public static class VideoUtils
    {
        public static string GetUsername(Message message) =>
            string.IsNullOrEmpty(message.From?.Username)
                ? $"*{message.From?.FirstName}*"
                : $"*{message.From.Username}*";

        public static string DetermineVideoSource(string url) => url switch
        {
            var u when Regex.IsMatch(u, @"youtube\.com\/shorts", RegexOptions.IgnoreCase) => "YouTube Shorts",
            var u when Regex.IsMatch(u, @"vk\.com\/clip", RegexOptions.IgnoreCase)      => "ВК клипы",
            var u when Regex.IsMatch(u, @"tiktok\.com", RegexOptions.IgnoreCase)         => "TikTok",
            var u when Regex.IsMatch(u, @"instagram\.com\/reel", RegexOptions.IgnoreCase)  => "Instagram Reels",
            _ => "Неизвестный источник"
        };

        public static string FormatVideoCaption(string username, string source, string url) =>
            $"{username} [Ссылка на {source}]({url})";

        public static Task SendErrorMessageAsync(TelegramBotClient botClient, long chatId, string errorMessage) =>
            botClient.SendTextMessageAsync(chatId, errorMessage, parseMode: ParseMode.Markdown);
    }

    public class YtDlpVideoDownloader : IVideoDownloader
    {
        private readonly string _ytdlpPath;
        public YtDlpVideoDownloader(string ytdlpPath) => _ytdlpPath = ytdlpPath;

        public async Task DownloadVideoAsync(string videoUrl, Stream outputStream)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ytdlpPath,
                Arguments = $"-o - {videoUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_ytdlpPath)
            };

            using var process = new Process { StartInfo = startInfo };
            try
            {
                process.Start();
                var copyTask = process.StandardOutput.BaseStream.CopyToAsync(outputStream);
                var errorTask = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(copyTask, process.WaitForExitAsync());
                var errorOutput = await errorTask;
                if (process.ExitCode != 0)
                    Console.WriteLine($"Ошибка при загрузке видео: {errorOutput}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
