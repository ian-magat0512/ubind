IF NOT EXISTS (SELECT *
FROM sys.schemas
WHERE name = 'JobScheduler')
BEGIN
    EXEC('CREATE SCHEMA JobScheduler')
END

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT *
FROM sysobjects
WHERE name='StateMachineJob' and xtype='U')
   CREATE TABLE [JobScheduler].[StateMachineJob]
(
    [Id] [uniqueidentifier] NOT NULL,
    [HangfireJobId] [nvarchar](max) NULL,
    [StateMachineJobType] [nvarchar](max) NULL,
    [State] [nvarchar](max) NULL,
    [StartTimeAsTicksSinceEpoch] [bigint] NULL,
    [SerializedError] [nvarchar](max) NULL,
    [StateMachineJobManifest] [nvarchar](max) NULL,
    [IsDownloaded] [bit] NOT NULL,
    [IsExtracted] [bit] NOT NULL,
    [DatasetUrl] [nvarchar](max) NULL,
    [CreationTimeInTicksSinceEpoch] [bigint] NOT NULL,
    CONSTRAINT [PK_JobScheduler.StateMachineJob] 
            PRIMARY KEY CLUSTERED ([Id] ASC) 
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[JobScheduler].[StateMachineJob]') 
         AND name = 'EndTimeAsTicksSinceEpoch'
)
    ALTER TABLE [JobScheduler].[StateMachineJob] 
    ADD EndTimeAsTicksSinceEpoch [bigint] NULL
GO
