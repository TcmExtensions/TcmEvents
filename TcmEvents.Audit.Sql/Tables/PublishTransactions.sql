-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Table to track all publish transactions
-- =============================================
CREATE TABLE [dbo].[PublishTransactions]
(
	[ID] [int] NOT NULL IDENTITY,
	[Transaction] [varchar](25) NOT NULL,
	[Action] [varchar](10) NULL,
	[TimeStamp] [datetime] NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[UserDescription] [nvarchar](250) NULL, 
    CONSTRAINT [PK_PublishTransactions] PRIMARY KEY ([ID])
) ON [PRIMARY]
GO

CREATE INDEX [IX_PublishTransactions_Transaction] ON [dbo].[PublishTransactions] ([Transaction])
GO

CREATE INDEX [IX_PublishTransactions_Username] ON [dbo].[PublishTransactions] ([Username])
GO