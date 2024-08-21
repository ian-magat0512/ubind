BEGIN TRANSACTION;
		BEGIN TRY 
			UPDATE [ReportReadModels] 
			      SET SourceData = (SELECT TOP 1 JSON_VALUE(EventJson, '$.SourceData') AS 'SourceData'
				  FROM [EventRecordWithGuidIds]
				  WHERE JSON_VALUE(EventJson, '$.SourceData') IS NOT NULL AND 
				        AggregateId = ReportId AND 
						EventJson like '%ReportSourceDataUpdatedEvent%'
				  ORDER BY Sequence DESC)
		END TRY  
BEGIN CATCH 
		ROLLBACK;  
END CATCH;  
COMMIT; 
