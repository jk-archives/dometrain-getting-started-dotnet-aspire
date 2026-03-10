IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Podcasts' AND xtype='U')
BEGIN
    CREATE TABLE Podcasts
    (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Title NVARCHAR(MAX) NOT NULL
    )
END
GO

USE podcasts
GO

IF NOT EXISTS (SELECT * FROM Podcasts)
BEGIN
    INSERT INTO Podcasts (Title)
    VALUES
    ('Unhandled Exception Podcast'),
    ('Keep Coding Podcast'),
    ('Developer Weekly Podcast'),
    ('The Stack Overflow Podcast'),
    ('The Hanselminutes Podcast'),
    ('The .NET Rocks Podcast'),
    ('The Azure Podcast'),
    ('The AWS Podcast'),
    ('The Rabbit Hole Podcast'),
    ('The .NET Core Podcast');
END
GO