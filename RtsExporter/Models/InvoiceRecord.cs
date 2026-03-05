using CsvHelper.Configuration.Attributes;

namespace RtsExporter.Models;

public class InvoiceRecord
{
    [Name("client")]
    public string Client { get; set; } = string.Empty;

    [Name("invoice#")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Name("debtorNo")]
    public string DebtorNo { get; set; } = string.Empty;

    [Name("Debtor Name")]
    public string DebtorName { get; set; } = string.Empty;

    [Name("load #")]
    public string LoadNumber { get; set; } = string.Empty;

    [Name("invDate")]
    public string InvDate { get; set; } = string.Empty;

    [Name("InvAmt")]
    public string InvAmt { get; set; } = string.Empty;
}
