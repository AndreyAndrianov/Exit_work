using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainigDataSetGeneration
{
    public class UserAccount
    {
        public Guid UserId { get; set; }

        public Guid DeviceId { get; set; }

        public IEnumerable<OperationReceiverInfo> OperationReceiverInfos { get; set; }

        public List<Operation> Operations { get; private set; }

        public UserAccount(Guid userId, Guid deviceId, IEnumerable<OperationReceiverInfo> receiverInfos)
        {
            this.UserId = userId;
            this.DeviceId = deviceId;
            this.OperationReceiverInfos = receiverInfos;
        }

        public IEnumerable<OperationItem> GetAllOperationItems(int eachTypeItemsCount)
        {
            this.GenerateOperationItems(eachTypeItemsCount);
            return Operations?.Select(o =>
                    new OperationItem(
                        this.UserId.ToString("D"), 
                        this.DeviceId.ToString("D"),
                        o.Amount,
                        Convert.ToInt32(o.IsFraud))
                    {
                        PhoneNumber = o.OperationType == OperationType.ByPhone ? o.ReceiverId : string.Empty,
                        CardNumber = o.OperationType == OperationType.ByCreditCard ? o.ReceiverId : string.Empty,
                        BankAccount = o.OperationType == OperationType.ToOuterAccount ? o.ReceiverId : string.Empty,
                        Ewallet = o.OperationType == OperationType.ToEWallet ? o.ReceiverId : string.Empty,
                    })
                .ToList();
        }

        public class OperationReceiverInfo
        {
            public string ReceiverId { get; }
            public AmountRange FraudRange { get; }
            public AmountRange NormalRange { get; }
            public OperationType OperationType { get; }

            public OperationReceiverInfo(string receiverId, OperationType operationType, AmountRange fraudRange, AmountRange normalRange)
            {
                this.ReceiverId = receiverId;
                this.FraudRange = fraudRange;
                this.NormalRange = normalRange;
                this.OperationType = operationType;
            }
        }

        private void GenerateOperationItems(int operationsCount)
        {
            // TODO initialize all operations
            Random rnd = new Random();
            this.Operations = new List<Operation>();
            foreach (var info in this.OperationReceiverInfos)
            {
                Operations.AddRange(Enumerable.Range(0, operationsCount)
                    .Select(i =>
                    {
                        if (i % 2 == 0)
                        {
                            // TODO fraud
                            return new Operation(rnd.Next(info.FraudRange.Start, info.FraudRange.End), true, info.ReceiverId, info.OperationType);
                        }

                        // TODO non-fraud
                        return new Operation(rnd.Next(info.NormalRange.Start, info.NormalRange.End), false, info.ReceiverId, info.OperationType);
                    }));
            }
        }

        public class Operation
        {
            public int Amount { get; }

            public bool IsFraud { get; }

            public string ReceiverId { get; }

            public OperationType OperationType { get; }

            public Operation(int amount, bool isFraud, string receiverId, OperationType type)
            {
                this.OperationType = type;
                this.ReceiverId = receiverId;
                this.Amount = amount;
                this.IsFraud = isFraud;
            }
        }
    }
}
