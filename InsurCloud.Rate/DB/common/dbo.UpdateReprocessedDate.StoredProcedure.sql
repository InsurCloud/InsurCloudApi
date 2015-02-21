USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[UpdateReprocessedDate]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






create PROCEDURE [dbo].[UpdateReprocessedDate]
	@PaymentIDIN As varchar(50),
	@PostedDateIN As DateTime
AS

UPDATE
Payment
Set ReprocessedDate = @PostedDateIN, ReprocessFlag = 0
WHERE PaymentID = @PaymentIDIN





GO
