using System;
using System.Linq;
using IsraeliFinancialImporter.Importers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Xunit;

namespace IsraeliFinancialImporter.Tests
{
    public class BasicTests
    {
        private readonly DateTime _fromInclusive = DateTime.Today.AddMonths(-1);
        private readonly DateTime _toInclusive = DateTime.Today;

        public BasicTests()
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
        }

        [Fact]
        public void BadUnionBankLogin()
        {
            using var unionBankImporter = new UnionBankImporter("", "");
            Assert.ThrowsAny<Exception>(() =>
                unionBankImporter.Import(_fromInclusive, _toInclusive).ToArray());
        }

        [Fact]
        public void BadMaxLogin()
        {
            using var maxImporter = new MaxImporter("", "");
            Assert.ThrowsAny<Exception>(() =>
                maxImporter.Import(_fromInclusive, _toInclusive).ToArray());
        }

        [Fact]
        public void BadCalLogin()
        {
            using var calImporter = new CalImporter("", "");
            Assert.ThrowsAny<Exception>(() =>
                calImporter.Import(_fromInclusive, _toInclusive).ToArray());
        }
    }
}