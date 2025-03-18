using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Models
{
    public class RequestModel
    {
        public string Status { get; set; } = string.Empty;

        public string AgentResponse { get; set; } = string.Empty;

        public string UserPrompt { get; set; } = string.Empty;

        public string ChatHistory { get; set; } = string.Empty;
    }
}
