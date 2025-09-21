using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class AlertEmail : TableEntity
    {
        public AlertEmail()
        {
            PartitionKey = "ALERT_EMAIL";
        }

        public AlertEmail(string email) : this()
        {
            RowKey = email;
            Email = email;
        }

        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
