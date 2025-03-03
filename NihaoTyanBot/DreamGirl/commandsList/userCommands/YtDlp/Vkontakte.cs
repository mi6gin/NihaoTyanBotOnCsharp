using NihaoTyan.Bot.commandsList.userCommands.YtDlp.Settings.Main;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace NihaoTyan.Bot.commandsList.userCommands.YtDlp
{
    public static class Vkudahti
    {
        private static readonly Regex VkVideoRegex = new(@"https://vk\.com/clip", RegexOptions.Compiled);

        public static async void  VKMessage(object sender, MessageEventArgs e)
        {
            if (sender is not TelegramBotClient botClient || e.Message is not { Type: MessageType.Text } message)
                return;

            // Проверяем, содержит ли сообщение ссылку на видео ВКонтакте
            if (!VkVideoRegex.IsMatch(message.Text))
                return;

            try
            {
                // Удаляем исходное сообщение с ссылкой
                await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                // Уведомляем пользователя о начале загрузки
                var sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Скачиваю видео с ВК: {message.Text}"
                );

                // Отображаем действие загрузки видео
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadVideo);

                // Идентификатор отправленного сообщения, чтобы затем удалить его при необходимости
                int messageToDelete = sentMessage.MessageId;

                // Создаём поток для хранения загружаемого видео
                using var outputStream = new MemoryStream();
                await VideoDownloaderHelper.DownloadAndSendVideoAsync(message.Text, botClient, message, messageToDelete, outputStream);
            }
            catch (Exception ex)
            {
                // Логирование ошибки (предполагается, что у вас есть система логирования)
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            }
        }
    }
}
