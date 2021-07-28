using System;
using System.Linq;
using IsraeliFinancialImporter.Importers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Xunit;

namespace IsraeliFinancialImporter.Tests
{
    public class BasicTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly DateTime _fromInclusive = DateTime.Today.Subtract(TimeSpan.FromDays(30));
        private readonly DateTime _toInclusive = DateTime.Today;

        public BasicTests()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            _driver = new ChromeDriver();
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        [Fact]
        public void BadUnionBankLogin()
        {
            var unionBankImporter = new UnionBankImporter(_driver, "", "");
            Assert.Throws<Exception>(() => unionBankImporter.Import(_fromInclusive, _toInclusive));
        }

        [Fact]
        public void BadMaxLogin()
        {
            var unionBankImporter = new MaxImporter(_driver, "", "");
            Assert.Throws<Exception>(() => unionBankImporter.Import(_fromInclusive, _toInclusive));
        }
    }
}