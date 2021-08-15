using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IsraeliFinancialImporter.Types;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace IsraeliFinancialImporter.Importers
{
    // todo: add support for multiple accounts
    // todo support installments / pending payments
    // todo: support not cleared transactions
    public class UnionBankImporter : IFinancialImporter
    {
        private readonly IWebDriver _driver = new ChromeDriver();
        private readonly string _password;
        private readonly string _username;

        public UnionBankImporter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public IEnumerable<FinancialTransaction> Import(DateTime fromInclusive, DateTime toInclusive)
        {
            Login();

            _driver.Url = "https://hb.unionbank.co.il/ebanking/Accounts/ExtendedActivity.aspx";
            var accountId = new SelectElement(_driver.FindElement(By.Id("ddlAccounts_m_ddl"))).SelectedOption.Text;
            return ScrapeTransactions(accountId, fromInclusive, toInclusive);
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private IEnumerable<FinancialTransaction> ScrapeTransactions(string accountId, DateTime fromInclusive,
            DateTime toInclusive)
        {
            var dropDown = new SelectElement(_driver.FindElement(By.Id("ddlTransactionPeriod")));
            dropDown.SelectByValue("004");
            var dateFormat = "dd/MM/yy";
            _driver.FindElement(By.Id("dtFromDate_textBox")).SendKeys(fromInclusive.ToString(dateFormat));
            _driver.FindElement(By.Id("dtToDate_textBox")).SendKeys(toInclusive.ToString(dateFormat));
            _driver.FindElement(By.Id("btnDisplayDates")).Click();

            // wait for asp.net postback
            // todo find another method wait other than waiting...
            Thread.Sleep(TimeSpan.FromSeconds(20));

            // expand all memos
            _driver.FindElement(By.Id("lnkCtlExpandAll")).Click();

            var trs = _driver.FindElement(By.Id("ctlActivityTable")).FindElements(By.TagName("tr"));
            for (var index = 0; index < trs.Count; index++)
            {
                var tr = trs[index];
                if (!tr.GetAttribute("class").Contains("printItem")) continue;
                var columns = tr.FindElements(By.TagName("td")).Select(x => x.Text.Trim()).ToArray();

                var id = columns[3];

                var occuredAt = DateTime.ParseExact(columns[1], dateFormat, null);

                var negativeStr = columns[4];
                var positiveStr = columns[5];
                var amount = !string.IsNullOrEmpty(negativeStr)
                    ? -decimal.Parse(negativeStr)
                    : decimal.Parse(positiveStr);

                var payee = columns[2];

                string memo = null;
                if (tr.FindElements(By.ClassName("additionalMinus")).Count > 0)
                    // has memo
                    memo = trs[index + 1].Text.Trim();

                yield return new FinancialTransaction(id, accountId, occuredAt, amount, Currency.NewIsraeliShekel,
                    payee, memo, true, null);
            }
        }

        private void Login()
        {
            _driver.Url = "https://hb.unionbank.co.il/H/Login.html";
            _driver.FindElement(By.Id("uid")).SendKeys(_username);
            _driver.FindElement(By.Id("password")).SendKeys(_password);
            _driver.FindElement(By.Id("enter")).Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            wait.Until(drv => drv.FindElements(By.TagName("app-sign-off")).FirstOrDefault());
        }
    }
}