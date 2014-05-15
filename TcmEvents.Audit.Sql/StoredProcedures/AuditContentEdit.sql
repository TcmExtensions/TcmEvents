-- =============================================
-- Author:		Rob van Oostenrijk
-- Create date: 8th May, 2013
-- Description:	Insert a content editing action into the audit log
-- =============================================
CREATE PROCEDURE [dbo].[AuditContentEdit] 
	@Action varchar(16),
	@ItemID varchar(25),
	@ItemTitle nvarchar(250),
	@Username varchar(50),
	@UserDescription nvarchar(250),
	@Comments nvarchar(250),
	@Xml xml
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	INSERT INTO ContentEdits ([Action], [ItemID], [ItemTitle], [Username], [UserDescription], [Comments], [Xml])
		VALUES (@Action, @ItemID, @ItemTitle, @Username, @UserDescription, @Comments, @Xml)
END