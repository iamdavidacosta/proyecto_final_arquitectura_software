UPDATE mysql_users SET password='FileShare@123!' WHERE username='fileshare_user';
UPDATE mysql_users SET password='RootP@ssw0rd!2024' WHERE username='root';
LOAD MYSQL USERS TO RUNTIME;
SAVE MYSQL USERS TO DISK;
