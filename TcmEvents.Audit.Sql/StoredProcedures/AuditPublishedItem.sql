-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Insert a seperate published item into the published items log
-- =============================================
CREATE PROCEDURE [dbo].[AuditPublishedItem] 
	@TransactionID int,
	@PublicationTarget varchar(25),
	@Publication varchar(25),
	@ItemID varchar(25),
	@ItemTitle nvarchar(250),
	@ItemTemplate varchar(25),
	@IsComponentTemplate bit,
	@IsDCP bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	INSERT INTO PublishedItems ([TransactionID], [PublicationTarget], [Publication], [ItemID], [ItemTitle], [ItemTemplate], [IsComponentTemplate], [IsDCP])
		VALUES (@TransactionID, @PublicationTarget, @Publication, @ItemID, @ItemTitle, @ItemTemplate, @IsComponentTemplate, @IsDCP)
END