namespace Infrabot.WebUI.Constants
{
    public static class TempDataKeys
    {
        // AccountController

        // ConfigurationController
        public const string ConfigurationSaved = "ConfigurationSaved";

        // GroupsController
        public const string GroupAlreadyExists = "GroupAlreadyExists";

        // TelegramUsersController
        public const string TelegramUserAlreadyExists = "TelegramUserAlreadyExists";

        // UsersController
        public const string CreateUserAlreadyExists = "CreateUserAlreadyExists";
        public const string CreateUserSucceeded = "CreateUserSucceeded";
        public const string CreateUserFailed = "CreateUserFailed";
        public const string ConfigurationError = "ConfigurationError";
        public const string UserAlreadyExists = "UserAlreadyExists";
        public const string PasswordDoesNotMeetComplexity = "PasswordDoesNotMeetComplexity";
    }
}
