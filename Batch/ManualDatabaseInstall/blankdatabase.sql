USE [FFToiletBowl];

--show if installed already
SELECT physical_name FROM sys.database_files


USE [master];

DROP DATABASE [FFToiletBowl];

CREATE DATABASE
    [FFToiletBowl]
ON PRIMARY (
    NAME=FFToiletBowl_data,
    FILENAME = 'C:\Users\FullPath Where you Want to store it\FFToiletBowl.mdf'
)
LOG ON (
    NAME=FFToiletBowl_log,
    FILENAME = 'c:\FullPath Where you Want to store it\FFToiletBowl.ldf'
);
ALTER DATABASE FFToiletBowl SET RECOVERY SIMPLE;

