namespace Infrabot.Common.Enums
{
    public enum AuditLogAction
    {
        None = 0,
        Create = 1,
        Delete = 2,
        Update = 3,
        Migrate = 4,
        Clean = 5,
        LogOut = 6,
        LogIn = 7,
        ChangePassword = 8
    }

    public enum AuditLogItem
    {
        Plugin = 0,
        User = 1,
        TelegramUser = 2,
        PermissionAssignment = 3,
        Configuration = 4,
        Group = 5
    }
}
