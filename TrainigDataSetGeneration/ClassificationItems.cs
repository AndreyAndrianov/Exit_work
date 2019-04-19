using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainigDataSetGeneration
{
    public class SimpleItem
    {
        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public int Amount { get; set; }

        public int IsFraud { get; set; }

        public SimpleItem()
        {
        }

        public SimpleItem(string userId, string deviceId, int amount, int isFraud)
        {
            this.UserId = userId;
            this.DeviceId = deviceId;
            this.Amount = amount;
            this.IsFraud = isFraud;
        }
    }

    public class OperationItem : SimpleItem
    {
        public string PhoneNumber { get; set; }

        public string CardNumber { get; set; }

        public string Ewallet { get; set; }

        public string BankAccount { get; set; }

        public OperationItem(string userId, string deviceId, int amount, int isFraud) 
            : base (userId, deviceId, amount, isFraud)
        {
        }
    }
}
