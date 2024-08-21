-- set Id
IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'StreetAddressReadModelSetId')
BEGIN
    DROP TRIGGER StreetAddressReadModelSetId
END

IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'EmailAddressReadModelSetId')
BEGIN
	DROP TRIGGER EmailAddressReadModelSetId
END

IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'MessengerIdReadModelSetId')
BEGIN
	DROP TRIGGER MessengerIdReadModelSetId
END

IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'PhoneNumberReadModelSetId')
BEGIN
	DROP TRIGGER PhoneNumberReadModelSetId
END

IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'SocialMediaIdReadModelSetId')
BEGIN
	DROP TRIGGER SocialMediaIdReadModelSetId
END

IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'WebsiteAddressReadModelSetId')
BEGIN
	DROP TRIGGER WebsiteAddressReadModelSetId
END