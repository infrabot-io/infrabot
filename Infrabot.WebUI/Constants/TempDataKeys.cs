namespace Infrabot.WebUI.Constants
{
    public static class TempDataKeys
    {
        // AccountController
        public const string LoginOrPasswordIncorrect = "LoginOrPasswordIncorrect";
        public const string LoginDenied = "LoginDenied";
        public const string LoginDataIsNotValid = "LoginDataIsNotValid";
        public const string ADAuthFailed = "ADAuthFailed";
        public const string LoginDeniedForApiUser = "LoginDeniedForApiUser";
        public const string OldPasswordIsIncorrect = "OldPasswordIsIncorrect";
        public const string NewPasswordNotEqualToRepeat = "NewPasswordNotEqualToRepeat";
        public const string DoesNotMeetComplexityRequirements = "DoesNotMeetComplexityRequirements";
        public const string SucessfullyChanged = "SucessfullyChanged";

        // ConfigurationController
        public const string ConfigurationSaved = "ConfigurationSaved";

        // GroupsController
        public const string GroupAlreadyExists = "GroupAlreadyExists";

        // TelegramUsersController
        public const string TelegramUserAlreadyExists = "TelegramUserAlreadyExists";

        // UsersController
        public const string ConfigurationError = "ConfigurationError";
        public const string UserAlreadyExists = "UserAlreadyExists";
        public const string PasswordDoesNotMeetComplexity = "PasswordDoesNotMeetComplexity";
    }
}
