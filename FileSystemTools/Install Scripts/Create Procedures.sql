--------------------------------------------------------------------------------------------------
--Create Stored Procedures
--------------------------------------------------------------------------------------------------
SET ansi_nulls ON

GO

SET quoted_identifier OFF

GO
CREATE PROCEDURE [dbo].[InsertFile]
	@oFileName           NVARCHAR(255),
	@oFilePath           NVARCHAR(255),
	@oFileExtention      NVARCHAR(20),
	@oFileSize           BIGINT,
	@oFileCreateDate     DATETIME,
	@oFileLastWriteDate  DATETIME,
	@oFileLastAccessDate DATETIME,
	@SQLStorageType      TINYINT,
	@FileData            VARBINARY(MAX),
	@ReturnFileId BIGINT OUTPUT
AS
BEGIN
	SET nocount ON

	DECLARE @FileId AS BIGINT

	INSERT INTO [dbo].[SQLFileStore]
				(oFileName,
				 oFilePath,
				 oFileExtention,
				 oFileSize,
				 oFileCreateDate,
				 oFileLastWriteDate,
				 oFileLastAccessDate,
				 SQLStorageType)
	VALUES      ( @oFileName,
				  @oFilePath,
				  @oFileExtention,
				  @oFileSize,
				  @oFileCreateDate,
				  @oFileLastWriteDate,
				  @oFileLastAccessDate,
				  @SQLStorageType)

	SELECT
		@FileId = Scope_identity()
	SELECT 
		@ReturnFileId = Scope_identity()
		
	INSERT INTO [dbo].[SQLFileStoreData]
				([FileId],
				 [FileData])
	VALUES      (@FileId,
				 @FileData)

	IF ( @SQLStorageType = 2 )
		OR ( @SQLStorageType = 4 )
	BEGIN
		INSERT INTO [dbo].[SQLFileStoreConfigItems]
					([FileId],
					 [ConfigItem])
		VALUES      (@FileId,
					 'FileKey')
	END
END
GO

CREATE PROCEDURE [dbo].[SavePassphrase]
	@Password AS NVARCHAR(255)
AS
BEGIN
	SET nocount ON

	INSERT INTO SQLFileStoreConfig
				(ConfigItem,
				 ConfigValue)
	VALUES      ('FileKey',
				 @Password)
END

GO

CREATE PROCEDURE [dbo].[RetrievePassPhrase]
AS
begin
  Set nocount on
	select ConfigValue from SQLFileStoreConfig where ConfigItem = 'FileKey'
end

GO

CREATE PROCEDURE [dbo].[RetrieveFile]
	@Id BIGINT
AS
BEGIN
	SET nocount ON

	SELECT
	oFileName,
	oFilePath,
	oFileExtention,
	oFileSize,
	oFileCreateDate,
	oFileLastWriteDate,
	oFileLastAccessDate,
	SQLInsertDate,
	SQLStorageType,
	isnull(ConfigValue, '') AS ConfigValue,
	FileData
	FROM
		[dbo].[SQLFileStore] sfs
		INNER JOIN [dbo].[SQLFileStoreData] sfsd
			ON sfs.FileId = sfsd.FileId
		LEFT OUTER JOIN (SELECT
							 FileId,
							 ConfigValue
						 FROM
							 [dbo].[SQLFileStoreConfigItems] sfsci
							 INNER JOIN [dbo].[SQLFileStoreConfig] sfsc
								 ON sfsci.ConfigItem = sfsc.ConfigItem
						 WHERE  sfsci.ConfigItem = 'FileKey') sfsci
			ON sfs.FileId = sfsci.FileId
	WHERE  sfs.FileId = @Id 

END

GO

CREATE FUNCTION [dbo].[CSVToTable]
(
	@StringInput VARCHAR(8000)
)
RETURNS @OutputTable TABLE (
	[String] VARCHAR(10))
