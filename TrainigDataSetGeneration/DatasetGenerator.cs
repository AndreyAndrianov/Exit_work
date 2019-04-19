using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace TrainigDataSetGeneration
{
    public class DatasetGenerator
    {
        private const string DefaultPath = "F:\\Учеба\\Диплом\\data\\Generated_trainig_data";
        private const int ByPhoneMultiplier = 35;
        private const int ByCardNumberMultiplier = 55;
        private const int ByBankAccountMultiplier = 65;
        private const int ByEWalletMultiplier = 15;

        public void GenerateSimpleDataset()
        {
            Random rnd = new Random();
            Guid deviceId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var items = Enumerable.Range(0, 10000)
                .Select(i =>
                {
                    if (i % 3 == 0)
                    {
                        return new SimpleItem(deviceId.ToString(), userId.ToString(), rnd.Next(6000, 9000), 1);
                    }

                    return new SimpleItem(deviceId.ToString(), userId.ToString(), rnd.Next(5000, 6500), 0);
                });

            this.WriteCsv("Simplest_Dataset.csv", items);
        }

        public void GenerateSimplestDatasetManyUsers(int usersNumber)
        {
            Random random = new Random();
            Dictionary<(Guid userId, Guid deviceId), (AmountRange normal, AmountRange fraud)> dict =
                new Dictionary<(Guid userId, Guid deviceId), (AmountRange normal, AmountRange fraud)>();
            for (int i = 1; i <= usersNumber; i++)
            {
                var normalRange = new AmountRange(i * 150, i * 300);
                var fraudRange = new AmountRange(i * 250, i * 350);
                dict.Add((Guid.NewGuid(), Guid.NewGuid()), (normalRange, fraudRange));
            }

            (Guid userId, Guid deviceId)[] keys = dict.Keys.ToArray();

            var items = Enumerable.Range(0, usersNumber * 6000).Select(n =>
            {
                var targetKey = keys[n % usersNumber];
                if ((n / usersNumber) % 2 == 0)
                {
                    return new SimpleItem(
                        targetKey.userId.ToString(),
                        targetKey.deviceId.ToString(),
                        random.Next(dict[targetKey].fraud.Start, dict[targetKey].fraud.End),
                        1);
                }

                return new SimpleItem(
                    targetKey.userId.ToString(),
                    targetKey.deviceId.ToString(), 
                    random.Next(dict[targetKey].normal.Start, dict[targetKey].normal.End), 
                    0);
            });

            this.WriteCsv($"Simplest_Dataset_{usersNumber}_users.csv", items);
        }

        public void GenerateDatasetWithPhoneNumberOneUser(int usersCount, int phonesCount)
        {
            Random rnd = new Random();
            Guid deviceId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var phoneNumbers = Enumerable.Range(0, phonesCount)
                .Select(i => GeneratePhoneNumber(rnd)).ToList();
            phoneNumbers.Add(string.Empty);
            var items = Enumerable.Range(0, 10000)
                .Select(i =>
                {
                    if (i % 2 == 0)
                    {
                        // Generate fraud environments one part with phone number, other part without phone number
                        if (i % 3 == 0)
                        {
                            return new OperationItem(userId.ToString(), deviceId.ToString(), rnd.Next(6000, 9000), 1);
                        }

                        return new SimpleItem(userId.ToString(), deviceId.ToString(), rnd.Next(6000, 9000), 1);
                    }

                    // Generate non-fraud environments one part with phone number, other part without phone number
                    return new SimpleItem(userId.ToString(), deviceId.ToString(), rnd.Next(5000, 6500), 0);
                });

            this.WriteCsv("Simplest_Dataset_users.csv", items);
        }

        public void GenerateComplexDataset(
            int usersCount, 
            int operationCount, 
            int friendsNumber,
            bool withPhone = false, 
            bool withCard = false, 
            bool withEwallet = false, 
            bool withBankAccount = false)
        {
            Random rnd = new Random();
            IEnumerable<UserAccount> userAccounts = Enumerable.Range(2, usersCount)
                .Select(i =>
                {
                    var userId = Guid.NewGuid();
                    var deviceId = Guid.NewGuid();
                    var baseStartRangeValueFraud = i * 20;
                    var baseEndRangeValueFraud = i * 32;
                    var baseStartRangeValueNormal = i * 14;
                    var baseEndRangeValueNormal = i * 24;

                    var moneyReceivers = new List<UserAccount.OperationReceiverInfo>();
                    for (int f = 0; f < friendsNumber; f++)
                    {
                        double friendNumberMultiplier = 1 + f * 0.1;

                        if (withPhone)
                        {
                            var fraudRange = new AmountRange((int)(baseStartRangeValueFraud * ByPhoneMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueFraud * ByPhoneMultiplier * friendNumberMultiplier));
                            var normalRange = new AmountRange((int)(baseStartRangeValueNormal * ByPhoneMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueNormal * ByPhoneMultiplier * friendNumberMultiplier));
                            moneyReceivers.Add(new UserAccount.OperationReceiverInfo(GeneratePhoneNumber(rnd),
                                OperationType.ByPhone, fraudRange, normalRange));
                        }

                        if (withCard)
                        {
                            var fraudRange = new AmountRange((int)(baseStartRangeValueFraud * ByCardNumberMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueFraud * ByCardNumberMultiplier * friendNumberMultiplier));
                            var normalRange = new AmountRange((int)(baseStartRangeValueNormal * ByCardNumberMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueNormal * ByCardNumberMultiplier * friendNumberMultiplier));
                            moneyReceivers.Add(new UserAccount.OperationReceiverInfo(this.GenerateCardNumber(rnd),
                                OperationType.ByCreditCard,
                                fraudRange, normalRange));
                        }

                        if (withBankAccount)
                        {
                            var fraudRange = new AmountRange((int)(baseStartRangeValueFraud * ByBankAccountMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueFraud * ByBankAccountMultiplier * friendNumberMultiplier));
                            var normalRange = new AmountRange((int)(baseStartRangeValueNormal * ByBankAccountMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueNormal * ByBankAccountMultiplier * friendNumberMultiplier));
                            moneyReceivers.Add(new UserAccount.OperationReceiverInfo(this.GenerateNumber(20, rnd),
                                OperationType.ToOuterAccount,
                                fraudRange, normalRange));
                        }

                        if (withEwallet)
                        {
                            var fraudRange = new AmountRange((int)(baseStartRangeValueFraud * ByEWalletMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueFraud * ByEWalletMultiplier * friendNumberMultiplier));
                            var normalRange = new AmountRange((int)(baseStartRangeValueNormal * ByEWalletMultiplier * friendNumberMultiplier),
                                (int)(baseEndRangeValueNormal * ByEWalletMultiplier * friendNumberMultiplier));
                            moneyReceivers.Add(new UserAccount.OperationReceiverInfo(this.GenerateNumber(22, rnd),
                                OperationType.ToEWallet,
                                fraudRange, normalRange));
                        }
                    }

                    return new UserAccount(userId, deviceId, moneyReceivers);
                })
                .ToList();

            IEnumerable<OperationItem> operationItemsItems = userAccounts.SelectMany(a => a.GetAllOperationItems(operationCount));
            this.WriteCsv("Complex_Dataset_users.csv", operationItemsItems);
        }

        private void WriteCsv<T>(string fileName, IEnumerable<T> items, string delimeter = ",")
        {
            var path = Path.Combine(DatasetGenerator.DefaultPath, fileName);
            using (var writer = new StreamWriter(path))
            {
                using (var csvWriter = new CsvWriter(writer))
                {
                    csvWriter.Configuration.CultureInfo = CultureInfo.InvariantCulture;
                    csvWriter.Configuration.Delimiter = delimeter;
                    csvWriter.WriteRecords<T>(items);
                }
            }
        }

        private string GeneratePhoneNumber(Random rnd)
        {
            var parts = new[]
            {
                "+7",
                rnd.Next(901, 999).ToString(),
                rnd.Next(0000000, 9999999).ToString(),
            };
            return string.Join(string.Empty, parts);
        }

        private string GenerateNumber(int length, Random rnd)
        {
            StringBuilder sb = new StringBuilder();
            while (sb.Length < length)
            {
                sb.Append(rnd.Next(0, 9));
            }

            return sb.ToString();
        }

        private string GenerateCardNumber(Random rnd)
        {
            return string.Join("", Enumerable.Range(0, 4).Select(i => rnd.Next(1000, 9999).ToString()));
        }
    }
}
