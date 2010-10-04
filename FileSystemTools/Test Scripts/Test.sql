set nocount on
GO
--clear out everything for a test run
truncate table SQLFileStoreConfigItems
truncate table SQLFileStoreData
truncate table SQLFileTags
delete Tags
delete SQLFileStoreConfig
delete SQLFileStore
--reset to default seed 
DBCC CHECKIDENT('SQLFileStore', RESEED, -9223372036854775808)
/*
EXEC sp_configure 'show advanced options', 1
GO
-- To update the currently configured value for advanced options.
RECONFIGURE
GO
-- To enable the feature.
EXEC sp_configure 'xp_cmdshell', 1
GO
-- To update the currently configured value for this feature.
RECONFIGURE
GO
*/
exec xp_cmdshell 'del s:\temp\*.* /F /Q'
DECLARE @FileLocation nvarchar(255)
DECLARE @CompressionLevel int
DECLARE @ImageId int
declare @FilePath varchar(255)
declare @retFileId bigint
declare @returnFileId bigint

--generate key if one doesn't exist
if not exists(select ConfigValue as cryptokey,LEN(ConfigValue) as keylen from SQLFileStoreConfig)
exec StorePassPhrase 'bob'

--verify key
--select ConfigValue as cryptokey,LEN(ConfigValue) as keylen from SQLFileStoreConfig

--location of file to be imported
--must be local on disk or a UNC that SQL Server has read access to!
set @FileLocation = 's:\SQLServerFileTools\Test Scripts\testdocument.txt'

--compression level 0-9 is valid 0 is uncompressed though 3 is default
set @CompressionLevel = 3

--store file uncompress and unencrypted
EXECUTE [dbo].[StoreFile] @FileLocation ,@returnFileId OUT
--add tags to files
exec InsertTag @returnfileid,'test,file,tag1'

--store file uncompress and encrypted
EXECUTE [dbo].[StoreFileEncrypted]    @FileLocation,@returnFileId OUT
--add tags to files
exec InsertTag @returnfileid,'test,file,tag2'

--store file compress and unencrypted
EXECUTE [dbo].[StoreFileCompressed]    @FileLocation  ,@CompressionLevel,@returnFileId OUT
--add tags to files
exec InsertTag @returnfileid,'test,file,tag3'

--store file compress and encrypted
EXECUTE [dbo].[StoreFileEncryptedCompressed]    @FileLocation  ,@CompressionLevel,@returnFileId OUT
exec InsertTag @returnfileid,'test,file,tag4'

--test file details
exec Retrievefiledetails

--check all tables for data
select * from SQLFileStoreConfigItems
select * from SQLFileStoreData
select * from SQLFileTags
select * from Tags
select * from SQLFileStoreConfig
select * from SQLFileStore

--DECLARE @FileLocation nvarchar(255)
--DECLARE @CompressionLevel int
--DECLARE @ImageId int
--declare @FilePath varchar(255)
--declare @retFileId bigint
--declare @i tinyint

--location to write the file to
--must be a local disk or a UNC that mcr\sqladmin can write to!
set @FileLocation = 's:\temp'

set @FilePath = 's:\temp\testdocument.txt'
set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	print @retFileId
	exec RetrieveFile @retFileId
	--will extract file to disk no matter the storage method
	EXECUTE [dbo].[ExtractFileToDisk] @retFileId  ,@FileLocation
	print 'Trying to find file '+@FilePath	
	exec xp_getfiledetails @FilePath
	--verify file info
	set @retFileId = @retFileId + 1
end

set @FilePath = 's:\temp\testdocument.txt.enc'
set @retFileId = -9223372036854775807

while @retFileId < -9223372036854775803
begin
	--will only work on encrypted uncompressed files
	EXECUTE [dbo].[ExtractFileToDiskEncrypted] @retFileId  ,@FileLocation
	--verify file info
	print 'Trying to find file '+@FilePath		
	exec xp_getfiledetails @FilePath
	set @retFileId = @retFileId + 1
end

set @FilePath = 's:\temp\testdocument.txt.zip'
set @retFileId = -9223372036854775807

while @retFileId < -9223372036854775803
begin
	--will only work on unencrypted comrpessed files
	EXECUTE [dbo].[ExtractFileToDiskCompressed] @retFileId  ,@FileLocation
	--verify file info
	print 'Trying to find file '+@FilePath
	exec xp_getfiledetails @FilePath
	set @retFileId = @retFileId + 1
end

set @FilePath = 's:\temp\testdocument.txt.zip.enc'
set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	--will only work on encrypted compressed files
	EXECUTE [dbo].[ExtractFileToDiskEncryptedCompressed] @retFileId, @FileLocation
	--verify file info
	print 'Trying to find file '+@FilePath
	exec xp_getfiledetails @FilePath
	set @retFileId = @retFileId + 1
end

set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	--will extract file to disk no matter the storage method
	EXECUTE [dbo].[ExtractFileToRecord] @retFileId
	set @retFileId = @retFileId + 1
end


set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	--will only work on encrypted uncompressed files
	EXECUTE [dbo].[ExtractFileToRecordEncrypted] @retFileId
	set @retFileId = @retFileId + 1
end

set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	--will only work on unencrypted comrpessed files
	EXECUTE [dbo].[ExtractFileToRecordCompressed] @retFileId
	set @retFileId = @retFileId + 1
end

set @retFileId = -9223372036854775807
while @retFileId < -9223372036854775803
begin
	--will only work on encrypted compressed files
	EXECUTE [dbo].[ExtractFileToRecordEncryptedCompressed] @retFileId
	set @retFileId = @retFileId + 1
end


