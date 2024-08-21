-- set Id
IF EXISTS (SELECT 1 FROM sys.triggers 
           WHERE Name = 'SystemEventsInsertSetPayloadJson')
BEGIN
    DROP TRIGGER SystemEventsInsertSetPayloadJson
END