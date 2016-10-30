USE [RazorEngineCms.ApplicationContext]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Addison
-- Create date: 2016/10/30
-- Description:	Returns the first include record that matches that passed in @Name and @Type
-- =============================================
CREATE PROCEDURE [dbo].[GetInclude]
(
	@Name NVARCHAR(255),
	@Type NVARCHAR(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT TOP 1 [Id], [Name], [Content], [Type], [Updated]
	FROM [Includes] WHERE [Name] = @Name AND [Type] = @Type;
END
GO
