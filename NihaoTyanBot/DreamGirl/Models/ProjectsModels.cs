// Models/UserSettings.cs
using System.ComponentModel.DataAnnotations;

namespace NihaoTyan.Bot.commandsList.userCommands.Models
{
    /// <summary>
    /// Таблица StepToFreedom
    /// </summary>
    public class STF
    {
        [Key]
        public long UserId { get; set; }
        public int FirstTimer { get; set; } 
        public int SecondTimer { get; set; } 
        public int orange { get; set; } 
    }

}
