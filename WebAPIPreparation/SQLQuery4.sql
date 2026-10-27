USE [PreparationDB]
GO

/****** Object:  Table [dbo].[State]    Script Date: 10/26/2026 3:50:19 PM ******/
SET ANSI_NULLS ON
GO

--drop table dbo.State

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[State](
	[Id] [int] identity NOT NULL,
	[StateName] [nvarchar](100) NOT NULL,
	[CountryID] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


