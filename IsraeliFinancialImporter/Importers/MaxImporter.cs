using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using IsraeliFinancialImporter.Types;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Cookie = System.Net.Cookie;

namespace IsraeliFinancialImporter.Importers
{
    // todo: support installments
    public class MaxImporter : IFinancialImporter
    {
        private readonly IWebDriver _driver = new ChromeDriver();
        private readonly string _password;
        private readonly string _username;

        public MaxImporter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public IEnumerable<FinancialTransaction> Import(DateTime fromInclusive, DateTime toInclusive)
        {
            Login();

            using var client = GetHttpClientForDriver();
            var dateFormat = "yyyy-MM-dd";
            var strResponse = client.GetStringAsync(
                    $"/api/registered/transactionDetails/getTransactionsAndGraphs?filterData={{\"userIndex\":-1,\"cardIndex\":-1,\"monthView\":false,\"date\":\"{fromInclusive.ToString(dateFormat)}\",\"dates\":{{\"startDate\":\"{fromInclusive.ToString(dateFormat)}\",\"endDate\":\"{toInclusive.ToString(dateFormat)}\"}}}}&v=V3.13-HF.6.26")
                .Result;
            var jsonResponse = JObject.Parse(strResponse);
            foreach (var transObj in jsonResponse["result"]["transactions"])
            {
                var dealData = transObj["dealData"];
                var financialTransaction = new FinancialTransaction(dealData.Value<string>("refNbr"),
                    transObj.Value<string>("shortCardNumber"),
                    transObj.Value<DateTime>("purchaseDate"),
                    -transObj.Value<decimal>("actualPaymentAmount"),
                    Currency.NewIsraeliShekel,
                    transObj.Value<string>("merchantName"),
                    transObj.Value<string>("comments"),
                    transObj.Value<DateTime>("paymentDate") < DateTime.Now,
                    null);
                yield return financialTransaction;
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private HttpClient GetHttpClientForDriver()
        {
            var baseAddress = new Uri("https://www.max.co.il");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            var client = new HttpClient(handler) { BaseAddress = baseAddress };
            foreach (var cookie in _driver.Manage().Cookies.AllCookies.Where(x => x.Domain.Contains("max.co.il")))
            {
                var encodedCommaCookieValue = cookie.Value.Replace(",", "%2C");
                cookieContainer.Add(baseAddress, new Cookie(cookie.Name, encodedCommaCookieValue));
            }

            return client;
        }

        private void Login()
        {
            _driver.Url = "https://www.max.co.il/login";
            _driver.FindElement(By.Id("login-password-link")).Click();
            _driver.FindElement(By.Id("user-name")).SendKeys(_username);
            _driver.FindElement(By.Id("password")).SendKeys($"{_password}\n"); // add \n to submit form

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(drv => drv.Url == "https://www.max.co.il/homepage/personal");
        }
    }
}