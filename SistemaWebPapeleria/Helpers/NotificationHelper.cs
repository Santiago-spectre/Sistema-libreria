using SistemaWebPapeleria.Data;
using SistemaWebPapeleria.Models;

namespace SistemaWebPapeleria.Helpers
{
    public static class NotificationHelper
    {
        public static async Task CrearAsync(AppDbContext context, int userId, string title, string message, string type)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTimeHelper.AhoraEnPeru(),
                UserId = userId
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }
    }
}
