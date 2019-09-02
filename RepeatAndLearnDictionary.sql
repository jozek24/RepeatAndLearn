USE master;
GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RepeatAndLearnDictionary')
Begin
CREATE DATABASE RepeatAndLearnDictionary
END;
GO
USE RepeatAndLearnDictionary
GO


CREATE TABLE Words (
    IdWord   int NOT NULL IDENTITY(1,1) PRIMARY KEY,
    PlWord   VARCHAR(255) NOT NULL,
    EnWord   VARCHAR(255) NOT NULL,
    DateOfNextRepeat     datetime DEFAULT GETDATE(),
    CurrentAmountOfRepeats   INT NOT NULL DEFAULT 0,
    TotalAmountOfRepeats   INT NOT NULL DEFAULT 0
)


CREATE PROC SelectAllWords
@plWord varchar(255),
@enWord varchar(255)
AS BEGIN
SELECT *
FROM Words
WHERE PlWord=@plWord AND EnWord=@enWord
END


CREATE PROC AddNewWord
@plWord varchar(255),
@enWord varchar(255),
@dateOfNextRepeat datetime,
@currentAmountOfRepeats int,
@totalAmountOfRepeats int
AS BEGIN
INSERT INTO Words(PlWord,
EnWord,
DateOfNextRepeat,
CurrentAmountOfRepeats,
TotalAmountOfRepeats)
VALUES
(@plWord,
@enWord,
@dateOfNextRepeat,
@currentAmountOfRepeats,
@totalAmountOfRepeats)
END


CREATE PROC DeleteOldWord
@plWord varchar(255),
@enWord varchar(255)
AS BEGIN
DELETE FROM Words
WHERE PlWord=@plWord AND EnWord=@enWord
END


CREATE PROC UpdateWordOnCorrect
@idOfWord varchar(255),
@dateOfNextRepeat datetime,
@totalAmountOfRepeats int
AS BEGIN
UPDATE Words SET 
DateOfNextRepeat=@dateOfNextRepeat,
CurrentAmountOfRepeats=0,
TotalAmountOfRepeats=@totalAmountOfRepeats
WHERE
IdWord=@idOfWord
END


CREATE PROC UpdateWordOnWrong
@idOfWord varchar(255),
@dateOfNextRepeat datetime,
@currentAmountOfRepeats int
AS BEGIN
UPDATE Words SET 
DateOfNextRepeat=@dateOfNextRepeat,
CurrentAmountOfRepeats=@currentAmountOfRepeats
WHERE
IdWord=@idOfWord
END
