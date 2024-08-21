-- set Id
CREATE TRIGGER StreetAddressReadModelSetId
ON StreetAddressReadModels
AFTER INSERT   
AS
	UPDATE dbo.StreetAddressReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN StreetAddressReadModels model ON model.FieldId = i.FieldId
GO

CREATE TRIGGER EmailAddressReadModelSetId
ON EmailAddressReadModels
AFTER INSERT   
AS
	UPDATE dbo.EmailAddressReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN EmailAddressReadModels model ON model.FieldId = i.FieldId
GO

CREATE TRIGGER MessengerIdReadModelSetId
ON MessengerIdReadModels
AFTER INSERT   
AS
	UPDATE dbo.MessengerIdReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN MessengerIdReadModels model ON model.FieldId = i.FieldId
GO

CREATE TRIGGER PhoneNumberReadModelSetId
ON PhoneNumberReadModels
AFTER INSERT   
AS
	UPDATE dbo.PhoneNumberReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN PhoneNumberReadModels model ON model.FieldId = i.FieldId
GO

CREATE TRIGGER SocialMediaIdReadModelSetId
ON SocialMediaIdReadModels
AFTER INSERT   
AS
	UPDATE dbo.SocialMediaIdReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN SocialMediaIdReadModels model ON model.FieldId = i.FieldId
GO

CREATE TRIGGER WebsiteAddressReadModelSetId
ON WebsiteAddressReadModels
AFTER INSERT   
AS
	UPDATE dbo.WebsiteAddressReadModels
	SET Id = i.FieldId
	FROM inserted i
	JOIN WebsiteAddressReadModels model ON model.FieldId = i.FieldId
GO