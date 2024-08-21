UPDATE QUOTES
            SET LastUpdatedByUserTicksSinceEpoch = LastUpdatedTicksSinceEpoch
            WHERE ISNULL(LastUpdatedByUserTicksSinceEpoch, '') =''
			
IF NOT EXISTS(
						SELECT * 
						FROM sys.indexes 
						WHERE name='IX_Quotes_PolicyReadModels_LastUpdated' AND object_id = OBJECT_ID('Quotes'))
						BEGIN
								CREATE NONCLUSTERED INDEX [IX_Quotes_PolicyReadModels_LastUpdated] ON [dbo].[Quotes]
								(
									[LastUpdatedTicksSinceEpoch] ASC
								)
								INCLUDE ([PolicyId],
									[Type],
									[QuoteState],
									[LatestFormData],
									[QuoteNumber],
									[CreationTimeInTicksSinceEpoch],
									[LastUpdatedByUserTicksSinceEpoch],
									[ExpiryTimeAsTicksSinceEpoch],
									[IsDiscarded]) 
								WHERE ([QuoteNumber] IS NOT NULL AND [IsDiscarded]=(0))
						END