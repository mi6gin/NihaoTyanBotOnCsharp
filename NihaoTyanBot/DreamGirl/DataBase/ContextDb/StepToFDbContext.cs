// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace NihaoTyan.Bot.commandsList.userCommands.Models
{
    public class StepToFDbContext : DbContext
    {
        public DbSet<STF> STFSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Формируем полный путь к файлу базы данных относительно базового пути приложения
            string dbPath = Path.Combine("DreamGirl", "DataBase", "SteptofreedomUsers.db");
            // Проверяем существование директории для базы данных
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory); // Создаем директорию, если она не существует
            }

            // Проверяем существование файла базы данных
            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"База данных не найдена по ожидаемому пути: {dbPath}");
            }

            // Настраиваем подключение к SQLite
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}