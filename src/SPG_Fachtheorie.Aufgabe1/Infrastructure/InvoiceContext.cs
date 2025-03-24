using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class InvoiceContext : DbContext
    {
        public record ArticleWithSalesDto(int ArticleNumber,
                                          string ArticleName,
                                          DateTime PurchaseDate,
                                          string CustomerFirstName,
                                          string CustomerLastName);
        public record EmployeeWithSalesDto(int InvoiceNumber,
                                           DateTime PurchaseDate,
                                           string EmployeeFirstName,
                                           string EmployeeLastName,
                                           decimal Total);

        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Company> Companys => Set<Company>();
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

        private static readonly string[] _customerTypeConversion = new[] { "B", "C" };

        public InvoiceContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>().OwnsOne(c => c.Address);
            modelBuilder.Entity<Customer>().OwnsMany(c => c.Addresses, c =>
            {
                c.HasKey("Id");
                c.Property<int>("Id");
            });
            modelBuilder.Entity<User>().Property(u => u.Email)
                .HasConversion(objValue => objValue.Value, dbValue => new Email(dbValue));
            modelBuilder.Entity<Customer>().Property(c => c.Type)
                .HasConversion(
                    objValue => _customerTypeConversion[(int)objValue],
                    dbValue => (CustomerType)Array.IndexOf(_customerTypeConversion, dbValue)
                );
        }

        /// <summary>
        /// Listen Sie alle Artikel auf, die innerhalb eines bestimmten Zeitraumes gekauft wurden.
        /// Geben Sie Artikelnummer, Artikelname, Kaufdatum und den Vor- und Zunamen des Kunden aus.
        /// </summary>
        public List<ArticleWithSalesDto> GetArticleWithSalesInfo(DateTime from, DateTime to)
        {
            return InvoiceItems.Where(i => i.Invoice.Date > from && to > i.Invoice.Date)
                .Select(i => new ArticleWithSalesDto(
                    i.Article.Number,
                    i.Article.Name,
                    i.Invoice.Date,
                    i.Invoice.Customer.FirstName,
                    i.Invoice.Customer.LastName
                 )).ToList();
        }

        /// <summary>
        /// Listen Sie alle Verkäufe auf, die ein bestimmter Mitarbeiter getätigt hat.
        /// Geben Sie die Rechnungsnummer, das Rechnungsdatum, den Vor- und Zunamen des Kunden
        /// und den Gesamtbetrag aus.
        /// </summary>
        public List<EmployeeWithSalesDto> GetEmployeeWithSales(int employeeId)
        {
            return Invoices
                .Select(i => new EmployeeWithSalesDto(
                    i.Number,
                    i.Date,
                    i.Employee.FirstName,
                    i.Employee.LastName,
                    i.InvoiceItems.Sum(i => i.Quantity * i.Article.Price)
                 )).ToList();

        }
    }
}