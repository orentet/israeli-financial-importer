using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using IsraeliFinancialImporter.Types;
using Newtonsoft.Json.Linq;

namespace IsraeliFinancialImporter.Importers
{
    // todo: support installments
    // todo: support non-shekel
    public class CalImporter : IFinancialImporter
    {
        private static readonly string dateFormat = "dd/MM/yyyy";
        private readonly string _password;
        private readonly string _username;

        public CalImporter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public IEnumerable<FinancialTransaction> Import(DateTime fromInclusive, DateTime toInclusive)
        {
            var results = Enumerable.Empty<FinancialTransaction>();

            var loginToken = Login();

            // wait for cals systems to refresh itself...
            Thread.Sleep(TimeSpan.FromSeconds(10));

            var client = CreateClient();
            client.BaseAddress = new Uri("https://cal4u.cal-online.co.il/Cal4U/");
            client.DefaultRequestHeaders.Add("Authorization", $"CALAuthScheme {loginToken}");
            var cardsResponse = JObject.Parse(client.GetStringAsync("CardsByAccounts").Result);
            foreach (var account in cardsResponse["BankAccounts"])
            {
                var accountId = account.Value<string>("AccountID");
                var cardDebitDates = GetCardDebitDates(client, accountId, fromInclusive, toInclusive);
                foreach (var cardDebitDate in cardDebitDates)
                {
                    var transactionsResponse = JObject.Parse(client.GetStringAsync("CalTransactions/" +
                            cardDebitDate.Item1 +
                            "?FromDate=" +
                            cardDebitDate.Item2.ToString(dateFormat) +
                            "&ToDate=" +
                            cardDebitDate.Item2.ToString(dateFormat))
                        .Result);
                    results = results.Concat(HandleTransactions(transactionsResponse, cardDebitDate.Item1, dateFormat));
                    while (transactionsResponse.Value<bool>("HasNextPage"))
                    {
                        transactionsResponse = JObject.Parse(client.GetStringAsync("CalTransNextPage").Result);
                        results = results.Concat(HandleTransactions(transactionsResponse, cardDebitDate.Item1,
                            dateFormat));
                    }
                }
            }

            // since we are getting transactions based on debit dates, it might not be in the specified range.
            return results.Where(x => x.OccuredAt >= fromInclusive && x.OccuredAt <= toInclusive);
        }

        private IEnumerable<(string, DateTime)> GetCardDebitDates(HttpClient client, string accountId,
            DateTime fromInclusive, DateTime toInclusive)
        {
            var debitsResponse = JObject.Parse(client.GetStringAsync("CalBankDebits/" + accountId +
                                                                     "?DebitLevel=A" +
                                                                     "&DebitType=2" +
                                                                     "&FromMonth=" + fromInclusive.Month +
                                                                     "&FromYear=" + fromInclusive.Year +
                                                                     "&ToMonth=" + toInclusive.Month +
                                                                     "&ToYear=" + toInclusive.Year).Result);
            return debitsResponse["Debits"].Select(x =>
                (x.Value<string>("CardId"), DateTime.ParseExact(x.Value<string>("Date"), dateFormat, null)));
        }

        private static IEnumerable<FinancialTransaction> HandleTransactions(JObject transactionsResponse,
            string cardNumber, string dateFormat)
        {
            foreach (var transaction in transactionsResponse["Transactions"])
                yield return new FinancialTransaction(transaction.Value<string>("Id"),
                    cardNumber,
                    DateTime.ParseExact(transaction.Value<string>("Date"), dateFormat, null),
                    -transaction["Amount"].Value<decimal>("Value"),
                    Currency.NewIsraeliShekel,
                    transaction["MerchantDetails"].Value<string>("Name"),
                    string.Join(",",
                        new[] {transaction.Value<string>("Comments"), transaction.Value<string>("Notes")}.Where(s =>
                            !string.IsNullOrEmpty(s))),
                    DateTime.ParseExact(transaction.Value<string>("DebitDate"), dateFormat, null) < DateTime.Now,
                    null);
        }

        private string Login()
        {
            var client = CreateClient();
            var request = new
            {
                username = _username,
                password = _password,
                rememberMe = (bool?) null
            };
            var response = client.PostAsJsonAsync("https://connect.cal-online.co.il/api/authentication/login", request)
                .Result;
            response.EnsureSuccessStatusCode();
            var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return jsonResponse.Value<string>("token");
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Site-Id", "8D37DF16-5812-4ACD-BAE7-CD1A5BFA2206");
            return client;
        }
    }
}