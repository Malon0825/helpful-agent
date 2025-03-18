using nova_log.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Models
{
    public class ChatHistoryModel
    {
        public ChatRoleEnum Role { get; set; }

        public string? Content { get; set; }

        public string? ToolCallId { get; set; }
    }
}
