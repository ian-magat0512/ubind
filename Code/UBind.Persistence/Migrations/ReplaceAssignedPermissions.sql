-- Replace AdjustPolicies, RenewPolicies and CancelPolicies with ManagePolicies
UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'AdjustPolicies','ManagePolicies')
FROM Roles
WHERE SerializedPermissions LIKE '%AdjustPolicies%'

UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'RenewPolicies','ManagePolicies')
FROM Roles
WHERE SerializedPermissions LIKE '%RenewPolicies%'

UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'CancelPolicies','ManagePolicies')
FROM Roles
WHERE SerializedPermissions LIKE '%CancelPolicies%'

-- Replace AssignClaimNumber and AssociateClaim with ManageClaims
UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'AssignClaimNumbers','ManageClaims')
FROM Roles
WHERE SerializedPermissions LIKE '%AssignClaimNumbers%'

UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'AssociateClaims','ManageClaims')
FROM Roles
WHERE SerializedPermissions LIKE '%AssociateClaims%'

UPDATE Roles
SET SerializedPermissions = REPLACE(SerializedPermissions, 'ManageClientAdminUsers','ManageTenantAdminUsers')
FROM Roles
WHERE SerializedPermissions LIKE '%ManageClientAdminUsers%'

-- Remove duplicates in SerializedPermissions column caused by one-to-one replacement of AdjustPolicies, RenewPolicies and CancelPolicies into ManagePolicies
-- Based on: https://stackoverflow.com/questions/49361543/sql-remove-duplicates-from-comma-separated-string
;WITH Splitted AS
(
	SELECT SerializedPermissions 
			,CAST('<x>' + REPLACE(SerializedPermissions,',','</x><x>') + '</x>' AS XML) AS TheParts
	FROM Roles 
	WHERE SerializedPermissions LIKE '%ManagePolicies%' OR SerializedPermissions LIKE '%ManageClaims%' OR SerializedPermissions LIKE '%ManageTenantAdminUsers%'
),
duplicatesRemoved_cte AS (
	SELECT SerializedPermissions, STUFF(
				(TheParts.query
				('
				for $x in distinct-values(/x/text())
				return <x>{concat(",", $x)}</x>
				').value('.','nvarchar(max)')),1,1,'') AS SerializedPermissionsUnique
	FROM Splitted
)

UPDATE duplicatesRemoved_cte
SET SerializedPermissions = SerializedPermissionsUnique;