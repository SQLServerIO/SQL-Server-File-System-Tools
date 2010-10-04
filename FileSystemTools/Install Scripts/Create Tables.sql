--------------------------------------------------------------------------------------------------
--Create Tables
--------------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SQLFileStore](
	[FileId] [bigint] IDENTITY(-9223372036854775808,1) NOT NULL,
	[oFileName] [nvarchar](255) NOT NULL,
	[oFilePath] [nvarchar](255) NOT NULL,
	[oFileExtention] [nvarchar](20) NOT NULL,
	[oFileSize] [bigint] NOT NULL,
	[oFileCreateDate] [datetime] NOT NULL,
	[oFileLastWriteDate] [datetime] NOT NULL,
	[oFileLastAccessDate] [datetime] NOT NULL,
	[SQLInsertDate] [datetime] NOT NULL,
	[SQLStorageType] [tinyint] NOT NULL,
 CONSTRAINT [PK_SQLFileStore] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SQLFileStoreConfig](
	[ConfigItem] [nvarchar](50) NOT NULL,
	[ConfigValue] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_SQLFileStoreConfig] PRIMARY KEY CLUSTERED 
(
	[ConfigItem] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SQLFileStoreConfigItems](
	[FileId] [bigint] NOT NULL,
	[ConfigItem] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_SQLFileStoreConfigItems] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC,
	[ConfigItem] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SQLFileStoreData](
	[FileId] [bigint] NOT NULL,
	[FileData] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_SQLFileStoreData] PRIMARY KEY CLUSTERED 
(
	[FileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[Tags](
	[Tag] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Tags_1] PRIMARY KEY CLUSTERED 
(
	[Tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SQLFileTags](
	[Tag] [nvarchar](50) NOT NULL,
	[FileId] [bigint] NOT NULL,
 CONSTRAINT [PK_SQLFileTags] PRIMARY KEY CLUSTERED 
(
	[Tag] ASC,
	[FileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SQLFileStore] ADD  CONSTRAINT [DF_SQLFileStore_SQLInsertDate]  DEFAULT (getdate()) FOR [SQLInsertDate]
GO

ALTER TABLE [dbo].[SQLFileStoreConfigItems]  WITH CHECK ADD  CONSTRAINT [FK_SQLFileStoreConfigItems_SQLFileStore] FOREIGN KEY([FileId])
REFERENCES [dbo].[SQLFileStore] ([FileId])
GO

ALTER TABLE [dbo].[SQLFileStoreConfigItems] CHECK CONSTRAINT [FK_SQLFileStoreConfigItems_SQLFileStore]
GO

ALTER TABLE [dbo].[SQLFileStoreConfigItems]  WITH CHECK ADD  CONSTRAINT [FK_SQLFileStoreConfigItems_SQLFileStoreConfig] FOREIGN KEY([ConfigItem])
REFERENCES [dbo].[SQLFileStoreConfig] ([ConfigItem])
GO

ALTER TABLE [dbo].[SQLFileStoreConfigItems] CHECK CONSTRAINT [FK_SQLFileStoreConfigItems_SQLFileStoreConfig]
GO

ALTER TABLE [dbo].[SQLFileStoreData]  WITH CHECK ADD  CONSTRAINT [FK_SQLFileStoreData_SQLFileStore] FOREIGN KEY([FileId])
REFERENCES [dbo].[SQLFileStore] ([FileId])
GO

ALTER TABLE [dbo].[SQLFileStoreData] CHECK CONSTRAINT [FK_SQLFileStoreData_SQLFileStore]
GO

ALTER TABLE [dbo].[SQLFileTags]  WITH CHECK ADD  CONSTRAINT [FK_SQLFileTags_SQLFileStore] FOREIGN KEY([FileId])
REFERENCES [dbo].[SQLFileStore] ([FileId])
GO

ALTER TABLE [dbo].[SQLFileTags] CHECK CONSTRAINT [FK_SQLFileTags_SQLFileStore]
GO

ALTER TABLE [dbo].[SQLFileTags]  WITH CHECK ADD  CONSTRAINT [FK_SQLFileTags_Tags] FOREIGN KEY([Tag])
REFERENCES [dbo].[Tags] ([Tag])
GO

ALTER TABLE [dbo].[SQLFileTags] CHECK CONSTRAINT [FK_SQLFileTags_Tags]
GO
