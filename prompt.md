# Create the corrected markdown prompt file so the user can download it

content = """

# .NET DB2 → CSV → FTP Automation

Act as a **Senior .NET Engineer specialized in Windows Console applications, DB2 integration, and automation jobs.**

I need to build a **.NET Console Application** that runs on **Windows Server** and performs the following automated workflow.

The solution must be **production ready**, follow **clean architecture**, and be **easy to maintain**.

Use **.NET 8 Console Application**.

---

# Requirements

The application must:

- Connect to a **DB2 database**
- Read configuration from a **.env file**
- Execute a **SQL query**
- Export the result to a **CSV file**
- Upload the CSV file via **FTP**
- Be designed to run daily at **10:53 AM using Windows Task Scheduler**

---

# Environment Variables (.env)

Use a `.env` file in the root folder.

Example:

DB_HOST=xxxx
DB_PORT=50000
DB_NAME=xxxx
DB_USER=xxxx
DB_PASSWORD=xxxx

FTP_HOST=ftp.rtsfdd.com
FTP_USER=156003REC
FTP_PASSWORD=POLMAX

OUTPUT_FOLDER=./output
PATH_CSVFILE=./output

LASTSTART=2026-03-04 06:00:00
CURRSTART=2026-03-04 18:00:00

Load environment variables using a .NET compatible dotenv library.

Recommended library:

DotNetEnv

---

# Database

Use **IBM DB2**.

Use the official provider:

IBM.Data.DB2.Core

Connection must be created from the environment variables.

Example connection string format:

Server=${DB_HOST}:${DB_PORT};
Database=${DB_NAME};
UID=${DB_USER};
PWD=${DB_PASSWORD};

---

# SQL Query

Execute this query using **parameterized dates** from environment variables:

- **LASTSTART** – start of the time window (exclusive: `R.ENTRY_TIME > LASTSTART`)
- **CURRSTART** – end of the time window (inclusive: `R.ENTRY_TIME <= CURRSTART`)

Read `LASTSTART` and `CURRSTART` from `.env` and pass them as parameters to avoid SQL injection.

Query:

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
AND R.ENTRY_TIME > @LASTSTART
AND R.ENTRY_TIME <= @CURRSTART
ORDER BY 1, 2

The query results must be loaded using a **DB2 DataReader**.

---

# CSV File Generation

Generate a CSV file with headers.

File name format:

rts_export_YYYYMMDD_HHmm.csv

Example:

rts_export_20260304_1053.csv

CSV must contain the following columns:

client
invoice#
debtorNo
Debtor Name
load #
invDate
InvAmt

Use a reliable CSV library.

Recommended:

CsvHelper

Encoding:

UTF-8

Delimiter:

comma

---

# FTP Upload

After generating the CSV file, upload it via FTP.

Use the environment variables:

FTP_HOST  
FTP_USER  
FTP_PASSWORD

Upload using **Passive FTP Mode**.

The uploaded filename must match the generated CSV filename.

If upload succeeds:

log success

If upload fails:

log error and exit with code 1

---

# Logging

Implement structured logging.

Log the following events:

- Application start
- DB connection success
- Number of rows exported
- CSV file path
- FTP upload status
- Errors

Use:

Serilog

Log file location:

logs/app.log

---

# Project Structure

RtsExporter/

Program.cs  
.env  

Services/  
   Db2Service.cs  
   CsvService.cs  
   FtpService.cs  

Models/  
   InvoiceRecord.cs  

Infrastructure/  
   EnvLoader.cs  
   Logger.cs  

output/  
logs/  

---

# Error Handling

Handle errors for:

- DB connection failure
- Query execution failure
- CSV generation failure
- FTP upload failure

Return proper **exit codes**.

---

# Scheduling

The application **does not manage scheduling internally**.

It will be executed using **Windows Task Scheduler**.

Execution example:

RtsExporter.exe

Scheduled daily at:

10:53 AM

---

# Expected Output Example

[INFO] Starting export process  
[INFO] Connecting to DB2...  
[INFO] Query returned 124 rows  
[INFO] CSV generated: output/rts_export_20260304_1053.csv  
[INFO] Uploading file to FTP...  
[INFO] Upload completed successfully  
"""

 