using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using NihaoTyan.Bot.commandsList.userCommands.Models;
using File = System.IO.File;

namespace  NihaoTyan.Bot.commandsList.userCommands
{
    public class StepToFreedom
    {
        // Хранит идентификаторы пользователя и чата для текущей сессии
        private static long _userId;
        private static long _chatId;
        private static int _messageToDelete;
        // Флаг для продолжения цикла режима
        private static bool _continueCycle = false;

        /// <summary>
        /// Запускает режим "Step to Freedom" – отправляет приветственное изображение с клавиатурой.
        /// </summary>
        public static async Task StartModeAsync(Message message, TelegramBotClient botClient)
        {
            _chatId = message.Chat.Id;
            _userId = message.From.Id;

            await InitializeDatabaseAsync();

            // Формируем клавиатуру для управления режимом
            var keyboard = BuildKeyboard(_userId);

            // Загружаем изображение и отправляем пользователю
            string imagePath = Path.Combine(
                "DreamGirl", "commandsList", "userCommands", "mediaFiles", "nihaoSTF.jpeg"
                );
            await using var stream = File.OpenRead(imagePath);
            var sentMessage = await botClient.SendPhotoAsync(
                _chatId,
                new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream),
                "Привет товарищ mi6gun, ты запустил особый режим\r\nРежим ПРОГУЛКИ подразумевает концентрацию и усидчивость\r\nКак будешь готов, дай мне знать",
                replyMarkup: keyboard);
            
            _messageToDelete = sentMessage.MessageId;
        }

        /// <summary>
        /// Инициализирует или обновляет запись настроек пользователя в базе данных.
        /// </summary>
        private static async Task InitializeDatabaseAsync()
        {
            using var dbContext = new StepToFDbContext();
            await dbContext.Database.EnsureCreatedAsync();

            var userSettings = await dbContext.STFSettings.FindAsync(_userId);

            if (userSettings == null)
            {
                // Запись отсутствует — добавляем новую
                userSettings = new STF { UserId = _userId };
                dbContext.STFSettings.Add(userSettings);
            }
            else
            {
                // Запись найдена — можно обновлять, если нужно
                dbContext.STFSettings.Update(userSettings);
            }
    
            await dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Завершает текущий режим и предлагает пользователю перезапустить его.
        /// </summary>
        public static async Task EndModeAsync(Message message, TelegramBotClient botClient)
        {
            _chatId = message.Chat.Id;
            _userId = message.From.Id;

            var keyboard = BuildKeyboard(_userId);
            string photoUrl = Path.Combine(
                "DreamGirl", "commandsList", "userCommands", "mediaFiles", "nihaoSTF.jpeg"
                );
            // Отправляем изображение с предложением повторить режим
            await botClient.SendPhotoAsync(
                _chatId, 
                photoUrl,
                "Вы МОШЫНА товарищ\nМожем повторить?",
                replyMarkup: keyboard);

            // Ожидаем подтверждения продолжения режима
            while (!_continueCycle)
            {
                await Task.Delay(1000);
            }

            _continueCycle = false;
            await StartModeAsync(message, botClient);
        }

        /// <summary>
        /// Обновляет настройки таймера пользователя (рабочее и отдых).
        /// </summary>
        public static async Task UpdateUserTimerSettingAsync(long userId, int workTimer, int relaxTimer)
        {
            using var dbContext = new StepToFDbContext();
            var userSettings = await dbContext.STFSettings.FindAsync(userId);

            if (userSettings == null)
            {
                // Если запись отсутствует, создаём новую и добавляем в контекст
                userSettings = new STF 
                { 
                    UserId = userId, 
                    FirstTimer = workTimer, 
                    SecondTimer = relaxTimer 
                };
                dbContext.STFSettings.Add(userSettings);
            }
            else
            {
                // Если запись найдена, обновляем поля
                userSettings.FirstTimer = workTimer;
                userSettings.SecondTimer = relaxTimer;
                dbContext.STFSettings.Update(userSettings);
            }
            await dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Увеличивает счётчик "orange" для пользователя.
        /// </summary>
        public static async Task IncrementUserOrangeAsync(long userId)
        {
            using var dbContext = new StepToFDbContext();
            var userSettings = await dbContext.STFSettings.FindAsync(userId)
                               ?? new STF { UserId = userId };
            
            userSettings.orange++;
            dbContext.STFSettings.Update(userSettings);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Формирует inline-клавиатуру с возможными действиями пользователя.
        /// </summary>
        private static InlineKeyboardMarkup BuildKeyboard(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Запуск", $"sSTF{userId}"),
                    InlineKeyboardButton.WithCallbackData("Отмена", $"cSTF{userId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изменить время", $"chSTF{userId}")
                }
            });
        }
    }
}
