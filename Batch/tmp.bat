SET BACKUPFILE=FFToiletBowl.visualstudio.bak

SqlCmd -E -S "FUJIBOOKPRO-WIN\SQLSERVER2008" -Q "BACKUP DATABASE FFToiletBowl TO DISK='%BACKUPFILE%'"
SET PATHSQL="set nocount on;select physical_device_name from msdb.dbo.backupmediafamily where physical_device_name like '%%%BACKUPFILE%%%'"
SET PATHCMD=SqlCmd -U toilet -P toiletbowl -S "FUJIBOOKPRO-WIN\SQLSERVER2008" -W -Q %PATHSQL%
for /f "tokens=1* delims=" %%a in ('%PATHCMD%') do set BACKUPPATH=%%a

echo %BACKUPPATH%

SET VSPATH=$(ProjectDir)%BACKUPFILE%
IF EXIST "%VSPATH%" del "%VSPATH%"

move "%BACKUPPATH%" "%VSPATH%"
