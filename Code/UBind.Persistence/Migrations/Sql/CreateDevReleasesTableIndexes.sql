-- This index speeds up getting a dev release by product ID
-- It includes all necessary properties, so it's a covering index which avoids a key lookup
CREATE NONCLUSTERED INDEX [IX_DevReleases_TenantId_ProductId_Covered]
ON [dbo].[DevReleases] ([TenantId], [ProductId])
INCLUDE ([CreatedTicksSinceEpoch], [ClaimDetails_Id], [QuoteDetails_Id])
