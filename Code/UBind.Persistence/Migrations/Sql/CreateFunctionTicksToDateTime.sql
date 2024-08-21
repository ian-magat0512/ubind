CREATE OR ALTER FUNCTION dbo.ToDateTime2 ( @Ticks bigint )
  RETURNS datetime2
AS
BEGIN
    SET @Ticks += 621355968000000000;
    DECLARE @DateTime datetime2 = '00010101';
    SET @DateTime = DATEADD( DAY, @Ticks / 864000000000, @DateTime );
    SET @DateTime = DATEADD( SECOND, ( @Ticks % 864000000000) / 10000000, @DateTime );
    RETURN DATEADD( NANOSECOND, ( @Ticks % 10000000 ) * 100, @DateTime );
END