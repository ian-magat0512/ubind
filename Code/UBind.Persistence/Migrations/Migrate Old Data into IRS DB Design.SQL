
If((Select Count(*) [cnt] from INFORMATION_SCHEMA.Tables a
where a.TABLE_NAME IN ('Quotes', 'PolicyReadModels', 'PolicyTransactions')) <> 3)
BEGIN

	Declare @PrintMessage varchar(1000) = 'Running this script requires TARGET TABLES =>  Quotes, PolicyReadmodel and PolicyTransactions.';
	Print @PrintMessage;
	THROW 51000, @PrintMessage, 1;  
END


		PRINT 'TARGET TABLES  Quotes, PolicyReadmodel and PolicyTransactions - FOUND'

		PRINT 'Migrating Values of Old QuoteReadModels to Quotes, PolicyReadmodel and PolicyTransactions'

		SELECT QuoteId,
		   ROW_NUMBER() OVER (
		 ORDER BY [CreationTimeTicksSinceEpoch]
		   ) as RowNumber 
		  into #QuoteReadModelTemp
		FROM [QuoteReadModels]
		where CustomerFullName <> 'Monitoring Test' and Environment = 3

		DECLARE @RowsCount INT = (Select Count(QuoteiD) from #QuoteReadModelTemp);

		DECLARE @Counter INT 
		SET @Counter=1
		WHILE (@Counter <= @RowsCount)
		BEGIN
			Declare @CurrentId uniqueidentifier = (select QuoteId from  #QuoteReadModelTemp where Rownumber = @Counter);
			PRINT '------------------------------------------------------------------------------------------------------------------------------------';
			PRINT 'RowNumber =' + convert(varchar(100),@Counter) + 'QuoteID = ' + convert(varchar(100),@CurrentId);
	
			PRINT 'Setting up reference to the current row of OLD QuoteReadModel'
			SELECT top 1 *, @counter [RowNumber]
				INTO #Temp
				FROM QuoteReadModels
				where QuoteId = @CurrentId;

				BEGIN TRY
					BEGIN TRAN
			  
				        Declare @QuoteState varchar(20) = 'Incomplete'
                      
						Declare @IsNascent bit = 0;
                
				        Declare @Iscomplete bit = 0;
                
				        SET @Iscomplete = (select 1 from #Temp
                        where (PolicyIssueTimeTicksSinceEpoch > 0
                        OR IsSubmitted = 1
                        )
                        and QuoteNumber IS NOT NULL
                            );
                      
					  SET @IsNascent = (select 1 from #Temp
                        where (PolicyIssueTimeTicksSinceEpoch  = 0
                        and IsSubmitted  = 0
                        )
                        and QuoteNumber IS NULL
                      );

                      IF(@Iscomplete = 1)
                      BEGIN
                        SET @QuoteState = 'Complete';
                      END
                      IF(@IsNascent = 1)
                      BEGIN
                        SET @QuoteState = 'Nascent';
                      END  
                
					  --PolicyIssued should be true if QuoteReadModel.PolicyIssueTimeTicksSinceEpoch > 0, otherwise false
					  PRINT 'set the value of PolicyIssued'
					  DECLARE @PolicyIssued bit =0;
					  If((select PolicyIssueTimeTicksSinceEpoch from #Temp) > 0)
					  BEGIN
						Set @PolicyIssued = 1;
					  END

					DECLARE @SerializedPolicyCalculationResult nvarchar(max) = null;
				
					if (SELECT PolicyCalculationResultJson
					FROM #Temp) is not null
					BEGIN
						 --PRINT 'Getting the current SerializedPolicyCalculationResult'
						SET @SerializedPolicyCalculationResult = (SELECT convert(nvarchar(MAx),(
						SELECT PolicyCalculationResultJson as [Json] 
						FROM #Temp
						for JSON Path, WITHOUT_ARRAY_WRAPPER)));
					END

				    PRINT 'Inserting to PolicyReadModels';
			 
					 INSERT INTO PolicyReadModels(
						Id	
						,IsDiscarded
						,SerializedCalculationResult
						,TenantId
						,ProductId
						,Environment
						,IsTestData
						,QuoteId
						,OwnerUserId
						,OwnerPersonId
						,OwnerFullName
						,CustomerId
						,CustomerPersonId
						,CustomerFullName
						,CustomerPreferredName
						,CustomerEmail
						,CustomerAlternativeEmail
						,CustomerMobilePhone
						,CustomerHomePhone
						,CustomerWorkPhone
						,PolicyIssueTimeInTicksSinceEpoch
						,PolicyNumber
						,InceptionDateAsDateTime
						,ExpiryDateAsDateTime
						,InceptionTimeAsTicksSinceEpoch
						,ExpiryTimeAsTicksSinceEpoch
						,CancellationEffectiveTimeInTicksSinceEpoch
						,CreationTimeInTicksSinceEpoch
						,LastUpdatedTicksSinceEpoch)
				
						Select
								QuoteId
								,0
								, @SerializedPolicyCalculationResult
								,TenantId
								,ProductId
								,Environment
								,IsTestData
								,QuoteId
								,OwnerUserId
								,OwnerPersonId
								,OwnerFullName
								,CustomerId
								,CustomerPersonId
								,CustomerFullName
								,CustomerPreferredName
								,CustomerEmail
								,CustomerAlternativeEmail
								,CustomerMobilePhone
								,CustomerHomePhone
								,CustomerWorkPhone
								,PolicyIssueTimeTicksSinceEpoch
								,PolicyNumber
						
								,CASE
									WHEN PolicyInceptionDateAsDateTime < '1800-1-1'  THEN null

									ELSE PolicyInceptionDateAsDateTime
									END
								,CASE
									WHEN PolicyExpiryDateAsDateTime < '1800-1-1'  THEN null

									ELSE PolicyExpiryDateAsDateTime
									END
								,PolicyInceptionTimeAsTicksSinceEpoch
								,PolicyExpiryTimeAsTicksSinceEpoch	
								,0
								,CreationTimeTicksSinceEpoch
								,LastUpdatedTicksSinceEpoch
								from #Temp;

					--PRINT 'Getting the current SerializedLatestCalculationResult'
					DECLARE @SerializedLatestCalculationResult nvarchar(max) = null;
					If( (SELECT LatestCalculationResultJson  
					FROM #Temp) is not null)
					BEGIN
					
					SET @SerializedLatestCalculationResult = (Select convert(nvarchar(MAx),(
					SELECT LatestCalculationResultJson as [Json] 
					FROM #Temp
					for JSON Path, WITHOUT_ARRAY_WRAPPER)));

					END

					 PRINT 'Inserting to Quotes Table'

						INSERT INTO QUOTES(ID,
						EventSequenceNumber,
    					 PolicyId
						,[Type]
						,QuoteNumber
						,SerializedLatestCalculationResult
						,QuoteState
						,IsDiscarded
						,PolicyIssued
						,TotalPayable
						,LastUpdatedTicksSinceEpoch
						,InvoiceTimeAsTicksSinceEpoch
						,SubmissionTimeAsTicksSinceEpoch
						,PaymentTimeAsTicksSinceEpoch
						,FundingTimeAsTicksSinceEpoch
						,WorkflowStep
						,CreationTimeInTicksSinceEpoch
						,IsSubmitted
						,IsInvoiced
						,InvoiceNumber
						,IsPaidFor
						,PaymentReference
						,IsFunded
						,FundingId
						,AmountFunded
						,FundingPaymentFrequency
						,FundingNumberOfInstallments
						,FundingInitialInstalmentAmount
						,FundingRegularInstalmentAmount
						,LatestFormData
						,LatestCalculationResultId
						,LatestCalculationResultFormDataId
						,LatestCalculationResultJson)

						select QuoteId,
								0,
							QuoteId
							, 0
							,QuoteNumber
							,@SerializedLatestCalculationResult
							,@QuoteState
							,0
							,@PolicyIssued
							,TotalPayable
							,LastUpdatedTicksSinceEpoch
							,InvoiceTimeAsTicksSinceEpoch
							,SubmissionTimeAsTicksSinceEpoch
							,PaymentTimeAsTicksSinceEpoch
							,FundingTimeAsTicksSinceEpoch
							,WorkflowStep
							,CreationTimeTicksSinceEpoch
							,IsSubmitted
							,IsInvoiced
							,InvoiceNumber
							,IsPaidFor
							,PaymentReference
							,IsFunded
							,FundingId
							,AmountFunded
							,FundingPaymentFrequency
							,FundingNumberOfInstallments
							,FundingInitialInstalmentAmount
							,FundingRegularInstalmentAmount
							,LatestFormData
							,LatestCalculationResultId
							,LatestCalculationResultFormDataId
							,LatestCalculationResultJson
							from #Temp;
						
					 if @PolicyIssued = 1
					 BEGIN
						 PRINT 'Inserting to PolicyTransaction';

						 INSERT INTO PolicyTransactions(ID,
							PolicyId
							,QuoteId
							,EffectiveTimeInTicksSinceEpoch
							,CreationTimeInTicksSinceEpoch
							,PolicyData_FormData
							,PolicyData_SerializedCalculationResult
							,PolicyData_InceptionTimeInTicksSinceEpoch
							,PolicyData_ExpiryTimeInTicksSinceEpoch
							,PolicyData_InceptionDateAsDateTime
							,PolicyData_ExpiryDateAsDateTime
							,Discriminator)

							Select 
							QuoteId
							,QuoteId
							,QuoteId
							,PolicyInceptionTimeAsTicksSinceEpoch
							,CreationTimeTicksSinceEpoch
							,LatestFormData
							,@SerializedPolicyCalculationResult
							,PolicyInceptionTimeAsTicksSinceEpoch
							,PolicyExpiryTimeAsTicksSinceEpoch
							,PolicyInceptionDateAsDateTime
							,PolicyExpiryDateAsDateTime
							,'NewBusinessTransaction'
							from #Temp
					END

					COMMIT TRAN;
					PRINT 'Successful migration of QuoteReadModel Row Number = ' + convert(varchar(100), @Counter);
				END TRY
				BEGIN CATCH
					PRINT 'Error encountered while processing RowNumber =' + convert(varchar(100), @Counter);
					Print 'Error Info => ErrorNumber: ' + Convert(varchar(100),Error_Number()) + ' ErrorMessage: ' + Error_Message();   
					IF(@@TRANCOUNT > 0)
						ROLLBACK TRAN;
				END CATCH
	
			   DROP TABLE #Temp
			   SET @Counter  = @Counter  + 1

		END

		DROP TABLE #QuoteReadModelTemp;

		--Updated records:
		--QuoteDocumentReadModels:
		--New column QuoteOrPolicyTransactionId should be set to same value as QuoteId column.

		PRINT '------------------------------------------------------------------------------------------------------------';


		PRINT 'Updating QuoteDocumentReadModels table - column QuoteOrPolicyTransactionId' ;

		If((Select 1 from INFORMATION_SCHEMA.COLUMNS a
		where a.COLUMN_NAME = 'QuoteOrPolicyTransactionId'
		and a.TABLE_NAME = 'QuoteDocumentReadModels') is null)
		BEGIN 
			ALTER TABLE [dbo].[QuoteDocumentReadModels]
			Add QuoteOrPolicyTransactionId uniqueidentifier  null;
			Print 'Added Column - QuoteOrPolicyTransactionId '
		END

		GO

		UPDATE a
		SET a.QuoteOrPolicyTransactionId = a.QuoteId
		from QuoteDocumentReadModels a;

		GO

		PRINT 'Updating QuoteEmailReadModels';

		If((Select 1 from INFORMATION_SCHEMA.COLUMNS a
		where a.COLUMN_NAME = 'EmailSourceType'
		and a.TABLE_NAME = 'QuoteEmailReadModels') is null)
			BEGIN 


					ALTER TABLE [dbo].[QuoteEmailReadModels]
					ADD EmailType int  null,
					EmailSourceType int  null,
					QuoteId uniqueidentifier  null,
					PolicyId   uniqueidentifier  null,
					PolicyTransactionId   uniqueidentifier  null;

					Print 'Added Columns - EmailType,EmailSourceType,QuoteId,PolicyId,PolicyTransactionId to TABLE: QuoteEmailReadModels '

			END


		SELECT Id,
		   ROW_NUMBER() OVER (
		 ORDER BY [CreationTimeAsTicksSinceEpoch]
		   ) as RowNumber 
		   into #QuoteEmailTemp
		FROM [QuoteEmailReadModels] 

		DECLARE @RowsEmailModelCount INT = (Select count(Id) from #QuoteEmailTemp);

		DECLARE @CounterEmailModel INT;
		SET @CounterEmailModel=1;

		WHILE (@CounterEmailModel <= @RowsEmailModelCount)
		BEGIN

				Declare @CurrentEmailModelId uniqueidentifier = (select Id from  #QuoteEmailTemp where Rownumber = @CounterEmailModel);
				PRINT 'Updating QuoteEMAILReadModels Row Number = ' + convert(varchar(100), @CounterEmailModel) + ' With ID: ' + convert(varchar(100), @CurrentEmailModelId);
		

				SELECT top 1 
				a.Id,
				a.QuoteEmailModelId,
				b.EmailType, 
				b.QuoteId,
				 @CounterEmailModel [RowNumber]
				INTO #TempEmail
				FROM [QuoteEmailReadModels] a join QuoteEmailModels b
				on a.QuoteEmailModelId = b.id
				where a.Id = @CurrentEmailModelId;

				DECLARE @QuoteIDForEmail uniqueidentifier = (Select QuoteId from  #TempEmail); 
				DECLARE @PolicyTransactionId uniqueidentifier; 
				DECLARE @EmailSourceType INT;

				SET @EmailSourceType = (Select top 1
						CASE PolicyIssueTimeInTicksSinceEpoch 
						WHEN 0 then 2
						ELSE 1
						END [SourceType]
						from PolicyReadModels
				where QuoteId = @QuoteIDForEmail);

				DECLARE @PolicyIdForUpdate uniqueidentifier = (SELECT CAST(0x0 AS UNIQUEIDENTIFIER));
				
				SET @PolicyIdForUpdate = @QuoteIDForEmail;

				if(@EmailSourceType = 2)
				BEGIN
					SET @PolicyTransactionId = @QuoteIDForEmail;
					SET @QuoteIDForEmail = (SELECT CAST(0x0 AS UNIQUEIDENTIFIER));
				END
				ELSE
				BEGIN
					SET @EmailSourceType = 1;
					SET @PolicyTransactionId = (SELECT CAST(0x0 AS UNIQUEIDENTIFIER));
				END


				DECLARE @EmailType INT = (SELECT EmailType from #TempEmail);
				BEGIN TRY
					BEGIN TRAN
			  
	
						Update QuoteEmailReadModels
						SET EmailSourceType = @EmailSourceType,
						EmailType = @EmailType,
						QuoteId = @QuoteIDForEmail,
						PolicyId =  @PolicyIdForUpdate,
						PolicyTransactionId = @PolicyTransactionId
						where iD = (Select Id
						from #TempEmail)
		
						COMMIT TRAN;
						PRINT 'Successful migration of QuoteReadModel Row Number = ' + convert(varchar(100), @CounterEmailModel);
			
				END TRY
				BEGIN CATCH
					PRINT 'Error encountered while processing EMAIL Models => RowNumber ='
					+ convert(varchar(100), @CounterEmailModel) +  + ' With ID: ' + convert(varchar(100), @CurrentEmailModelId);

					Print 'Error Info => ErrorNumber: ' + Convert(varchar(100),Error_Number()) + ' ErrorMessage: ' + Error_Message();  
					IF(@@TRANCOUNT > 0)
						ROLLBACK TRAN;
				END CATCH
	
				DROP TABLE #TempEmail

				Set @CounterEmailModel = @CounterEmailModel + 1;
		END

		DROP TABLE #QuoteEmailTemp;

		PRINT 'FINISHED.';


	--	DROP TABLE #QuoteEmailTemp;
	--DROP TABLE #TempEmail
	-- DROP TABLE #Temp
	-- DROP TABLE #QuoteReadModelTemp;