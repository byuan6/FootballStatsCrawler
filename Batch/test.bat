regreader > servers.txt
set HMM=ABC
echo %HMM%
echo %HMM% >> servers.txt

for /f %%i in ('FFToiletBowlSQL vwLastLoadedGm') do set LAST_GM=%%i
if "%LAST_GM%" == "" exit /b 999
echo %LAST_GM%
