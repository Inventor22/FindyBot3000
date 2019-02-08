CREATE TABLE [dbo].[Item]
(
    [Name] NVARCHAR(50) NOT NULL, 
    [Quantity] INT NOT NULL DEFAULT 0, 
    [TimeCreated] DATETIME NOT NULL, 
    [Row] INT NOT NULL, 
    [Col] INT NOT NULL, 
    [LastUpdated] DATETIME NULL, 
    PRIMARY KEY ([Name]) 
)

CREATE TABLE [dbo].[HttpRequests]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [HttpRequestBody] NVARCHAR(MAX) NULL, 
    [DateCreated] DATETIME NOT NULL
)

CREATE TABLE [dbo].[Commands]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [DateCreated] DATETIME NOT NULL, 
    [Command] VARCHAR(50) NOT NULL, 
    [DataIn] VARCHAR(255) NULL, 
    [DataOut] VARCHAR(255) NULL
)

CREATE TABLE [dbo].[Tags]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [Tag] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [FK_Tag_Item] FOREIGN KEY ([Name]) REFERENCES [Item]([Name])
)
