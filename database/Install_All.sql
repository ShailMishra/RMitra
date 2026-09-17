/*
    RasoiMitra database installer.
    Creates tbl* tables, mas* masters, usp* procedures, and seed rows.

    Local Windows auth:
        sqlcmd -S "(localdb)\MSSQLLocalDB" -E -I -i Install_All.sql
    Azure SQL (database must already exist):
        sqlcmd -S tcp:YOUR_SERVER.database.windows.net,1433 -d RasoiMitra -U USER -P PASSWORD -I -i Install_All.sql
*/

:r 00_CreateDatabase.sql
:r Modules\01_Masters.sql
:r Modules\02_Tables.sql
:r Modules\03_Procedures.sql
:r Modules\04_Procedures_Ordering.sql
:r Modules\05_Procedures_Commerce.sql
:r Modules\99_SeedData.sql
