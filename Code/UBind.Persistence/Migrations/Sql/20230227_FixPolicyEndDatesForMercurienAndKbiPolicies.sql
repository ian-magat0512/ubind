-- Update policy end date and related tables
-- JIRA Ticket: https://jira.aptiture.com/browse/UB-8500

IF OBJECT_ID('tempdb..#fixPolicyEndDate') IS NOT NULL DROP PROCEDURE #fixPolicyEndDate
GO

CREATE PROCEDURE #fixPolicyEndDate
	@aggregateID UNIQUEIDENTIFIER,
	@oldPolicyEndDate NVARCHAR(MAX),
	@expiryDateTime DATETIME2,
	@policyState NVARCHAR(MAX) = 'Active',
	@expiryTicksSinceEpoch BIGINT = NULL
AS
BEGIN
	SELECT @expiryTicksSinceEpoch
	IF @expiryTicksSinceEpoch IS NULL
	BEGIN
		SET @expiryTicksSinceEpoch = DATEDIFF(s, '1970-01-01', @expiryDateTime) + (6*60*60)
		SET @expiryTicksSinceEpoch = @expiryTicksSinceEpoch * 10000000
	 END

	UPDATE PolicyReadModels
	SET ExpiryTicksSinceEpoch = @expiryTicksSinceEpoch,
		ExpiryDateTime = @expiryDateTime,
		PolicyState = @policyState
	WHERE Id = @aggregateID

	DECLARE @newPolicyEndDate AS NVARCHAR(max) = CONVERT(NVARCHAR(max), @expiryDateTime, 103)	-- dd/mm/yyyy
	DECLARE @oldYear VARCHAR(4) = RIGHT(@oldPolicyEndDate, 4), @newYear VARCHAR(4) = RIGHT(@newPolicyEndDate, 4),
			@oldMonth VARCHAR(2) = SUBSTRING(@oldPolicyEndDate, 4, 2), @newMonth VARCHAR(2) = SUBSTRING(@newPolicyEndDate, 4, 2), 
			@oldDay VARCHAR(2) = LEFT(@oldPolicyEndDate, 2), @newDay VARCHAR(2) = LEFT(@newPolicyEndDate, 2), 
			@newExpiryDays INT = datediff(D, '1970-01-01', @expiryDateTime),
			@oldExpiryDays INT = datediff(D, '1970-01-01', convert(datetime, @oldPolicyEndDate, 103))

	DECLARE @endDateOldExpr1 VARCHAR(MAX) = CONCAT('"EndDate":{"year":', @oldYear, ',"month":', @oldMonth, ',"day":', @oldDay, ','),
			@endDateNewExpr1 VARCHAR(MAX) = CONCAT('"EndDate":{"year":', @newYear, ',"month":', @newMonth, ',"day":', @newDay, ','),
			@endDateOldExpr2 VARCHAR(MAX) = CONCAT('"EndDate":{"$type":"NodaTime.LocalDate, NodaTime","year":', @oldYear, '"month":', @oldMonth, ',"day":', @oldDay, ','),
			@endDateNewExpr2 VARCHAR(MAX) = CONCAT('"EndDate":{"$type":"NodaTime.LocalDate, NodaTime","year":', @newYear, '"month":', @newMonth, ',"day":', @newDay, ',')
	DECLARE @policyEndDateOldExpr1 VARCHAR(MAX) = CONCAT('\"policyEndDate\": \"', @oldPolicyEndDate, '\"'),
			@policyEndDateNewExpr1 VARCHAR(MAX) = CONCAT('\"policyEndDate\": \"', @newPolicyEndDate, '\"'),
			@policyEndDateOldExpr2 VARCHAR(MAX) = CONCAT('"policyEndDate": "', @oldPolicyEndDate, '"'),
			@policyEndDateNewExpr2 VARCHAR(MAX) = CONCAT('"policyEndDate": "', @newPolicyEndDate, '"'),
			@policyEndDateOldExpr3 VARCHAR(MAX) = CONCAT('"policyEndDate":"', @oldPolicyEndDate, '"'),
			@policyEndDateNewExpr3 VARCHAR(MAX) = CONCAT('"policyEndDate":"', @newPolicyEndDate, '"')
	DECLARE @expiryDateOldExpr1 VARCHAR(MAX) = CONCAT('"ExpiryDate":{"$type":"NodaTime.LocalDate, NodaTime","year":', @oldYear, '"month":', @oldMonth, ',"day":', @oldDay, ','),
			@expiryDateNewExpr1 VARCHAR(MAX) = CONCAT('"ExpiryDate":{"$type":"NodaTime.LocalDate, NodaTime","year":', @newYear, '"month":', @newMonth, ',"day":', @newDay, ',')
	DECLARE @expiryTimeOldExpr1 VARCHAR(MAX) = CONCAT('"ExpiryTime":{"$type":"NodaTime.Instant, NodaTime","days":', @oldExpiryDays, ','),
			@expiryTimeNewExpr1 VARCHAR(MAX) = CONCAT('"ExpiryTime":{"$type":"NodaTime.Instant, NodaTime","days":', @newExpiryDays, ',')

	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @policyEndDateOldExpr1, @policyEndDateNewExpr1) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @policyEndDateOldExpr2, @policyEndDateNewExpr2) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @policyEndDateOldExpr3, @policyEndDateNewExpr3) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @endDateOldExpr1, @endDateNewExpr1) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @endDateOldExpr2, @endDateNewExpr2) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @expiryDateOldExpr1, @expiryDateNewExpr1) WHERE Id = @aggregateID;
	UPDATE dbo.PolicyReadModels SET SerializedCalculationResult = REPLACE(SerializedCalculationResult, @expiryTimeOldExpr1, @expiryTimeNewExpr1) WHERE Id = @aggregateID;

	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @policyEndDateOldExpr1, @policyEndDateNewExpr1) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @policyEndDateOldExpr2, @policyEndDateNewExpr2) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @policyEndDateOldExpr3, @policyEndDateNewExpr3) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @endDateOldExpr1, @endDateNewExpr1) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @endDateOldExpr2, @endDateNewExpr2) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @expiryDateOldExpr1, @expiryDateNewExpr1) WHERE AggregateId = @aggregateId;
	UPDATE dbo.EventRecordWithGuidIds SET EventJson = REPLACE(EventJson, @expiryTimeOldExpr1, @expiryTimeNewExpr1) WHERE AggregateId = @aggregateId;

	UPDATE dbo.PolicyTransactions SET ExpiryTicksSinceEpoch = @expiryTicksSinceEpoch, ExpiryDateTime = @expiryDateTime WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @policyEndDateOldExpr1, @policyEndDateNewExpr1) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @policyEndDateOldExpr2, @policyEndDateNewExpr2) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @policyEndDateOldExpr3, @policyEndDateNewExpr3) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @endDateOldExpr1, @endDateNewExpr1) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @endDateOldExpr2, @endDateNewExpr2) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @expiryDateOldExpr1, @expiryDateNewExpr1) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_FormData = REPLACE(PolicyData_FormData, @expiryTimeOldExpr1, @expiryTimeNewExpr1) WHERE PolicyId = @aggregateId;

	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @policyEndDateOldExpr1, @policyEndDateNewExpr1) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @policyEndDateOldExpr2, @policyEndDateNewExpr2) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @policyEndDateOldExpr3, @policyEndDateNewExpr3) WHERE PolicyId = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @endDateOldExpr1, @endDateNewExpr1) WHERE Id = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @endDateOldExpr2, @endDateNewExpr2) WHERE Id = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @expiryDateOldExpr1, @expiryDateNewExpr1) WHERE Id = @aggregateId;
	UPDATE dbo.PolicyTransactions SET PolicyData_SerializedCalculationResult = REPLACE(PolicyData_SerializedCalculationResult, @expiryTimeOldExpr1, @expiryTimeNewExpr1) WHERE PolicyId = @aggregateId;
