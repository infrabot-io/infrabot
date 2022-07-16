using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrabot.ConfigEditor.Serialization
{
    public class Config
    {
        public string telegram_bot_token { get; set; }
        public bool telegram_enable_logging { get; set; }
        public bool telegram_enable_reloadconfig { get; set; }
        public bool telegram_enable_emergency { get; set; }
        public bool telegram_enable_showmyid { get; set; }
        public string telegram_powershell_default_path { get; set; }
        public string telegram_powershell_arguments { get; set; }
        public int telegram_result_max_length { get; set; }
        public List<long> telegram_allowed_users_id { get; set; }
        public List<long> telegram_allowed_users_id_emergency { get; set; }
        public List<long> telegram_allowed_users_id_reloadconfig { get; set; }
        public List<long> telegram_allowed_users_id_getcommands { get; set; }
    }
}
