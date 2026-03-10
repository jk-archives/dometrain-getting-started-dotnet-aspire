#!/bin/bash
# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
for i in {1..50};
do
    /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P Dometrain#123 -Q "SELECT 1"
    if [ $? -eq 0 ]
    then
        echo "SQL Server is ready."
        break
    else
        echo "Not ready yet..."
        sleep 1
    fi
done

# Run the SQL script
/opt/mssql-tools18/bin/sqlcmd -C -S database -U sa -P Dometrain#123 -d master -i /CreateDatabaseAndSeed.sql