END
GO

-- TYSON HOWARD (CMO1000011) - required end date 17 Jun 2023
EXEC #fixPolicyEndDate
	'4b99f7dc-c7df-463c-a227-e15fda153ff7',	-- Aggregate Id
	'17/06/2022',							-- Current Policy End Date: dd/mm/yyyy
	'2023-06-17 00:00:00.0000000',			-- Required End Date
	'Active',								-- Policy State
	16869816000000000						-- Expiry Ticks Since Epoch

-- CMO1000012 - Scott Sadler
EXEC #fixPolicyEndDate
	'024accd6-7e6e-4afd-8077-290e6be8b77e',	-- Aggregate Id
	'15/08/2022',							-- Current Policy End Date: dd/mm/yyyy
	'2023-08-15 00:00:00.0000000',			-- Required End Date
	'Active',								-- Policy State
	16920792000000000						-- Expiry Ticks Since Epoch

-- Kelly King - CY-0312 - required end date 27 Oct 2023, from 25 Nov 2022
-- https://app.ubind.com.au/portal/kbi/quote/7729ef9c-ac73-418d-8a95-d390b64988cd
EXEC #fixPolicyEndDate
	'ca3261c4-e08c-4a1c-8dd2-ddb95edbb2d6',	-- Aggregate Id
	'25/11/2022',							-- Current Policy End Date: dd/mm/yyyy
	'2023-10-27 00:00:00.0000000'			-- Required End Date

-- Jonathan Selim - CY-0313 - required end date 10 Nov 2023, from 10 Dec 2022
-- https://app.ubind.com.au/portal/kbi/quote/80b81637-ea40-40e0-ad6f-3b9baf6464e0
EXEC #fixPolicyEndDate
	'a7cdb476-35b4-4825-9b78-3c12e27e6bd3',	-- Aggregate Id
	'10/12/2022',							-- Current Policy End Date: dd/mm/yyyy
	'2023-11-10 00:00:00.0000000'			-- Required End Date
