﻿CREATE VIEW [{0}].ADDRESS_VIEW
AS
SELECT AD.ADDRESS_DETAIL_PID AS ADDRESS_DETAIL_PID
	,AD.STREET_LOCALITY_PID AS STREET_LOCALITY_PID
	,AD.LOCALITY_PID AS LOCALITY_PID
	,AD.BUILDING_NAME AS BUILDING_NAME
	,AD.LOT_NUMBER_PREFIX AS LOT_NUMBER_PREFIX
	,AD.LOT_NUMBER AS LOT_NUMBER
	,AD.LOT_NUMBER_SUFFIX AS LOT_NUMBER_SUFFIX
	,FTA.NAME AS FLAT_TYPE
	,AD.FLAT_NUMBER_PREFIX AS FLAT_NUMBER_PREFIX
	,AD.FLAT_NUMBER AS FLAT_NUMBER
	,AD.FLAT_NUMBER_SUFFIX AS FLAT_NUMBER_SUFFIX
	,LTA.NAME AS LEVEL_TYPE
	,AD.LEVEL_NUMBER_PREFIX AS LEVEL_NUMBER_PREFIX
	,AD.LEVEL_NUMBER AS LEVEL_NUMBER
	,AD.LEVEL_NUMBER_SUFFIX AS LEVEL_NUMBER_SUFFIX
	,AD.NUMBER_FIRST_PREFIX AS NUMBER_FIRST_PREFIX
	,AD.NUMBER_FIRST AS NUMBER_FIRST
	,AD.NUMBER_FIRST_SUFFIX AS NUMBER_FIRST_SUFFIX
	,AD.NUMBER_LAST_PREFIX AS NUMBER_LAST_PREFIX
	,AD.NUMBER_LAST AS NUMBER_LAST
	,AD.NUMBER_LAST_SUFFIX AS NUMBER_LAST_SUFFIX
	,SL.STREET_NAME AS STREET_NAME
	,SL.STREET_CLASS_CODE AS STREET_CLASS_CODE
	,SCA.NAME AS STREET_CLASS_TYPE
	,SL.STREET_TYPE_CODE AS STREET_TYPE_CODE
	,SL.STREET_SUFFIX_CODE AS STREET_SUFFIX_CODE
	,SSA.NAME AS STREET_SUFFIX_TYPE
	,L.LOCALITY_NAME AS LOCALITY_NAME
	,ST.STATE_ABBREVIATION AS STATE_ABBREVIATION
	,AD.POSTCODE AS POSTCODE
	,ADG.LATITUDE AS LATITUDE
	,ADG.LONGITUDE AS LONGITUDE
	,GTA.NAME AS GEOCODE_TYPE
	,AD.CONFIDENCE AS CONFIDENCE
	,AD.ALIAS_PRINCIPAL AS ALIAS_PRINCIPAL
	,AD.PRIMARY_SECONDARY AS PRIMARY_SECONDARY
	,AD.LEGAL_PARCEL_ID AS LEGAL_PARCEL_ID
	,AD.DATE_CREATED AS DATE_CREATED
	,STA.NAME AS STREET_TYPE_NAME
FROM [{0}].ADDRESS_DETAIL_View AD
	LEFT JOIN [{0}].FLAT_TYPE_AUT_View FTA ON AD.FLAT_TYPE_CODE = FTA.CODE
	LEFT JOIN [{0}].LEVEL_TYPE_AUT_View LTA ON AD.LEVEL_TYPE_CODE = LTA.CODE
	LEFT JOIN [{0}].STREET_LOCALITY_View SL ON AD.STREET_LOCALITY_PID = SL.STREET_LOCALITY_PID
	LEFT JOIN [{0}].STREET_SUFFIX_AUT_View SSA ON SL.STREET_SUFFIX_CODE = SSA.CODE
	LEFT JOIN [{0}].STREET_CLASS_AUT_View SCA ON SL.STREET_CLASS_CODE = SCA.CODE
	LEFT JOIN [{0}].STREET_TYPE_AUT_View STA ON SL.STREET_TYPE_CODE = STA.CODE
	LEFT JOIN [{0}].LOCALITY_View L ON AD.LOCALITY_PID = L.LOCALITY_PID
	LEFT JOIN [{0}].ADDRESS_DEFAULT_GEOCODE_View ADG ON AD.ADDRESS_DETAIL_PID = ADG.ADDRESS_DETAIL_PID
	LEFT JOIN [{0}].GEOCODE_TYPE_AUT_View GTA ON ADG.GEOCODE_TYPE_CODE = GTA.CODE
	LEFT JOIN [{0}].GEOCODED_LEVEL_TYPE_AUT_View GLTA ON AD.LEVEL_GEOCODED_CODE = GLTA.CODE
	JOIN [{0}].STATE_View ST ON L.STATE_PID = ST.STATE_PID
