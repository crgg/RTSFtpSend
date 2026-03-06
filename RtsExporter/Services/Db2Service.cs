using System.Data.Odbc;
using RtsExporter.Infrastructure;
using RtsExporter.Models;
using Serilog;

namespace RtsExporter.Services;

public class Db2Service
{
    private readonly string _connectionString;

    private const string SqlQuery = """
        SELECT
        '156003REC' "client",
        T.BILL_NUMBER "invoice#",
        T.CUSTOMER "debtorNo",
        T.CALLNAME "Debtor Name",
        (SELECT TRACE_NUMBER FROM TRACE WHERE TRACE_TYPE = 'L' AND DETAIL_NUMBER = T.DETAIL_LINE_ID fetch first row only) "load #",
        T.BILL_DATE "invDate",
        T.TOTAL_CHARGES "InvAmt"
        FROM TLORDER T
        JOIN CLIENT C ON C.CLIENT_ID = T.BILL_TO_CODE
        JOIN REG_AUDIT R ON R.SOURCE_AUDIT = T.INTERFACE_STATUS_F AND R.SOURCE_REG = 'Billing Register'
        WHERE 1 = 1
        AND T.COMPANY_ID = 1
        AND T.EXTRA_STOPS <> 'Child'
        AND T.BILL_TO_CODE NOT IN ('FORBILALS', 'EXPENJEDI', '1486')
        AND R.ENTRY_TIME > ?
        AND R.ENTRY_TIME <= ?
        ORDER BY 1, 2
        """;

    public Db2Service()
    {
        _connectionString = Db2Config.ConnectionString;
    }

    public async Task<IReadOnlyList<InvoiceRecord>> ExecuteQueryAsync(string lastStart, string currStart, CancellationToken ct = default)
    {
        var records = new List<InvoiceRecord>();

        await using var connection = new OdbcConnection(_connectionString);
        await connection.OpenAsync(ct);

        Log.Information("Connecting to DB2 (ODBC)...");
        Log.Information("DB connection success");

        await using var command = new OdbcCommand(SqlQuery, connection);
        command.Parameters.AddWithValue("@lastStart", lastStart);
        command.Parameters.AddWithValue("@currStart", currStart);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            records.Add(new InvoiceRecord
            {
                Client = reader.GetString(0),
                InvoiceNumber = reader.IsDBNull(1) ? "" : reader.GetString(1),
                DebtorNo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                DebtorName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                LoadNumber = reader.IsDBNull(4) ? "" : reader.GetString(4),
                InvDate = reader.IsDBNull(5) ? "" : reader.GetValue(5)?.ToString() ?? "",
                InvAmt = reader.IsDBNull(6) ? "" : reader.GetValue(6)?.ToString() ?? ""
            });
        }

        Log.Information("Query returned {RowCount} rows", records.Count);
        return records;
    }
}
