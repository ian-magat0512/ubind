IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ReleaseDetails_Id' AND object_id = OBJECT_ID('Assets'))
BEGIN
    DROP index [dbo].[Assets].[IX_ReleaseDetails_Id];
END;

IF EXISTS(SELECT * FROM sys.indexes WHERE name='IX_ReleaseDetails_Id1' AND object_id = OBJECT_ID('Assets'))
BEGIN
    DROP index [dbo].[Assets].[IX_ReleaseDetails_Id1];
END;
