/****** Script for populating the new payment gateway and paymentdata on Quotes table.  ******/

/****** 
FYI only. This script took 3+ minutes to complete in QA.
******/

IF OBJECT_ID(N'tempdb..#tempEvents', N'U') IS NOT NULL 
   BEGIN  
      DROP TABLE #tempEvents;
   END 
GO  


DECLARE @policyId AS UNIQUEIDENTIFIER
DECLARE @quoteId AS UNIQUEIDENTIFIER
DECLARE @eventJson AS VARCHAR(MAX)
DECLARE @responseJson AS VARCHAR(MAX)
DECLARE @paymentDetailsJson AS VARCHAR(MAX)
DECLARE @paymentGate AS VARCHAR(20)

SELECT * 
INTO #tempEvents
FROM [dbo].[EventRecordWithGuidIds] 
WHERE AggregateId IN (SELECT PolicyId FROM Quotes WHERE IsPaidFor = 1)
AND EventJson LIKE '{"$type":"UBind.Domain.Aggregates.Quote.QuoteAggregate+PaymentMadeEvent%'

DECLARE pTransaction_cursor CURSOR FOR   
SELECT Id, PolicyId FROM Quotes WHERE IsPaidFor = 1
  
OPEN pTransaction_cursor  
FETCH NEXT FROM pTransaction_cursor INTO @quoteId, @policyId


WHILE @@FETCH_STATUS = 0  
    BEGIN  
		    SET @eventJson = NULL
			SET @paymentGate = NULL

			-- there are policy that has more than 1 payment details. 
		    SELECT TOP 1 @eventJson = EventJson FROM #tempEvents
			WHERE AggregateId = @policyId AND EventJson like '%"QuoteId":"' + lower(@quoteId) + '"%'

			IF(@eventJson IS NULL) -- the old payment details dont have quoteId in the property.
			BEGIN
				SELECT @eventJson = EventJson FROM #tempEvents
				WHERE AggregateId = @policyId 
					AND EventJson LIKE '{"$type":"UBind.Domain.Aggregates.Quote.QuoteAggregate+PaymentMadeEvent%'
			END

			SELECT @paymentGate = CASE 
									  WHEN CHARINDEX('pay.stripe.com',@eventJson,0) > 0 THEN 'Stripe'
									  WHEN CHARINDEX('SurchargeRate',@eventJson,0) > 0 THEN 'Deft'
									  WHEN CHARINDEX('BeagleScore',@eventJson,0) > 0 THEN 'EWay'
									  ELSE ''
									END 

			SET @responseJson = JSON_QUERY(@eventJson, '$.PaymentDetails.response')
			PRINT CAST(@quoteId AS VARCHAR(50)) + '---' + @paymentGate
			UPDATE Quotes SET PaymentGateway = @paymentGate, PaymentResponseJson = @responseJson WHERE Id = @quoteId

        FETCH NEXT FROM pTransaction_cursor INTO @quoteId, @policyId
    END  

CLOSE pTransaction_cursor  
DEALLOCATE pTransaction_cursor

IF OBJECT_ID(N'tempdb..#tempEvents', N'U') IS NOT NULL 
   BEGIN  
      DROP TABLE #tempEvents;
   END 