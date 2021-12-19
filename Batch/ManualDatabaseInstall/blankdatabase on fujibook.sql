USE [FFToiletBowl];

SELECT physical_name FROM sys.database_files
--C:\Users\Bob\Downloads\USB30\Projects\FantasyFootball\FFToiletBowl\FFToiletBowl.mdf
--C:\Users\Bob\Downloads\USB30\Projects\FantasyFootball\FFToiletBowl\FFToiletBowl.ldf
USE [master];

DROP DATABASE [FFToiletBowl];

CREATE DATABASE
    [FFToiletBowl]
ON PRIMARY (
    NAME=FFToiletBowl_data,
    FILENAME = 'C:\Users\Bob\Downloads\USB30\Projects\FantasyFootball\FFToiletBowl\FFToiletBowl.mdf'
)
LOG ON (
    NAME=FFToiletBowl_log,
    FILENAME = 'C:\Users\Bob\Downloads\USB30\Projects\FantasyFootball\FFToiletBowl\FFToiletBowl.ldf'
);
ALTER DATABASE FFToiletBowl SET RECOVERY SIMPLE;

