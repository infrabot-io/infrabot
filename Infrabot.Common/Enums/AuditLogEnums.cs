namespace Infrabot.Common.Enums
{
    public enum AuditLogAction
    {
        None = 0,
        Create = 1,
        Delete = 2,
        Update = 3,
        Migrate = 4,
        Clean = 5
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
