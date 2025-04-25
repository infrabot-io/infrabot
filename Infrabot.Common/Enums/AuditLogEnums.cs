namespace Infrabot.Common.Enums
{
    public enum AuditLogAction
    {
        None,
        Create,
        Delete,
        Update,
        Migrate,
        Clean,
        LogOut,
        LogIn,
        ChangePassword,
        Access
    }

    public enum AuditLogItem
    {
        Plugin,
        User,
        TelegramUser,
        PermissionAssignment,
        Configuration,
        Group
    }

    public enum AuditLogResult
    {
        Success,
        Failure,
        Error,
        Denied,
        NotFound,
        None
    }

    public enum AuditLogSeverity
    {
        Lowest,
        Low,
        Medium,
        Higer,
        Highest
    }
}
