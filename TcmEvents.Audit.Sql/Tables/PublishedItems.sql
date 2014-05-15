-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Table to track all seperate published items
-- =============================================
CREATE TABLE [dbo].[PublishedItems]
(
	[ID] [int] NOT NULL IDENTITY,
	[TransactionID] [int] NOT NULL,
	[PublicationTarget] [varchar](25) NOT NULL,
	[Publication] [varchar](25) NOT NULL,
	[ItemID] [varchar](25) NOT NULL,
	[ItemTitle] [nvarchar](250) NOT NULL,
	[ItemTemplate] [varchar](25) NOT NULL,
	[IsComponentTemplate] [bit] NOT NULL,
	[IsDCP] [bit] NOT NULL, 
    CONSTRAINT [PK_PublishedItems] PRIMARY KEY ([ID]), 
    CONSTRAINT [FK_PublishedItems_PublishTransactions] FOREIGN KEY ([TransactionID]) REFERENCES [PublishTransactions]([ID])
) ON [PRIMARY]
GO

CREATE INDEX [IX_PublishedItems_Publication] ON [dbo].[PublishedItems] ([Publication])
GO

CREATE INDEX [IX_PublishedItems_ItemID] ON [dbo].[PublishedItems] ([ItemID])
GO