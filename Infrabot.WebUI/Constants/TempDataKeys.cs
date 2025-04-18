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

        // TelegramUsersController
        public const string TelegramUserAlreadyExists = "TelegramUserAlreadyExists";

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