AS
BEGIN
	DECLARE @String VARCHAR(10)

	WHILE LEN(@StringInput) > 0
	BEGIN
		SET @String = LEFT(@StringInput, ISNULL(NULLIF(CHARINDEX(',', @StringInput) - 1, -1), LEN(@StringInput)))

		SET @StringInput = SUBSTRING(@StringInput, ISNULL(NULLIF(CHARINDEX(',', @StringInput), 0), LEN(@StringInput)) + 1, LEN(@StringInput))

		INSERT INTO @OutputTable
					([String])
		VALUES      ( @String )
	END

	RETURN
END

GO

CREATE PROCEDURE InsertTag
	@FileId  BIGINT,
	@TagList VARCHAR(8000)
AS
BEGIN
	if @FileId <> 0
	BEGIN
		--insert new tags into the tags table
		INSERT INTO [dbo].[Tags]
		SELECT [String] from [dbo].[CSVToTable] (@TagList) where [String] not in(Select Tag from [dbo].[Tags])

		--insert new tag file associations.
		INSERT INTO [SQLFileStore].[dbo].[SQLFileTags]
					([Tag],
					 [FileId])
	SELECT
			incTags.[String],
			@FileId
		FROM
			SQLFileTags sft
			right OUTER JOIN [dbo].[CSVToTable] (@TagList) incTags
				ON sft.FileId = @FileId
				   AND incTags.[String] = sft.Tag
		WHERE  sft.Tag IS NULL
		   AND sft.FileId IS NULL
	END
	ELSE
		RAISERROR('Invalid FileId')
	END	   
END

GO

CREATE PROCEDURE [dbo].[RetrieveFileDetails]
	@Id       INT = NULL,
	@FileName NVARCHAR(255) = NULL
AS
BEGIN
	SET NOCOUNT ON

	IF @Id IS NOT NULL
	BEGIN
		SELECT
			sfs.FileId,
			oFileName,
			oFilePath,
			oFileExtention,
			oFileSize,
			oFileCreateDate,
			oFileLastWriteDate,
			oFileLastAccessDate,
			SQLInsertDate,
			SQLStorageType,
			Len(filedata)                                                    AS StoredSize,
			100 - ( Len(sfsd.filedata) / Cast(oFileSize AS FLOAT) ) * 100.00 AS PercentCompressed
		FROM
			SQLFileStore sfs
			INNER JOIN SQLFileStoreData sfsd
				ON sfs.FileId = sfsd.FileId
		WHERE  sfs.FileId = @Id
	END
	ELSE
		IF @FileName IS NOT NULL
		BEGIN
			SELECT
				sfs.FileId,
				oFileName,
				oFilePath,
				oFileExtention,
				oFileSize,
				oFileCreateDate,
				oFileLastWriteDate,
				oFileLastAccessDate,
				SQLInsertDate,
				SQLStorageType,
				Len(filedata)                                                    AS StoredSize,
				100 - ( Len(sfsd.filedata) / Cast(oFileSize AS FLOAT) ) * 100.00 AS PercentCompressed
			FROM
				SQLFileStore sfs
				INNER JOIN SQLFileStoreData sfsd
					ON sfs.FileId = sfsd.FileId
			WHERE  oFileName = @FileName
		END
		ELSE
		BEGIN
			SELECT
				sfs.FileId,
				oFileName,
				oFilePath,
				oFileExtention,
				oFileSize,
				oFileCreateDate,
				oFileLastWriteDate,
				oFileLastAccessDate,
				SQLInsertDate,
				SQLStorageType,
				Len(filedata)                                                    AS StoredSize,
				100 - ( Len(sfsd.filedata) / Cast(oFileSize AS FLOAT) ) * 100.00 AS PercentCompressed
			FROM
				SQLFileStore sfs
				INNER JOIN SQLFileStoreData sfsd
					ON sfs.FileId = sfsd.FileId
		END
END

GO 
