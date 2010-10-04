-----------------------------------------------------------------------------------------------------------------------------------------------------
--drop existing tables
-----------------------------------------------------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SQLFileTags]') AND type in (N'U'))
DROP TABLE [dbo].[SQLFileTags]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SQLFileStoreConfigItems]') AND type in (N'U'))
DROP TABLE [dbo].[SQLFileStoreConfigItems]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SQLFileStoreData]') AND type in (N'U'))
DROP TABLE [dbo].[SQLFileStoreData]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SQLFileStoreConfig]') AND type in (N'U'))
DROP TABLE [dbo].[SQLFileStoreConfig]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SQLFileStore]') AND type in (N'U'))
DROP TABLE [dbo].[SQLFileStore]
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tags]') AND type in (N'U'))
DROP TABLE [dbo].[Tags]

