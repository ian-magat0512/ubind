-- Fetch DevRelease with QuoteDetails and ClaimDetails
SELECT TOP(1) dr.*, qd.*, cd.*
FROM DevReleases dr
LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
WHERE dr.TenantId = @TenantId AND dr.Id = @ReleaseId;

-- Fetch all Assets associated with the ReleaseDetails of a specific Release
WITH ReleaseDetailsIds AS (
    SELECT QuoteDetails_Id AS Id
    FROM DevReleases
    WHERE TenantId = @TenantId AND Id = @ReleaseId
    UNION
    SELECT ClaimDetails_Id
    FROM DevReleases
    WHERE TenantId = @TenantId AND Id = @ReleaseId
)
SELECT a.*, fc.*, rd.Id AS ReleaseDetailsId, 
    CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic
FROM Assets a
LEFT JOIN FileContents fc ON a.FileContentId = fc.Id
JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
JOIN ReleaseDetailsIds rdi ON rd.Id = rdi.Id;