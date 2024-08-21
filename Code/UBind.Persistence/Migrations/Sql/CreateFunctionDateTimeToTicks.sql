CREATE OR ALTER FUNCTION dbo.ToTicks ( @DateTime datetime2 )
  RETURNS bigint
AS
BEGIN 
    DECLARE @Ticks bigint = DATEDIFF_BIG( microsecond, '00010101', @DateTime ) * 10 +
           ( DATEPART( NANOSECOND, @DateTime ) % 1000 ) / 100;
    RETURN @Ticks - 621355968000000000;
END