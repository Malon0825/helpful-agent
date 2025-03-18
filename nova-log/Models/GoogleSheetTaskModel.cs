using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.Models
{
    public class GoogleSheetTaskModel
    {

        public string TaskDescription { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public int EstimateHours { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public string PriorityLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Task: {TaskDescription}, Type: {TaskType}, Priority: {PriorityLevel}, Status: {Status}";
        }
    }
}
