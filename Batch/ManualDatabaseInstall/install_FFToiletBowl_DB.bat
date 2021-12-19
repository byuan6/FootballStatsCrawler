echo In blankdatabase.sql, you need to change the path of the .mdf and .ldf files to where you want the database file installed"
echo
echo In this file, if your SQL Server LocalDB instance isn't installed in the path below, or is named differently that "(localdb)\MSSQLLocalDB", you need to change it below.  You can run "SQLLocalDB info", to see if SQL Server LocalDB is installed, and what the instance name is, ie. MSLSQLLocalDB

"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -i "C:\Users\Bob\Downloads\blankdatabase.sql"

echo SELECT 'Finished installing FFToiletBowl 2001-2018'; >> "C:\Users\Bob\Downloads\FFToiletbowl 2001 to 2018.sql"

REM SQLCMD won't process files bigger than 2GB.  So the script was split into 500,000 lines each, and BOM added to each file.
REM "c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -e -i "C:\Users\Bob\Downloads\FFToiletbowl 2001 to 2018.sql" > C:\Users\Bob\Downloads\FFToiletBowl_install_results.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -e -i "FFToiletbowl 2001 to 2018.1-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -e -i "FFToiletbowl 2001 to 2018.2-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.3-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.4-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.5-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.6-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.7-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.8-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.9-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -i "FFToiletbowl 2001 to 2018.10-11.sql" >> log.txt
"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -d ToiletBowl -e -i "FFToiletbowl 2001 to 2018.11-11.sql" >> log.txt

"c:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd" -S "(localdb)\MSSQLLocalDB" -Q "DBCC SHRINKDATABASE(FFToiletBowl,10);"

echo the MDF file you specified in "blankdatabase.sql" should have all the stats loaded.
