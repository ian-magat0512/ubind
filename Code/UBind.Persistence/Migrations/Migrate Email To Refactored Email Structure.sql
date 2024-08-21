BEGIN
	DECLARE @defaultId varchar(255) = '00000000-0000-0000-0000-000000000000';
	DECLARE @EmailCursor CURSOR;
	DECLARE @EmailId uniqueidentifier;
	DECLARE @IntegrationEventId uniqueidentifier;
	DECLARE @EmailType varchar(255);
	DECLARE @EmailTo varchar(255);
	DECLARE @EmailSubject varchar(255);
	DECLARE @EmailTextBody varchar(255);
	DECLARE @EmailRowNumber int = 0, 
		@UserRowNumber int = 0, 
		@QuoteRowNumber int = 0;
    
	SET @EmailCursor = CURSOR FOR
    select 
	--top 1000
	[Id],IIF([EmailType]=0,'Admin',IIF([EmailType]=1,'Customer','User')),[To],[Subject] from [Emails]  

    OPEN @EmailCursor 
    FETCH NEXT FROM @EmailCursor 
    INTO @EmailId, @EmailType, @EmailTo, @EmailSubject;

	/*Loop through each email*/
    WHILE @@FETCH_STATUS = 0
    BEGIN
		/* START */
	   --PRINT N'Row '+CAST(@EmailRowNumber AS varchar(255)) + ' with Email ID - ' + CAST(@EmailId AS varchar(255));  
	   
		PRINT N'--------------------EMAIL ID # '+CAST(@EmailId AS varchar(255))+'--------------------' ;
	   DECLARE @CreationTimeAsTicksSinceEpoch bigint = null, 
		   @Environment varchar(255)=null,
		   @ProductId varchar(255)=null, 
		   @TenantId varchar(255)=null, 
		   @OwnerId varchar(255)=null, 
		   @CustomerId varchar(255)=null;


	   /*GET USER RELATIONSHIP*/
	   declare @UserCustomerOwnerId uniqueidentifier=null, 
		   @UserCustomerId uniqueidentifier=null, 
		   @UserTenantId varchar(255)=null, 
		   @UserEnvironment int=null, 
		   @UserEmailReadModelId uniqueidentifier=null, 
		   @UserEmailUserId uniqueidentifier = null,
		   @UserEmailType int = null, 
		   @UserEmailSourceType int = null, 
		   @UserHasAttachment int = null, 
		   @UserCreationTimeAsTicksSinceEpoch bigint = null

	   select 
			@UserEmailReadModelId = [UserEmailReadModels].[Id],
			@UserEmailUserId = [UserEmailReadModels].UserId,
			@UserEmailType = [EmailType],
			@UserEmailSourceType = [EmailSourceType],
			@UserHasAttachment = [HasAttachment],
			@UserCreationTimeAsTicksSinceEpoch = [UserEmailReadModels].[CreationTimeAsTicksSinceEpoch],
			@UserEnvironment = [UserReadModels].Environment,
			@UserTenantId = [UserReadModels].TenantId,
			@UserCustomerId = [UserReadModels].CustomerId,
			@UserCustomerOwnerId = [CustomerReadModels].OwnerUserId
			from [UserEmailReadModels] 
			left join [UserReadModels] on [UserEmailReadModels].UserId = [UserReadModels].UserId
			left join [CustomerReadModels] on [UserReadModels].CustomerId = [CustomerReadModels].Id
			where [UserEmailReadModels].EmailId = @EmailId
			
		set @Environment = IIF(@UserEnvironment = 0,'None',IIF(@UserEnvironment = 1,'Development',IIF(@UserEnvironment = 2,'Staging',IIF(@UserEnvironment = 3,'Production',NULL))));
		set @TenantId = @UserTenantId
		set @ProductId = NULL
		set @CreationTimeAsTicksSinceEpoch = @UserCreationTimeAsTicksSinceEpoch

		/* IF HAS USER ASSOCIATED */
	   IF @UserEmailUserId IS NOT NULL
	   begin
			set @EmailRowNumber = @EmailRowNumber + 1;
			set @UserRowNumber = @UserRowNumber + 1; 
		
			PRINT N' RECORD # '+CAST(@EmailRowNumber AS varchar(255)) ;
			PRINT N'--User Record #'+CAST(@UserRowNumber AS varchar(255));
			PRINT N'--[UserEmailReadModels].[Id] # '+CAST(@UserEmailReadModelId AS varchar(255));

			-- PRINT N'TO %%%%%%%%%%%%%%%%% '+@EmailTo+ ' SUBJECT %%%%%%%%%%%%%%%%%%%%%'+@EmailSubject;

			-- CREATE ASSOCIATION TO CUSTOMER
			IF @UserCustomerId <> @defaultId AND @UserCustomerId IS NOT NULL
			begin
				set @CustomerId = @UserCustomerId;

				-- CREATE ASSOCIATION TO USER OWNER
				IF @UserCustomerOwnerId <> @defaultId AND @UserCustomerOwnerId IS NOT NULL
				begin
					set	@OwnerId = @UserCustomerOwnerId;
				end
			end

			ELSE
			begin 
				-- CREATE ASSOCIATION TO USER
				PRINT '--HAS RELATIONSHIP TO USER ID #'+CAST(@UserEmailUserId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(),9,@EmailId,1,2,@UserEmailUserId,@UserCreationTimeAsTicksSinceEpoch);
			end

			-- CREATE TAGS FOR USER
			IF LOWER(@EmailSubject) LIKE '%password reset invitation%'  
			begin
				PRINT '--HAS TAG TO "Password Reset"'
				PRINT '--HAS TAG TO "Invitation"';
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID() ,9 ,@EmailId ,1 ,'Password Reset', @UserCreationTimeAsTicksSinceEpoch);
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID() ,9 ,@EmailId ,1 ,'Invitation', @UserCreationTimeAsTicksSinceEpoch);
			end
			IF LOWER(@EmailSubject) LIKE '%activation%'  
			begin
				PRINT '--HAS TAG TO "Account Activation"'
				PRINT '--HAS TAG TO "Invitation"';
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID() ,9 ,@EmailId ,1 ,'Account Activation', @UserCreationTimeAsTicksSinceEpoch);
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID() ,9 ,@EmailId ,1 ,'Invitation', @UserCreationTimeAsTicksSinceEpoch);
			end

	   end


   
	   /*Get Quote Relationships*/
		declare @PolicyTransactionQuoteId varchar(255) = null, 
		@PolicyQuoteId varchar(255) = null, 
		@QuotePolicyTransactionDiscriminator varchar(255) = null,
		@PolicyCustomerEmail varchar(255) = null , 
		@QuoteCustomerEmail varchar(255) = null ,
		@QuoteSearchQuoteId uniqueidentifier=null, 
		@QuoteSearchPolicyId uniqueidentifier=null, 
		@QuoteSearchPolicyTransactionId uniqueidentifier=null, 
		@PolicyProductId varchar(255)=null, 
		@PolicyTenantId varchar(255)=null, 
		@QuoteProductId varchar(255)=null, 
		@QuoteTenantId varchar(255)=null, 
		@QuoteStatus varchar(255)=null,  
		@PolicyEnvironment int=null, 
		@PolicyOwnerUserId uniqueidentifier=null, 
		@PolicyCustomerId uniqueidentifier=null, 
		@QuoteEnvironment int=null, 
		@QuoteOwnerUserId uniqueidentifier=null, 
		@QuoteCustomerId uniqueidentifier=null, 
		@QuoteEmailReadModelId uniqueidentifier=null, 
		@QuotePolicyId uniqueidentifier = null,
		@QuoteQuoteId uniqueidentifier = null,
		@QuotePolicyTransactionId uniqueidentifier = null,
		@QuoteEmailType int = null, 
		@QuoteEmailSourceType int = null, 
		@QuoteHasAttachment int = null, 
		@QuoteCreationTimeAsTicksSinceEpoch bigint = null

		select 
			@QuoteEmailReadModelId =[QuoteEmailReadModels].[Id],
			@QuoteSearchPolicyId = [QuoteEmailReadModels].PolicyId,
			@QuoteSearchQuoteId = [QuoteEmailReadModels].QuoteId ,
			@QuoteSearchPolicyTransactionId = [QuoteEmailReadModels].[PolicyTransactionId],
			@QuotePolicyId = [PolicyReadModels].Id,
			@QuoteQuoteId = [QuoteReadModels].QuoteId ,
			@QuotePolicyTransactionId = [PolicyTransactions].Id,
			@QuotePolicyTransactionDiscriminator = [PolicyTransactions].Discriminator,
			@QuoteEmailType = [EmailType],
			@QuoteEmailSourceType = [EmailSourceType],
			@QuoteHasAttachment = [HasAttachment],
			@QuoteCreationTimeAsTicksSinceEpoch = [QuoteEmailReadModels].[CreationTimeAsTicksSinceEpoch],
			@QuoteCustomerId = [QuoteReadModels].CustomerId,
			@QuoteCustomerEmail = [QuoteReadModels].CustomerEmail,
			@QuoteOwnerUserId = [QuoteReadModels].OwnerUserId,
			@QuoteEnvironment = [QuoteReadModels].Environment,
			@QuoteStatus = [QuoteReadModels].QuoteState,
			@QuoteTenantId = [QuoteReadModels].TenantId,
			@QuoteProductId = [QuoteReadModels].ProductId,
			@PolicyQuoteId = [PolicyReadModels].QuoteId,
			@PolicyCustomerId = [PolicyReadModels].CustomerId,
			@PolicyCustomerEmail = [PolicyReadModels].CustomerEmail,
			@PolicyOwnerUserId = [PolicyReadModels].OwnerUserId,
			@PolicyEnvironment = [PolicyReadModels].Environment,
			@PolicyTenantId = [PolicyReadModels].TenantId,
			@PolicyProductId = [PolicyReadModels].ProductId,
			@PolicyTransactionQuoteId = [PolicyTransactions].QuoteId
			from [QuoteEmailReadModels] 
			left join [QuoteReadModels] on [QuoteReadModels].QuoteId = [QuoteEmailReadModels].QuoteId
			left join [PolicyReadModels] on [PolicyReadModels].Id = [QuoteEmailReadModels].PolicyId
			left join [PolicyTransactions] on [QuoteEmailReadModels].PolicyTransactionId = [PolicyTransactions].Id
			where [QuoteEmailReadModels].QuoteEmailModelId = @EmailId

			
		IF @QuoteSearchPolicyId IS NOT NULL
		begin
			set @EmailRowNumber = @EmailRowNumber + 1;
			set @QuoteRowNumber = @QuoteRowNumber + 1;

			declare @Status varchar(255);
			declare @CustomerEmail varchar(255);

			set @CustomerEmail = IIF(@QuoteCustomerEmail IS NOT NULL, @QuoteCustomerEmail, @PolicyCustomerEmail)
			set @Status = IIF(@QuoteStatus IS NOT NULL, @QuoteStatus, NULL)
			set @TenantId = IIF(@QuoteTenantId <> @defaultId and @QuoteTenantId IS NOT NULL, @QuoteTenantId, @PolicyTenantId)
			set @ProductId = IIF(@QuoteProductId <> @defaultId and @QuoteProductId IS NOT NULL, @QuoteProductId, @PolicyProductId)
			set @OwnerId = IIF(@QuoteOwnerUserId <> @defaultId and @QuoteOwnerUserId IS NOT NULL, @QuoteOwnerUserId, @PolicyOwnerUserId)
			set @CustomerId = IIF(@QuoteCustomerId <> @defaultId and @QuoteCustomerId IS NOT NULL, @QuoteCustomerId, @PolicyCustomerId)
			set @Environment = IIF(@QuoteEnvironment IS NOT NULL, IIF(@QuoteEnvironment = 0,'None',IIF(@QuoteEnvironment = 1,'Development',IIF(@QuoteEnvironment = 2,'Staging',IIF(@QuoteEnvironment = 3,'Production',NULL)))), IIF(@PolicyEnvironment = 0,'None',IIF(@PolicyEnvironment = 1,'Development',IIF(@PolicyEnvironment = 2,'Staging',IIF(@PolicyEnvironment = 3,'Production',NULL)))))
			set @QuotePolicyTransactionDiscriminator = IIF(@QuotePolicyTransactionDiscriminator IS NOT NULL, IIF(@QuotePolicyTransactionDiscriminator = 'NewBusinessTransaction', 'Purchase', IIF(@QuotePolicyTransactionDiscriminator = 'RenewalTransaction', 'Renewal', IIF(@QuotePolicyTransactionDiscriminator = 'MidTermAdjustmentTransaction', 'Adjustment', IIF(@QuotePolicyTransactionDiscriminator = 'CancellationTransaction', 'Cancellation', NULL)))),NULL)
			set @CreationTimeAsTicksSinceEpoch = @QuoteCreationTimeAsTicksSinceEpoch

			-- CHECK IF CUSTOMER EMAIL IS THE RECIPIENT
			IF CHARINDEX(LOWER(@CustomerEmail), LOWER(@EmailTo)) <= 0
			BEGIN
				--	REMOVE ASSOCIATION WITH CUSTOMER
				PRINT '-- CUSTOMER EMAIL IS NOT RECIPIENT OF THE EMAIL';
				set @CustomerId = NULL;
			END

			PRINT N' RECORD # '+CAST(@EmailRowNumber AS varchar(255)) ;
			PRINT N'--Quote Record #'+CAST(@QuoteRowNumber AS varchar(255));
			PRINT '--[QuoteEmailReadModels].Id # '+CAST(@QuoteEmailReadModelId AS varchar(255));
			PRINT 'CUSTOMER EMAIL : '+CAST(@CustomerEmail AS varchar(255));
			PRINT 'POLICY TRANSACTION DESCRIMINATOR - '+CAST(@QuotePolicyTransactionDiscriminator AS varchar(255));

			/*IF HAS TRANSACTION DISCRIMINATOR*/
			IF @QuotePolicyTransactionDiscriminator IS NOT NULL
			begin
				PRINT '--HAS TAG TO TRANSACTION TYPE # '+CAST(@QuotePolicyTransactionDiscriminator AS varchar(255));
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId , 1, @QuotePolicyTransactionDiscriminator, @QuoteCreationTimeAsTicksSinceEpoch);
			end

			/* 
			IF HAS QUOTE ASSOCIATED 
			*/
			IF @QuoteSearchQuoteId <> @defaultId and @QuoteSearchPolicyTransactionId = @defaultId
			begin
				-- PRINT N'     Quote Record #'+CAST(@QuoteRowNumber AS varchar(255))+' with Quote Id - '+CAST(@QuoteQuoteId AS varchar(255)) + '(quote read model record# '+CAST(@QuoteEmailReadModelId AS varchar(255))+')';  

				-- ASSOCIATE TO QUOTE
				PRINT '--HAS RELATIONSHIP TO QUOTE ID #'+CAST(@QuoteSearchQuoteId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 0, @QuoteSearchQuoteId, 3, 9, @EmailId, @QuoteCreationTimeAsTicksSinceEpoch);
				
				-- ASSOCIATE TO POLICY
				PRINT '--HAS RELATIONSHIP TO POLICY ID #'+CAST(@QuoteSearchPolicyId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 1, @QuoteSearchPolicyId, 5, 9, @EmailId, @QuoteCreationTimeAsTicksSinceEpoch);
				
				-- CREATE TAGS FOR QUOTE 
				PRINT '--HAS TAG TO "Quote"';
				 INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId , 1, 'Quote', @QuoteCreationTimeAsTicksSinceEpoch);
			end

			/* 
			IF HAS POLICY ASSOCIATED 
			*/
			IF @QuoteSearchPolicyTransactionId <> @defaultId and @QuoteSearchQuoteId = @defaultId
			begin
				set @QuoteRowNumber = @QuoteRowNumber + 1;
				-- PRINT N'     Policy Record #'+CAST(@QuoteRowNumber AS varchar(255))+' with Policy Transaction Id - '+CAST(@QuotePolicyTransactionId AS varchar(255)) + '(quote read model record# '+CAST(@QuoteEmailReadModelId AS varchar(255))+')';  
				Declare @tmpQuoteId varchar(255) = null;
				set @tmpQuoteId  = IIF(@PolicyTransactionQuoteId <> @defaultId and @PolicyTransactionQuoteId IS NOT NULL, @PolicyTransactionQuoteId, @PolicyQuoteId)

				IF @tmpQuoteId <> @defaultId AND @tmpQuoteId IS NOT NULL
				begin
					-- ASSOCIATE TO QUOTE
					PRINT '--HAS RELATIONSHIP TO QUOTE ID #'+CAST(@tmpQuoteId AS varchar(255));
					INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 0, @tmpQuoteId, 3, 9, @EmailId, @QuoteCreationTimeAsTicksSinceEpoch);
				end

				-- ASSOCIATE TO POLICY TRANSACTION
				PRINT '--HAS RELATIONSHIP TO POLICY TRANSACTION ID #'+CAST(@QuoteSearchPolicyTransactionId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 4, @QuoteSearchPolicyTransactionId, 6, 9, @EmailId, @QuoteCreationTimeAsTicksSinceEpoch);
				
				-- ASSOCIATE TO POLICY
				PRINT '--HAS RELATIONSHIP TO POLICY ID #'+CAST(@QuoteSearchPolicyId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 1, @QuoteSearchPolicyId, 5, 9, @EmailId, @QuoteCreationTimeAsTicksSinceEpoch);

				-- CREATE TAGS FOR QUOTE 
				PRINT '--HAS TAG TO "Policy"';
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId , 1, 'Policy', @QuoteCreationTimeAsTicksSinceEpoch);
			end
		end

		IF @UserEmailUserId IS NULL AND @QuoteSearchPolicyId IS NULL
		begin
			set @EmailRowNumber = @EmailRowNumber + 1;
		end

		IF @UserEmailUserId IS NOT NULL OR @QuoteSearchPolicyId IS NOT NULL
		begin
			PRINT N'EMAIL TO : '+@EmailTo; 
			PRINT N'EMAIL SUBJECT : '+@EmailSubject;

			-- UPDATE EMAIL WITH THE TENANT ID AND PRODUCT ID
			PRINT '--UPDATE EMAIL WITH TENANT AND PRODUCT '+CAST(@TenantId AS varchar(255))+'/'+CAST(@ProductId AS varchar(255));
			UPDATE [Emails] SET [TenantId] = @TenantId, [ProductId] = @ProductId where [Id] = @EmailId
		
			-- ASSOCIATE TO OWNER USER
			IF @OwnerId <> @defaultId AND @OwnerId IS NOT NULL
			begin
				PRINT '--HAS RELATIONSHIP TO OWNER USER ID #'+CAST(@OwnerId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId, 0, 2, @OwnerId, @CreationTimeAsTicksSinceEpoch);
			end

			-- ASSOCIATE TO Customer USER
			IF @CustomerId <> @defaultId AND @CustomerId IS NOT NULL
			begin
				PRINT '--HAS RELATIONSHIP TO CUSTOMER ID #'+CAST(@CustomerId AS varchar(255));
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9,@EmailId, 1, 3, @CustomerId, @CreationTimeAsTicksSinceEpoch);
				INSERT INTO [Relationships] (Id,FromEntityType,FromEntityId,Type,ToEntityType,ToEntityId,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 3, @CustomerId, 1, 9, @EmailId, @CreationTimeAsTicksSinceEpoch);
			end

			-- CREATE TAGS FOR ENVIRONMENT
			IF @Environment IS NOT NULL
			begin
				PRINT '--HAS TAG TO ENVIRONMENT #'+CAST(@Environment AS varchar(255));
				INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId ,0, @Environment, @CreationTimeAsTicksSinceEpoch);
			end

			-- CREATE TAGS FOR EMAIL TYPE
			PRINT '--HAS TAG TO EMAILTYPE #'+CAST(@EmailType AS varchar(255));
			INSERT INTO [Tags] (Id,EntityType,EntityId,TagType,Value,CreationTimeInTicksSinceEpoch) VALUES (NEWID(), 9, @EmailId, 2, @EmailType, @CreationTimeAsTicksSinceEpoch);

		end

		/* END */
      FETCH NEXT FROM @EmailCursor 
      INTO @EmailId, @EmailType, @EmailTo, @EmailSubject;
    END; 

    CLOSE @EmailCursor ;
    DEALLOCATE @EmailCursor;
END;



