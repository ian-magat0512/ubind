-- This index speeds up lookups listing the assets or files associated with the QuoteDetails of a release
CREATE NONCLUSTERED INDEX [IX_Assets_ReleaseDetails_Id]
ON [dbo].[Assets] ([ReleaseDetails_Id])
INCLUDE (
[Name],[CreatedTicksSinceEpoch],[FileContentId],[ReleaseDetails_Id1],[FileModifiedTicksSinceEpoch])

-- This index speeds up lookups listing the assets or files associated with the ClaimDetails of a release
CREATE NONCLUSTERED INDEX [IX_Assets_ReleaseDetails_Id1]
ON [dbo].[Assets] ([ReleaseDetails_Id1])
INCLUDE (
[Name],[CreatedTicksSinceEpoch],[FileContentId],[ReleaseDetails_Id],[FileModifiedTicksSinceEpoch])
