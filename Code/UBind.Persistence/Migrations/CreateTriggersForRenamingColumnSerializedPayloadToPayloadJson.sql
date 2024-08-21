-- set
CREATE TRIGGER SystemEventsInsertSetPayloadJson
ON SystemEvents
AFTER INSERT   
AS
BEGIN
	SET NOCOUNT ON;

	-- update if has SerializedPayload
	BEGIN
		UPDATE dbo.SystemEvents
		SET PayloadJson = i.SerializedPayload
		FROM INSERTED i
		where dbo.SystemEvents.Id = i.Id and i.SerializedPayload is not null
	END

	-- update if has SerializedPayload
	BEGIN
		UPDATE dbo.SystemEvents
		SET SerializedPayload = i.PayloadJson
		FROM INSERTED i
		where dbo.SystemEvents.Id = i.Id and i.PayloadJson is not null
	END
END