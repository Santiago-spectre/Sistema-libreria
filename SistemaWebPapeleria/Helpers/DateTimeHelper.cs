namespace SistemaWebPapeleria.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo ZonaPeru = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

        public static DateTime AhoraEnPeru()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaPeru);
        }
    }
}