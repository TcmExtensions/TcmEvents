-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Insert a publish transaction into the publish transaction log
-- =============================================
CREATE PROCEDURE [dbo].[AuditPublishTransaction] 
	@Transaction varchar(25),
	@Action varchar(10),
	@TimeStamp datetime,
	@Username varchar(50),
	@UserDescription nvarchar(250)
AS
BEGIN
	DECLARE @TransactionCount AS int

	SELECT @TransactionCount = COUNT(*)
	FROM PublishTransactions 
	WHERE [Transaction] = @Transaction
	AND [TimeStamp] = @TimeStamp

	IF (@TransactionCount = 0)
	BEGIN	
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		INSERT INTO PublishTransactions ([Transaction], [Action], [TimeStamp], [Username], [UserDescription])
			VALUES (@Transaction, @Action, @TimeStamp, @Username, @UserDescription)
	
		SELECT CAST(scope_identity() AS int)
	END
	ELSE SELECT -1
END