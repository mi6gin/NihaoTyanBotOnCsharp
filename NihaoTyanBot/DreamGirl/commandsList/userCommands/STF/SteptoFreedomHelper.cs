using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using NihaoTyan.Bot.commandsList.userCommands.Models;
using File = System.IO.File;

namespace  NihaoTyan.Bot.commandsList.userCommands
{
    public static class StepToFreedomHelper
    {
        // Поля для хранения состояния текущего цикла работы/отдыха
        private static long _userId;
        private static long _chatId;
        private static int _stepCounter;
        private static string _statusMessage;
        private static int _status;
        private static int _orange;

        /// <summary>
        /// Формирует inline-клавиатуру для управления режимом.
        /// </summary>
        private static InlineKeyboardMarkup BuildKeyboard(long userId)
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Давай", $"sSTF{userId}"),
                    InlineKeyboardButton.WithCallbackData("Не, с меня хватит", $"cSTF{userId}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изменить время", $"chSTF{userId}")
                }
            });
        }

        /// <summary>
        /// Управляет циклом таймера для работы и отдыха.
        /// </summary>
        public static async Task HandleStepToFreedomTimerAsync(CallbackQuery query, TelegramBotClient botClient)
        {
            _chatId = query.Message.Chat.Id;
            _userId = query.From.Id;

            int workTime = await GetUserTimerSettingAsync(_userId, "work");
            int relaxTime = await GetUserTimerSettingAsync(_userId, "relax");

            // Устанавливаем сообщение статуса в зависимости от текущего шага
            _statusMessage = _stepCounter switch
            {
                0 => "Впахивай как стахановец следующие",
                1 => "Отдыхай пролетарий следующие",
                _ => _statusMessage
            };

            // Обновляем счётчик шагов (0, 1, 2)
            _stepCounter = (_stepCounter + 1) % 3;

            if (_stepCounter == 1)
            {
                _status = 0;
                await RunTimerAsync(_chatId, workTime, botClient);
            }
            else if (_stepCounter == 2)
            {
                await RunTimerAsync(_chatId, relaxTime, botClient);
            }
            else // _stepCounter == 0 – завершён цикл
            {
                await RestartCycleAsync(botClient);
            }

            // Если цикл не завершён, продолжаем обработку
            if (_stepCounter != 0 && _status != 1)
            {
                await HandleStepToFreedomTimerAsync(query, botClient);
            }
        }

        /// <summary>
        /// Получает настройку таймера для пользователя из базы данных.
        /// </summary>
        private static async Task<int> GetUserTimerSettingAsync(long userId, string type)
        {
            using var dbContext = new StepToFDbContext();
            var userSetting = await dbContext.STFSettings.FirstOrDefaultAsync(u => u.UserId == userId);

            return type switch
            {
                "work" => userSetting?.FirstTimer ?? 1,
                "relax" => userSetting?.SecondTimer ?? 1,
                _ => 1
            };
        }

        /// <summary>
        /// Запускает обратный отсчет таймера, обновляя сообщение каждую секунду.
        /// </summary>
        private static async Task RunTimerAsync(long chatId, int durationMinutes, TelegramBotClient botClient)
        {
            int totalSeconds = durationMinutes * 60;
            Message timerMessage = await botClient.SendTextMessageAsync(chatId, $"{_statusMessage} {totalSeconds / 60:00}:{totalSeconds % 60:00}");

            // Обратный отсчет с обновлением текста сообщения каждую секунду
            while (totalSeconds > 0)
            {
                totalSeconds--;
                await botClient.EditMessageTextAsync(chatId, timerMessage.MessageId, $"{_statusMessage} {totalSeconds / 60:00}:{totalSeconds % 60:00}");
                await Task.Delay(1000);
            }

            await botClient.DeleteMessageAsync(chatId, timerMessage.MessageId);
        }

        /// <summary>
        /// Сбрасывает счетчик и отправляет сообщение с поздравлением и результатом.
        /// </summary>
        private static async Task RestartCycleAsync(TelegramBotClient botClient)
        {
            using var dbContext = new StepToFDbContext();
            var userSetting = await dbContext.STFSettings.FindAsync(_userId);
            if (userSetting != null)
            {
                userSetting.orange++;
                _orange = userSetting.orange;
                await dbContext.SaveChangesAsync();  
            }

            _status = 1;
            _stepCounter = 0;
            var keyboard = BuildKeyboard(_userId);

            // Отправляем изображение с поздравлением и информацией о накопленных "🍊"
            string imagePath = Path.Combine(
                Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, 
                "DreamGirl", "commandsList", "userCommands", "mediaFiles", "STFnihao.png");
            await using var stream = File.OpenRead(imagePath);
            await botClient.SendPhotoAsync(
                _chatId, 
                new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream), 
                $"Поздравляю, вы получили *[{_orange}🍊]*\nЕщё разок?", 
                replyMarkup: keyboard, 
                parseMode: ParseMode.Markdown);
        }
    }
}
