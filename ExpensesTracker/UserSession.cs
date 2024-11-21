namespace ExpensesTracker
{
    public static class UserSession
    {
        public static int CurrentUserId { get; set; }
        public static string Email { get; set; }
        public static int Age { get; set; }
        public static string Gender { get; set; }
        public static decimal Income { get; set; }
        public static string CurrencySymbol { get; set; } = "Rp"; // Default to Rupiah
    }
}
