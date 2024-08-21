SELECT p.Id
    ,p.Environment
    ,p.IsTestData
    ,p.QuoteId
    ,p.OwnerUserId
    ,p.OwnerPersonId
    ,p.OwnerFullName
    ,p.CustomerId
    ,p.CustomerPersonId
    ,p.CustomerFullName
    ,p.CustomerPreferredName
    ,p.CustomerEmail
    ,p.CustomerAlternativeEmail
    ,p.CustomerMobilePhone
    ,p.CustomerHomePhone
    ,p.CustomerWorkPhone
    ,p.IssuedTicksSinceEpoch
    ,p.PolicyNumber
    ,p.InceptionDateTime
    ,p.ExpiryDateTime
    ,p.InceptionTicksSinceEpoch
    ,p.ExpiryTicksSinceEpoch
    ,p.CancellationEffectiveTicksSinceEpoch
    ,p.IsDiscarded
    ,p.SerializedCalculationResult
    ,p.LastModifiedTicksSinceEpoch
    ,p.LastModifiedByUserTicksSinceEpoch
    ,p.CreatedTicksSinceEpoch
    ,p.CancellationEffectiveDateTime
    ,p.OrganisationId
    ,p.TenantId
    ,p.ProductId
    ,p.PolicyTitle
    ,p.PolicyState
    ,p.LatestRenewalEffectiveTicksSinceEpoch
    ,p.RetroactiveTicksSinceEpoch
    ,pt.Id PolicyTransactionId
    ,pt.Id
	,pt.PolicyId
	,pt.CreatedTicksSinceEpoch
	,pt.QuoteId
	,pt.QuoteNumber
	,pt.PolicyData_FormData
	,pt.PolicyData_SerializedCalculationResult
	,pt.EffectiveTicksSinceEpoch
	,pt.EffectiveDateTime
	,pt.ExpiryTicksSinceEpoch
	,pt.ExpiryDateTime
	,pt.Discriminator
	,pt.Environment
	,pt.CustomerId
	,pt.OwnerUserId
	,pt.OrganisationId
	,pt.IsTestData
	,pt.LastModifiedTicksSinceEpoch
	,pt.TenantId
	,pt.ProductId
FROM 
	PolicyReadModels p
	INNER JOIN PolicyTransactions pt ON p.Id = pt.PolicyId
WHERE
        (p.LastModifiedTicksSinceEpoch > @LastModifiedTicksSinceEpoch OR p.LastModifiedByUserTicksSinceEpoch > @LastModifiedTicksSinceEpoch)
    AND p.InceptionTicksSinceEpoch > 0
    AND p.TenantId = @TenantId
    AND p.Environment = @Environment
    AND p.PolicyNumber IS NOT NULL
ORDER BY p.LastModifiedTicksSinceEpoch
OFFSET @Offset ROWS
FETCH NEXT @Next ROWS ONLY