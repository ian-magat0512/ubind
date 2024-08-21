SELECT 
	PRM.Id, 
    PRM.PolicyTitle,
    PRM.TenantId, 
    PRM.ProductId, 
    PRM.OrganisationId, 
    PRM.IsTestData, 
    PRM.QuoteId, 
    PRM.OwnerUserId, 
    PRM.CustomerId, 
    PRM.CustomerFullName, 
    PRM.CustomerPreferredName, 
    PRM.IssuedTicksSinceEpoch, 
    PRM.PolicyNumber, 
    PRM.InceptionDateTime AS InceptionDateTimeColumn, 
    PRM.ExpiryDateTime AS ExpiryDateTimeColumn, 
    PRM.InceptionTicksSinceEpoch, 
    PRM.ExpiryTicksSinceEpoch, 
    PRM.CancellationEffectiveTicksSinceEpoch, 
/*
    PRM.SerializedCalculationResult, 
*/
    PRM.LastModifiedTicksSinceEpoch, 
    PRM.CreatedTicksSinceEpoch, 
    P.Name
FROM   
	PolicyReadModels PRM       
	INNER JOIN (        
		SELECT      
	        PD.Name,       
	        P.Id,     
	        P.TenantId        
        FROM        
	        Products P
	        INNER JOIN ProductDetails PD ON P.TenantId = PD.Product_TenantId AND P.Id = PD.Product_Id
        WHERE       
	        P.TenantId = @TenantId      
        GROUP BY        
	        PD.Name,       
	        P.Id,     
	        P.TenantId  
	) P ON PRM.ProductId = P.Id AND PRM.TenantId = P.TenantId        
WHERE   
	PRM.TenantId = @TenantId  
	AND PRM.Environment = @Environment     
	AND PRM.PolicyNumber IS NOT NULL