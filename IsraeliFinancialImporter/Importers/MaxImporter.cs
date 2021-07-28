using System;
using System.Collections.Generic;
using IsraeliFinancialImporter.Types;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace IsraeliFinancialImporter.Importers
{
    public class MaxImporter : IFinancialImporter
    {
        private readonly IWebDriver _driver;
        private readonly string _password;
        private readonly string _username;

        public MaxImporter(IWebDriver driver, string username, string password)
        {
            _driver = driver;
            _username = username;
            _password = password;
        }

        public IEnumerable<FinancialAccount> Import(DateTime fromInclusive, DateTime toInclusive)
        {
            Login();
            return null;
        }

        private void Login()
        {
            _driver.Url = "https://www.max.co.il/login";
            _driver.FindElement(By.Id("login-password-link")).Click();
            _driver.FindElement(By.Id("user-name")).SendKeys(_username);
            _driver.FindElement(By.Id("password")).SendKeys($"{_password}\n"); // add \n to submit form

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(drv => drv.Url == "https://www.max.co.il/homepage/personal");
        }
    }
}