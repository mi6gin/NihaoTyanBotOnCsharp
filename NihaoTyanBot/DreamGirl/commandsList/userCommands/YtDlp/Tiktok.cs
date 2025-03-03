using NihaoTyan.Bot.commandsList.userCommands.YtDlp.Settings.Main;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace NihaoTyan.Bot.commandsList.userCommands.YtDlp
{
    public class Tikitoki
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _url;

        public static async void TTMessage(object sender, MessageEventArgs e)
        {
            if (sender is not TelegramBotClient botClient || e.Message is not { Type: MessageType.Text } message)
                return;

            if (!Regex.IsMatch(message.Text, "^(vm|https://(www|vm|vr|vt))\\.tiktok\\.com/"))
                return;

            try
            {
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                var sentMessage = await botClient.SendTextMessageAsync(message.Chat.Id, $"Скачиваю {message.Text}");
                var messToDel = sentMessage.MessageId;

                // Приведение ссылки к полному виду
                _url = message.Text.StartsWith("https") ? message.Text : $"https://{message.Text}";
                var response = await _httpClient.GetAsync(_url);
                var fullUrl = response.RequestMessage?.RequestUri?.ToString();
                
                if (fullUrl is null)
                {
                    await botClient.EditMessageTextAsync(message.Chat.Id, messToDel, "Ошибка при обработке ссылки");
                    return;
                }

                // Проверка на тип контента (слайд-шоу не поддерживаются)
                if (fullUrl.Contains("photo"))
                {
                    await botClient.EditMessageTextAsync(message.Chat.Id, messToDel, $"Скачивание слайд-шоу не поддерживается: {_url}");
                    return;
                }
                
                int messageToDelete = sentMessage.MessageId;
                // Инициализация загрузки видео
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadVideo);
                using var outputStream = new MemoryStream();
                await VideoDownloaderHelper.DownloadAndSendVideoAsync(message.Text, botClient, message, messageToDelete, outputStream);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Ошибка: {ex.Message}");
            }
        }
    }
}
