DECLARE @ProductCursor CURSOR;
DECLARE @TenantID varchar(255);
DECLARE @ProductID varchar(255);
DECLARE @ProductFeatureId AS UNIQUEIDENTIFIER
DECLARE @CreationTimeAsTicksSinceEpoch bigint = (SELECT DATEDIFF(SECOND,'1970-01-01', GETUTCDATE()));
DECLARE @PurchaseType Int = 0;
DECLARE @AdjustmentType Int = 1;
DECLARE @RenewType Int = 2;
DECLARE @CanncellationType Int = 3;

 SET @ProductCursor = CURSOR FOR
 select distinct A.Id as Tenant_Id, B.Product_Id from Tenants as A inner join ProductDetails as B on A.Id = B.Product_TenantId group by A.ID , B.Product_Id;

 OPEN @ProductCursor
 FETCH NEXT FROM @ProductCursor INTO @TenantID, @ProductID;

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT N'Tenant ID '+CAST(@TenantID AS varchar(255)) ;
	PRINT N'Product ID '+CAST(@ProductID AS varchar(255)) ;
	insert into ProductFeatureSettings(Id, TenantId, ProductId, IsPurchaseEnabled, IsRenewalEnabled, IsAdjustmentEnabled, IsCancellationEnabled, CreationTimeInTicksSinceEpoch) 
	values(NEWID(), @TenantID, @ProductID, 'true', 'true', 'true', 'true', @CreationTimeAsTicksSinceEpoch);

	FETCH NEXT FROM @ProductCursor INTO @TenantID, @ProductID;
END;

