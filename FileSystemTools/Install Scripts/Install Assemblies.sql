-----------------------------------------------------------------------------------------------------------------------------------------------------
--Install Assemblies 
--set the file path to the FileSystemTools.dll
-----------------------------------------------------------------------------------------------------------------------------------------------------
if NOT EXISTS(select * from sys.databases where name = DB_NAME() and is_trustworthy_on = 1)
ALTER DATABASE SQLFileStore SET trustworthy ON
GO
CREATE ASSEMBLY FileSystemTools
AUTHORIZATION [dbo]
FROM 'C:\projects\source\OpenSource\SQLServerFileSystemTools\FileSystemTools\bin\Release\SQLServerFileSystemTools.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE PROCEDURE [dbo].[xp_getfiledetails]
	@FileName [nvarchar](128)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[xp_getfiledetails]
GO

CREATE PROCEDURE [dbo].[StorePassPhrase]
	@Password [nvarchar](255)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[StorePassPhrase]
GO

CREATE PROCEDURE [dbo].[StoreFile]
	@FileLocation [nvarchar](255) ,@ReturnFileId bigint output
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[StoreFile]
GO

CREATE PROCEDURE [dbo].[StoreFileEncrypted]
	@FileLocation [nvarchar](255) ,@ReturnFileId bigint output
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[StoreFileEncrypted]
GO

CREATE PROCEDURE [dbo].[StoreFileCompressed]
	@FileLocation [nvarchar](255), @CompressionLevel int ,@ReturnFileId bigint output
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[StoreFileCompressed]
GO

CREATE PROCEDURE [dbo].[StoreFileEncryptedCompressed]
	@FileLocation [nvarchar](255), @CompressionLevel int ,@ReturnFileId bigint output
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[StoreFileEncryptedCompressed]
GO

CREATE PROCEDURE [dbo].[ExtractFileToDisk]
	@FileId [bigint],
	@FileLocation [nvarchar](255)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToDisk]
GO

CREATE PROCEDURE [dbo].[ExtractFileToDiskEncrypted]
	@FileId [bigint],
	@FileLocation [nvarchar](255)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToDiskEncrypted]
GO

CREATE PROCEDURE [dbo].[ExtractFileToDiskCompressed]
	@FileId [bigint],
	@FileLocation [nvarchar](255)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToDiskCompressed]
GO

CREATE PROCEDURE [dbo].[ExtractFileToDiskEncryptedCompressed]
	@FileId [bigint],
	@FileLocation [nvarchar](255)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToDiskEncryptedCompressed]
GO

CREATE PROCEDURE [dbo].[ExtractFileToRecord]
	@FileId [bigint]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToRecord]
GO

CREATE PROCEDURE [dbo].[ExtractFileToRecordEncrypted]
	@FileId [bigint]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToRecordEncrypted]
GO

CREATE PROCEDURE [dbo].[ExtractFileToRecordCompressed]
	@FileId [bigint]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToRecordCompressed]
GO

CREATE PROCEDURE [dbo].[ExtractFileToRecordEncryptedCompressed]
	@FileId [bigint]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [FileSystemTools].[SQLServerFileSystemTools].[ExtractFileToRecordEncryptedCompressed]
GO

