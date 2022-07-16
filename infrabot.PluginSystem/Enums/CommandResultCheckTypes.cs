using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrabot.PluginSystem.Enums
{
    public enum CommandResultCheckTypes : int
    {
        EqualsTo = 0,
        NotEquals = 1,
        GreaterThan = 2,
        LessThan = 3,
        Contains = 4,
        StartsWith = 5,
        EndsWith = 6,
        NotContains = 7,
        NotStartsWith = 8,
        NotEndsWith = 9,
        IgnoreCaseEqualsTo = 10,
        IgnoreCaseContains = 11,
        IgnoreCaseStartsWith = 12,
        IgnoreCaseEndsWith = 13,
        IgnoreCaseNotContains = 14,
        IgnoreCaseNotStartsWith = 15,
        IgnoreCaseNotEndsWith = 16
    }
}
