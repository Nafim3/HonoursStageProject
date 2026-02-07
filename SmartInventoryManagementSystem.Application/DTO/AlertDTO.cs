using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Application.DTO
{
    public class AlertDTO
    {
        public string Category { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
