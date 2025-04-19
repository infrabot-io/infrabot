namespace Infrabot.WebUI.Constants
{
    public static class TempDataKeys
    {
        // AccountController
        public const string AccountLoginOrPasswordIncorrect = "AccountLoginOrPasswordIncorrect";
        public const string AccountLoginDenied = "AccountLoginDenied";
        public const string AccountADAuthFailed = "AccountADAuthFailed";
        public const string AccountNewPasswordNotEqualToRepeat = "AccountNewPasswordNotEqualToRepeat";
        public const string AccountUserNotFound = "AccountUserNotFound";
        public const string AccountPasswordChangeFailed = "AccountPasswordChangeFailed";
        public const string AccountPasswordChangeSucceeded = "AccountPasswordChangeSucceeded";

        // ConfigurationController
        public const string ConfigurationSaved = "ConfigurationSaved";

        // GroupsController
        public const string GroupAlreadyExists = "GroupAlreadyExists";
        public const string GroupNotFound = "GroupNotFound";
        public const string GroupSaved = "GroupSaved";
        public const string GroupDeleted = "GroupDeleted";

        // PermissionAssignmentController
        public const string PermissionAssignmentNotFound = "PermissionAssignmentNotFound"; 
        public const string PermissionAssignmentSaved = "PermissionAssignmentSaved";
        public const string PermissionAssignmentDeleted = "PermissionAssignmentDeleted";
        public const string PermissionAssignmentOnePluginMustBeSelected = "PermissionAssignmentOnePluginMustBeSelected";
        public const string PermissionAssignmentOneTelegramUserMustBeSelected = "PermissionAssignmentOneTelegramUserMustBeSelected";

        // TelegramUsersController
        public const string TelegramUserAlreadyExists = "TelegramUserAlreadyExists";
        public const string TelegramUserNotFound = "TelegramUserNotFound";
        public const string TelegramUserSaved = "TelegramUserSaved";
        public const string TelegramUserDeleted = "TelegramUserDeleted";

        // PluginsController
        public const string PluginNotFound = "PluginNotFound";
        public const string PluginDeleted = "PluginDeleted";
        public const string PluginDeleteFailed = "PluginDeleteFailed";
        public const string PluginUploaded = "PluginUploaded";

        // UsersController
        public const string CreateUserAlreadyExists = "CreateUserAlreadyExists";
        public const string CreateUserSucceeded = "CreateUserSucceeded";
        public const string CreateUserFailed = "CreateUserFailed";
        public const string EditUserSuccess = "EditUserSuccess";
        public const string EditUserSuccessWithPassword = "EditUserSuccessWithPassword";
        public const string EditUserFailed = "EditUserFailed";
        public const string EditUserFailedWithPassword = "EditUserFailedWithPassword";
        public const string PasswordDoesNotMeetComplexity = "PasswordDoesNotMeetComplexity";
    }
}
