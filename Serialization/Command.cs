using System;
using System.Collections.Generic;

namespace InfraBot.Core
{
    public class Command
    {
        public string command_starts_with { get; set; }
        public List<int> command_data_id { get; set; }
        public string command_execute_file { get; set; }
        public string command_help_manual { get; set; }
        public string command_help_short { get; set; }
        public string command_default_error { get; set; }
        public int command_execute_type { get; set; }
        public List<int> command_allowed_users_id { get; set; }
        public bool command_show_in_get_commands_list { get; set; }
        public List<ExecuteResult> command_execute_results { get; set; }
    }
}