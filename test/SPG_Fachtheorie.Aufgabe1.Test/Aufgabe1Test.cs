using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    //[Collection("Sequential")]
    public class Aufgabe1Test
    {
        private static readonly DateTime _testDateTime = new DateTime(2025,3,19,12,0,0);

        private InvoiceContext GetEmptyDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;

            var db = new InvoiceContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        private void GenerateFixtures(InvoiceContext db)
        {
            var article1 = new Article("Red Bull", 1.4M);
            var article2 = new Article("Red Bull 2", 1.5M);
            var article3 = new Article("Red Bull 3", 1.6M);
            var company = new Company("Evil Billa", new Address("Hansstraße", "Wien", "1140"));
            var employee = new Employee(company, "Hans", "Peter", "hp@billa.com");
            var customer = new Customer(
                new() { new Address("Peterstraße", "Wien", "1010") },
                CustomerType.B2C,
                null,
                "Max",
                "Mustermann",
                "mm@email.com");
            var invoice = new Invoice(1, _testDateTime, customer, employee);
            var invoiceItem1 = new InvoiceItem(invoice, article1, 1);
            var invoiceItem2 = new InvoiceItem(invoice, article2, 3);
            var invoiceItem3 = new InvoiceItem(invoice, article3, 2);
            invoice.InvoiceItems.AddRange([invoiceItem1, invoiceItem2, invoiceItem3]);
            
            db.Invoices.Add(invoice);

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        /// <summary>
        /// "Dummy Test". Läuft dann durch, wenn EF Core keine Exception liefert.
        /// Deswegen auch kein Assert.
        /// Sollte vor dem Schreiben der weiteren Tests geprüft werden.
        /// </summary>
        [Fact]
        public void CreateDatabaseTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
        }

        /// <summary>
        /// Prüft, ob der rich type Email in User korrekt gespeichert werden kann.
        /// </summary>
        [Fact]
        public void PersistRichTypesSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            Assert.Equal("hp@billa.com", db.Employees.First().Email);

        }
        /// <summary>
        /// Prüft, ob das enum CustomerType korrekt (mit B oder C) gespeichert werden kann.
        /// Hinweis: Erstelle einen Datensatz und lese diesen zurück. Ist der enum Wert korrekt?
        /// </summary>
        [Fact]
        public void PersistEnumSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            Assert.Equal(CustomerType.B2C, db.Customers.First().Type);

        }

        /// <summary>
        /// Prüft, ob das Property Address in Company als value object korrekt gespeichert werden kann.
        /// </summary>
        [Fact]
        public void PersistValueObjectInCompanySuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            Assert.Equal("Hansstraße", db.Companys.First().Address.Street);
            Assert.Equal("Wien", db.Companys.First().Address.City);
            Assert.Equal("1140", db.Companys.First().Address.Zip);

        }

        /// <summary>
        /// Prüft, ob ein Eintrag zur Liste von Adressen in Customer hinzugefügt und wieder
        /// gelesen werden kann.
        /// </summary>
        [Fact]
        public void PersistValueObjectInCustomerSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            Assert.Equal("Peterstraße", db.Customers.First().Addresses.First().Street);
            Assert.Equal("Wien", db.Customers.First().Addresses.First().City);
            Assert.Equal("1010", db.Customers.First().Addresses.First().Zip);

        }

        /// <summary>
        /// Das Entity InvoiceItem soll in der Datenbank gespeichert werden.
        /// Es referenziert auf alle anderen Entities, deswegen reicht dieser eine Test,
        /// um die korrekte Speicherung aller Entities zu prüfen.
        /// </summary>
        [Fact]
        public void PersistInvoiceItemSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            Assert.Equal(3, db.InvoiceItems.Count());
        }

        /// <summary>
        /// Unittest für die Methode GetArticleWithSalesInfo in InvoiceContext.
        /// </summary>
        [Fact]
        public void GetArticleWithSalesInfoSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            var articles = db.GetArticleWithSalesInfo(
                new DateTime(2025, 3, 18, 12, 0, 0),
                new DateTime(2025, 3, 20, 12, 0, 0));
            Assert.Equal(3, articles.Count());
        }

        /// <summary>
        /// Unittest für die Methode GetEmployeeWithSales in InvoiceContext.
        /// </summary>
        [Fact]
        public void GetEmployeeWithSalesSuccessTest()
        {
            using var db = GetEmptyDbContext();
            GenerateFixtures(db);
            var sales = db.GetEmployeeWithSales(db.Employees.First().Id);
            Assert.True(sales.Count() > 0);
        }

    }
}