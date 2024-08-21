--Purpose of this script is to modify the release records with the correct Minor and Major Number values

IF OBJECT_ID('tempdb..#ProductIdTenantPair') IS NOT NULL
DROP TABLE #ProductIdTenantPair
IF OBJECT_ID('tempdb..#releases') IS NOT NULL
DROP TABLE #releases

  SELECT ProductId, TenantId into #ProductIdTenantPair
FROM [Releases]
GROUP BY ProductId, TenantId

--check if has content
if exists (select * from #ProductIdTenantPair)
begin
	
	DECLARE @productId VARCHAR(50)
	DECLARE @tenantId VARCHAR(50)
	Declare @pairLoopCount int = (select count(*) from #ProductIdTenantPair)
	Declare @i int = 1

	--loop through each #ProductIdTenantPair
	while(@i <= @pairLoopCount)
	begin 
		--get productId 
		 select @productId = (select ProductId from (SELECT 
							  ROW_NUMBER() OVER(ORDER BY TenantId ASC) 
							  as [Row], ProductId, TenantId
							  from #ProductIdTenantPair) as j where j.Row = @i)
		--get tenantId
		 select @tenantId = (select TenantId from (SELECT 
							  ROW_NUMBER() OVER(ORDER BY TenantId ASC) 
							  as [Row], ProductId, TenantId
							  from #ProductIdTenantPair) as j where j.Row = @i)

		--selecting releases for the product and tenant
		select ROW_NUMBER() OVER(ORDER BY CreationTimeInTicksSinceEpoch ASC) as [Row], * into #releases from [dbo].[Releases] where ProductId = @productId and TenantId = @tenantId order by CreationTimeInTicksSinceEpoch
		DECLARE @majorCount INT = 0, @minorCount INT = 0
		
		Declare @releasesCount int = (select count(*) from #releases) 
		Declare @o int = 1

			--loop through each #release
			while(@o <= @releasesCount)
			begin 
				Declare @type varchar(30) = (select [Type] from #releases where Row = @o)
				Declare @id uniqueidentifier  = (select [Id] from #releases where Row = @o)
				--setup correct major and minor count
				--if its major
				if (@type = 1 )
				begin
					set @majorCount = @majorCount + 1
					set @minorCount = 0
				end 
				-- if its minor
				if (@type = 0 )
				begin
					set @minorCount = @minorCount + 1
				end 

				--update database with correct values
				update [Releases] set MinorNumber = @minorCount, Number = @majorCount where Id = @id

				set @o = @o + 1
			end

		IF OBJECT_ID('tempdb..#releases') IS NOT NULL
		drop table #releases

		set @i = @i + 1
	end
end