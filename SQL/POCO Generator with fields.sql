use MoneyCalendar

DECLARE @prefix VARCHAR(4) = 'dbo'
DECLARE @tableName VARCHAR(200) = 'MonthlyExpenses'
DECLARE @className VARCHAR(200) = SUBSTRING(@tableName
											, (CASE WHEN LEFT(@tableName, 3) = 'tbl' THEN 4 ELSE 1 END)
											, LEN(@tableName))
IF (RIGHT(@tableName, 1) = 's')
	SET @className = LEFT(@className, LEN(@className)-1)

--DECLARE @className VARCHAR(200) = 'Version'

DECLARE @columnName VARCHAR(200);
DECLARE @camelColumnName VARCHAR(200);
DECLARE @nullMarker VARCHAR(2)
DECLARE @accessors VARCHAR(200);
DECLARE @nullable BIT;
DECLARE @datatype VARCHAR(50);
DECLARE @maxLength INT;
DECLARE @columnID INT;
DECLARE @isPrimaryKey bit;
DECLARE @isIdentity bit;

DECLARE @pocoAttr VARCHAR(50);
DECLARE @pocoType VARCHAR(50);
DECLARE @pocoProperty VARCHAR(200);

--PRINT '#region ' + @className;
--PRINT 'public partial class ' + @className + ' : BindableBase'
--PRINT '{';
PRINT '	#region Source Table Fields';

SET ANSI_WARNINGS OFF

DECLARE column_cursor CURSOR
FOR
	SELECT	ColumnName = c.name,
			c.is_nullable,
			data_type = ty.name,
			c.max_length,
			c.column_id,
			is_primary_key = CAST(ISNULL(MAX(CAST(i.is_primary_key AS INT)), 0) AS BIT),
			c.is_identity

	FROM	sys.tables t 
		INNER JOIN sys.columns c ON c.object_id = t.object_id
		INNER JOIN sys.types ty ON ty.system_type_id = c.system_type_id
								AND ty.name <> 'sysname'
		LEFT JOIN sys.index_columns ic ON ic.object_id = c.object_id
									AND ic.column_id = c.column_id
		LEFT JOIN sys.indexes i ON i.index_id = ic.index_id
								AND i.object_id = ic.object_id

	WHERE	t.name = @tableName

	GROUP BY
			c.name,
			c.is_nullable,
			ty.name,
			c.max_length,
			c.column_id,
			c.is_identity

	ORDER BY
			c.column_id
OPEN column_cursor;

SET ANSI_WARNINGS ON

FETCH NEXT FROM column_cursor INTO @columnName, @nullable, @datatype, @maxLength, @columnID, @isPrimaryKey, @isIdentity

WHILE @@FETCH_STATUS = 0
BEGIN

	-- datatype
	SELECT  @pocoType = CASE @datatype
						WHEN 'int' THEN 'int'
						WHEN 'decimal' THEN 'decimal'
						WHEN 'float' THEN 'double'
						WHEN 'money' THEN 'decimal'
						WHEN 'char' THEN 'string'
						WHEN 'nchar' THEN 'string'
						WHEN 'varchar' THEN 'string'
						WHEN 'nvarchar' THEN 'string'
						WHEN 'uniqueidentifier' THEN 'Guid'
						WHEN 'date' THEN 'DateTime'
						WHEN 'datetime' THEN 'DateTime'
						WHEN 'smalldatetime' THEN 'DateTime'
						WHEN 'bit' THEN 'bool'
						WHEN 'varbinary' THEN 'Byte[]'
						ELSE 'string'
						END;

/*	If (@nullable = 'NO')
	PRINT '[Required]'
if (@pocoType = 'String' and @maxLen <> '-1')
	Print '[MaxLength(' +  convert(varchar(4),@maxLen) + ')]'
*/
	SET @camelColumnName = '_' + LOWER(LEFT(@columnName, 1)) + RIGHT(@columnName, LEN(@columnName)-1)
	SET @accessors = ' { get=>this.' + @camelColumnName + '; set=>this.SetProperty(ref this.'+@camelColumnName +', value);}'
	
	IF (@isPrimaryKey = 1)
		SET @pocoAttr = '[Key] '
	ELSE 
	BEGIN
		IF @pocoType = 'string'
			SET @pocoAttr = '[StringLength(' + CONVERT(varchar, @maxLength) + ')]'
		ELSE IF @datatype = 'money'
			SET @pocoAttr = '[Column(TypeName = "money")]'
		ELSE
			SET @pocoAttr = ''
	END

    IF ( @nullable = 0 )
        SET @nullMarker = ' '
    ELSE
	BEGIN
		IF @pocoType = 'string'
			SET @nullMarker = ' '
		ELSE
			SET @nullMarker = '? '
	END;
					
	PRINT '	private ' + @pocoType + @nullMarker + @camelColumnName + ';';               
	PRINT '	' + @pocoAttr + 'public ' + @pocoType + @nullMarker + @columnName + @accessors;               
		
	FETCH NEXT FROM column_cursor INTO @columnName, @nullable, @datatype, @maxLength, @columnID, @isPrimaryKey, @isIdentity

END;

CLOSE column_cursor;
DEALLOCATE column_cursor;

PRINT '	#endregion';
--PRINT '}';
--PRINT '#endregion';

