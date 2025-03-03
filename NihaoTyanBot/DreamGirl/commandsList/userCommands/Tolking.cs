using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace NihaoTyan.Bot.commandsList.userCommands
{
    public static class Tolking
    {
        /// <summary>
        /// Обрабатывает команду "/start", отправляя приветственное сообщение пользователю.
        /// </summary>
        public static async Task Start(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            var welcomeMessage = "Здравствуйте, товарищ ✌️! Меня зовут Нихао, я ассистент мистера Романова!\n"
                               + "Если вам что-нибудь понадобится, просто используйте команду /help, и я сразу же приду к вам на помощь!";
            
            await botClient.SendTextMessageAsync(chatId, welcomeMessage);
        }

        /// <summary>
        /// Обрабатывает команду "/help", отправляя пользователю изображение и список доступных команд.
        /// </summary>
        public static async Task Help(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            
            // Формируем путь к файлу изображения
            string imagePath = Path.Combine(
                "DreamGirl", "commandsList", "userCommands", "mediaFiles", "nihaoJPEG.jpg"
                );
            
            await using var stream = System.IO.File.OpenRead(imagePath);
            var inputFile = new InputOnlineFile(stream);
            
            var helpMessage = "Ну что, товарищ, вам нужна моя помощь?\n"
                            + "Не волнуйтесь, сейчас я вам всё объясню и расскажу 😉\n\n"
                            + "Доступные команды:\n\n"
                            + "/anecdote — расскажу шутку про Штирлица;\n\n"
                            + "/dedinside — функция агрессивного зова товарища (бомбер). Просто отправьте мне его никнейм;\n\n"
                            + "/stf — помогает распределять ваше время, полезно для тех, у кого нет силы воли.\n\n"
                            + "Я также могу скачать для вас TikTok, YouTube Shorts, Instagram Reels или ВК Клип. Просто отправьте мне ссылку.\n\n"
                            + "Про /help и /start я не рассказываю, так как вы уже их используете! 😅";
            
            await botClient.SendPhotoAsync(chatId, inputFile, caption: helpMessage);
        }
    }
}
