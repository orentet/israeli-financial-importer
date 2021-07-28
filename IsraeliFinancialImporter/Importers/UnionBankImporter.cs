using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IsraeliFinancialImporter.Types;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace IsraeliFinancialImporter.Importers
{
    // todo: add support for multiple accounts
    // todo support installments / pending payments
    public class UnionBankImporter : IFinancialImporter
    {
        private readonly IWebDriver _driver;
        private readonly string _password;
        private readonly string _username;

        public UnionBankImporter(IWebDriver driver, string username, string password)
        {
            _driver = driver;
            _username = username;
            _password = password;
        }

        public IEnumerable<FinancialAccount> Import(DateTime fromInclusive, DateTime toInclusive)
        {
            Login();

            _driver.Url = "https://hb.unionbank.co.il/ebanking/Accounts/ExtendedActivity.aspx";
            var accountId = new SelectElement(_driver.FindElement(By.Id("ddlAccounts_m_ddl"))).SelectedOption.Text;
            yield return new FinancialAccount(accountId, ScrapeTransactions(fromInclusive, toInclusive));
        }

        private IEnumerable<FinancialTransaction> ScrapeTransactions(DateTime fromInclusive, DateTime toInclusive)
        {
            var dropDown = new SelectElement(_driver.FindElement(By.Id("ddlTransactionPeriod")));
            dropDown.SelectByValue("004");
            var dateFormat = "dd/MM/yy";
            _driver.FindElement(By.Id("dtFromDate_textBox")).SendKeys(fromInclusive.ToString(dateFormat));
            _driver.FindElement(By.Id("dtToDate_textBox")).SendKeys(toInclusive.ToString(dateFormat));
            _driver.FindElement(By.Id("btnDisplayDates")).Click();

            // wait for asp.net postback
            // todo find another method wait other than waiting...
            Thread.Sleep(TimeSpan.FromSeconds(5));

            foreach (var item in _driver.FindElements(By.ClassName("printItem")))
            {
                var columns = item.FindElements(By.TagName("td")).Select(x => x.Text.Trim()).ToArray();

                var id = columns[3];

                var occuredAt = DateTime.ParseExact(columns[1], dateFormat, null);

                var negativeStr = columns[4];
                var positiveStr = columns[5];
                var amount = !string.IsNullOrEmpty(negativeStr)
                    ? -decimal.Parse(negativeStr)
                    : decimal.Parse(positiveStr);

                var description = columns[2];

                yield return new FinancialTransaction(id, occuredAt, amount, Currency.NewIsraeliShekel, description,
                    true, null);
            }
        }

        private void Login()
        {
            _driver.Url = "https://hb.unionbank.co.il/H/Login.html";
            _driver.FindElement(By.Id("uid")).SendKeys(_username);
            _driver.FindElement(By.Id("password")).SendKeys(_password);
            _driver.FindElement(By.Id("enter")).Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(drv => drv.FindElement(By.ClassName("signoff_label")));
        }
    }
}