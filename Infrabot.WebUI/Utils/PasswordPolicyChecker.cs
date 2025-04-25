namespace Infrabot.WebUI.Utils
{
    public static class PasswordPolicyChecker
    {
        public static bool CheckPasswordForPolicy(
            string password,
            int minPasswordLength = 6,
            bool containSpecialCharacter = false,
            bool containNumber = false,
            bool containLowerCase = false,
            bool containUpperCase = false
        )
        {
            // Check minimum length
            if (password.Length < minPasswordLength)
            {
                return false;
            }

            // Check for special characters if required
            if (containSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return false;
            }

            // Check for numbers if required
            if (containNumber && !password.Any(char.IsDigit))
            {
                return false;
            }

            // Check for lowercase letters if required
            if (containLowerCase && !password.Any(char.IsLower))
            {
                return false;
            }

            // Check for uppercase letters if required
            if (containUpperCase && !password.Any(char.IsUpper))
            {
                return false;
            }

            // If all checks passed
            return true;
        }

    }
}
