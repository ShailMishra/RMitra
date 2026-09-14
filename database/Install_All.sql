/*
    HOMELY / RMitra dedicated database installer.
    sqlcmd -S localhost -E -i Install_All.sql
*/

:r 00_CreateDatabase.sql
:r Modules\01_Identity.sql
:r Modules\02_Kitchen.sql
:r Modules\03_Catalog.sql
:r Modules\04_Customer.sql
:r Modules\05_Ordering.sql
:r Modules\06_Payment.sql
:r Modules\07_Delivery.sql
:r Modules\08_Review.sql
:r Modules\99_SeedData.sql
