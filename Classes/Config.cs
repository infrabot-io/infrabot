using System;
using System.Collections.Generic;

namespace InfraBot.Core
{
    public class Config
    {
        public string telegram_bot_token { get; set; }
        public bool telegram_enable_logging { get; set; }
        public bool telegram_enable_reminder { get; set; }
        public bool telegram_enable_reloadconfig { get; set; }
        public bool telegram_enable_emergency { get; set; }
        public string telegram_powershell_path { get; set; }
        public string telegram_powershell_executionpolicy { get; set; }
        public string telegram_logs_path { get; set; }
        public int telegram_result_max_length { get; set; }
        public List<long> telegram_allowed_chats_id { get; set; }
        public List<int> telegram_allowed_users_id { get; set; }
        public List<int> telegram_allowed_users_id_emergency { get; set; }
        public List<long> telegram_allowed_chats_id_emergency { get; set; }
        public List<int> telegram_allowed_users_id_reloadconfig { get; set; }
        public List<long> telegram_allowed_chats_id_reloadconfig { get; set; }
        public List<int> telegram_allowed_users_id_getcommands { get; set; }
        public List<long> telegram_allowed_chats_id_getcommands { get; set; }
        public List<int> telegram_allowed_users_id_remindme { get; set; }
        public List<long> telegram_allowed_chats_id_remindme { get; set; }
        public List<Command> telegram_commands { get; set; }
    }
}