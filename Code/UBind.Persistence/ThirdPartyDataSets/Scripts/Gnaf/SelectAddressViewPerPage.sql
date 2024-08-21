 Select  
IIF(FLAT_NUMBER is NULL, '', 
 IIF(FLAT_TYPE is NULL,'Unit ',FLAT_TYPE) + ' ' + isnull(FLAT_NUMBER_PREFIX,'') + isnull(cast(FLAT_NUMBER AS varchar),'') + isnull(FLAT_NUMBER_SUFFIX,'') + ', '
)  
+
IIF(LEVEL_NUMBER is NULL, '', 
 IIF(LEVEL_TYPE is NULL,'Level ',LEVEL_TYPE) + ' ' + isnull(LEVEL_NUMBER_PREFIX ,'') + isnull(cast(LEVEL_NUMBER  AS varchar),'') + isnull(LEVEL_NUMBER_SUFFIX ,'') + ', '
)
+
IIF(NUMBER_FIRST is NULL, IIF(LOT_NUMBER is null,  '' , 'Lot ' + isnull(LOT_NUMBER_PREFIX,'') + isnull(cast(LOT_NUMBER as varchar),'') + isnull(LOT_NUMBER_SUFFIX,'') + ' ') , 
 (isnull(NUMBER_FIRST_PREFIX,'') + isnull(cast(NUMBER_FIRST as varchar),'') + isnull(NUMBER_FIRST_SUFFIX,'') + IIF(NUMBER_LAST is null, '' , '-' + isnull(NUMBER_LAST_PREFIX,'') +isnull(cast(NUMBER_LAST as varchar),'')+ isnull(NUMBER_LAST_SUFFIX,'')) + ' ')
)
+
STREET_NAME + ' ' + isnull(STREET_TYPE_CODE,'') + ', ' + LOCALITY_NAME + ' ' + STATE_ABBREVIATION + ' ' + POSTCODE
as FullAddress, 
ADDRESS_DETAIL_PID As Id,
IIF(NUMBER_FIRST is NULL, '', 
 (isnull(NUMBER_FIRST_PREFIX,'') + isnull(cast(NUMBER_FIRST as varchar),'') + isnull(NUMBER_FIRST_SUFFIX,'') + IIF(NUMBER_LAST is null, '' , '-' + isnull(NUMBER_LAST_PREFIX,'') +isnull(cast(NUMBER_LAST as varchar),'')+ isnull(NUMBER_LAST_SUFFIX,'')) + ' ')
) as NumberFirst,
FLAT_TYPE as FlatType,
BUILDING_NAME as BuildingName,
IIF(NUMBER_FIRST is NOT NULL,'',IIF(LOT_NUMBER is null,  '' , 'Lot ' + isnull(LOT_NUMBER_PREFIX,'') + isnull(cast(LOT_NUMBER as varchar),'') + isnull(LOT_NUMBER_SUFFIX,'')))  as LotNumber,
IIF(FLAT_NUMBER is NULL, '', 
 IIF(FLAT_TYPE is NULL,'Unit ',FLAT_TYPE) + ' ' + isnull(FLAT_NUMBER_PREFIX,'') + isnull(cast(FLAT_NUMBER AS varchar),'') + isnull(FLAT_NUMBER_SUFFIX,'') 
)  as FlatNumber,
LEVEL_TYPE as LevelType,
IIF(LEVEL_NUMBER is NULL, '', 
 IIF(LEVEL_TYPE is NULL,'Level ',LEVEL_TYPE) + ' ' + isnull(LEVEL_NUMBER_PREFIX ,'') + isnull(cast(LEVEL_NUMBER  AS varchar),'') + isnull(LEVEL_NUMBER_SUFFIX ,'')
) as LevelNumber,
STREET_NAME as StreetName,
STREET_TYPE_CODE as StreetTypeCode,
STREET_TYPE_NAME as StreetTypeShortName,
LOCALITY_NAME as LocalityName,
STATE_ABBREVIATION as StateAbbreviation,
POSTCODE as PostCode,
LATITUDE as Latitude,
LONGITUDE as Longitude
from [{0}].ADDRESS_VIEW nolock
WHERE CONFIDENCE > -1
order by address_detail_pid 
OFFSET {1} * ({2}-1) ROWS
FETCH NEXT {1} ROWS ONLY 