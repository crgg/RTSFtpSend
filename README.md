# RtsExporter

A .NET 8 Console Application that exports invoice data from DB2 to CSV and uploads via FTP.

## Requirements

- .NET 8 SDK
- Windows Server (for production)
- Access to DB2 database and FTP server

## Configuration

Copy `.env` to the project root and configure:

```env
# DB2 (DB_HOST or DB2_HOST, DB_NAME or DB2_DB, etc.)
DB_HOST=your-db-host
DB_PORT=50000
DB_NAME=your-database
DB_USER=your-user
DB_PASSWORD=your-password

FTP_HOST=ftp.example.com
FTP_USER=your-ftp-user
FTP_PASSWORD=your-ftp-password

OUTPUT_FOLDER=./output
PATH_CSVFILE=./output

# Windows Registry - LASTSTART se lee y actualiza automáticamente
REGISTRY_PATH=Software\RtsExporter
LASTSTART_FALLBACK=2026-03-04 06:00:00
```

- **PATH_CSVFILE**: Folder where CSV files are saved (overrides OUTPUT_FOLDER if set)
- **REGISTRY_PATH**: Registry key for LASTSTART (default: `HKCU\Software\RtsExporter`)
- **LASTSTART_FALLBACK**: Used on first run when registry is empty

## Build & Run

```bash
dotnet build
dotnet run --project RtsExporter
```

## Scheduling

Configure Windows Task Scheduler to run daily at 10:53 AM:

- Program: `RtsExporter.exe` (or full path to published executable)
- Start in: Solution directory (where `.env` is located)

## Output

- CSV files: `RTS_YYYYMMDDHHmmssfff.csv` (e.g. `RTS_20260304133713422.csv`)
- Logs: `logs/app.log`
