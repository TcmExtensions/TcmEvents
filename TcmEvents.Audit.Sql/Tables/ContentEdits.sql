-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Table to track all content changes
-- =============================================
CREATE TABLE [dbo].[ContentEdits]
(
	[ID] [int] NOT NULL IDENTITY,
	[Action] [varchar](16) NOT NULL,
	[ItemID] [varchar](25) NOT NULL,
	[TimeStamp] [datetime] NOT NULL CONSTRAINT [DF_ContentAuthoring_TimeStamp]  DEFAULT (getdate()),
	[ItemTitle] [nvarchar](250) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[UserDescription] [nvarchar](250) NOT NULL,
	[Comments] [nvarchar](250) NULL,
	[Xml] [xml] NULL, 
    CONSTRAINT [PK_ContentEdits] PRIMARY KEY ([ID])
) ON [PRIMARY]
GO

CREATE INDEX [IX_ContentEdits_Action] ON [dbo].[ContentEdits] ([Action])
GO

CREATE INDEX [IX_ContentEdits_ItemID] ON [dbo].[ContentEdits] ([ItemID])
GO

CREATE INDEX [IX_ContentEdits_Username] ON [dbo].[ContentEdits] ([Username])
GO