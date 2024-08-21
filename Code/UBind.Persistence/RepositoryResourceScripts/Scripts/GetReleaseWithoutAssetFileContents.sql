-- Fetch Release with QuoteDetails and ClaimDetails
SELECT TOP(1) r.*, qd.*, cd.*
FROM Releases r
LEFT JOIN ReleaseDetails qd ON r.QuoteDetails_Id = qd.Id
LEFT JOIN ReleaseDetails cd ON r.ClaimDetails_Id = cd.Id
WHERE r.TenantId = @TenantId AND r.Id = @ProductReleaseId;

-- Fetch Assets for QuoteDetails and ClaimDetails
WITH ReleaseDetailsIds AS (
    SELECT QuoteDetails_Id AS Id
    FROM Releases
    WHERE TenantId = @TenantId AND Id = @ProductReleaseId
    UNION
    SELECT ClaimDetails_Id
    FROM Releases
    WHERE TenantId = @TenantId AND Id = @ProductReleaseId
)
SELECT a.*, rd.Id AS ReleaseDetailsId, 
    CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic
FROM Assets a
LEFT JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
JOIN ReleaseDetailsIds rdi ON rd.Id = rdi.Id;