-----------------------------------------------------------------------------------------------------------------------------------------------------
--drop existing procedures, functions and assemblies
-----------------------------------------------------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CSVToTable]') AND type in (N'TF'))
DROP FUNCTION [dbo].[CSVToTable]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertFile]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RetrievePassPhrase]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RetrievePassPhrase]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RetrieveFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RetrieveFile]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertTag]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertTag]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RetrieveFileDetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Retrievefiledetails]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SavePassPhrase]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SavePassPhrase]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[xp_getfiledetails]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[xp_getfiledetails]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StorePassPhrase]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StorePassPhrase]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreFile]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreFile]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreFileEncrypted]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreFileEncrypted]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreFileCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreFileCompressed]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreFileEncryptedCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StoreFileEncryptedCompressed]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToDisk]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToDisk]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToDiskEncrypted]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToDiskEncrypted]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToDiskCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToDiskCompressed]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToDiskEncryptedCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToDiskEncryptedCompressed]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToRecord]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToRecord]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToRecordEncrypted]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToRecordEncrypted]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToRecordCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToRecordCompressed]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExtractFileToRecordEncryptedCompressed]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ExtractFileToRecordEncryptedCompressed]
GO
IF EXISTS (select * from sys.assemblies where name = 'FileSystemTools')
DROP ASSEMBLY FileSystemTools
GO
