using System;

namespace InfraBot.Enums
{
    public enum CommandExecuteTypes : int
    {
        AppWithArgs = 1,
        AppWithoutArgs = 2,
        PSScriptWithArgs = 3,
        PSScriptWithoutArgs = 4
    }
}