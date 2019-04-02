CREATE TABLE [dbo].[Items] (
    [NameKey]     VARCHAR (50) NOT NULL,
    [Name]        VARCHAR (50) NOT NULL,
    [Quantity]    INT          DEFAULT ((0)) NOT NULL,
    [Row]         INT          NOT NULL,
    [Col]         INT          NOT NULL,
    [IsSmallBox]  BIT          DEFAULT ((1)) NOT NULL,
    [DateCreated] DATETIME     NOT NULL,
    [LastUpdated] DATETIME     NULL,
    PRIMARY KEY CLUSTERED ([NameKey] ASC)
);


CREATE TABLE [dbo].[Tags] (
    [Id]      INT          IDENTITY (1, 1) NOT NULL,
    [NameKey] VARCHAR (50) NOT NULL,
    [Tag]     VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Tag_Item] FOREIGN KEY ([NameKey]) REFERENCES [dbo].[Items] ([NameKey])
);


CREATE TABLE [dbo].[Commands] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [DateCreated] DATETIME      NOT NULL,
    [Command]     VARCHAR (50)  NOT NULL,
    [DataIn]      VARCHAR (255) NULL,
    [DataOut]     VARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[HttpRequests] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [HttpRequestBody] VARCHAR (MAX) NULL,
    [DateCreated]     DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);