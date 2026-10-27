USE [PreparationDB]
GO

/****** Object:  Table [dbo].[Customer]    Script Date: 10/26/2026 3:52:57 PM ******/
SET ANSI_NULLS ON
GO

--drop table [dbo].[Customer]

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customer](
	[Id] [int] identity NOT NULL,
	[CustomerName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](max) NULL,
	[Pincode] [nchar](10) NULL,
	[City] [nvarchar](50) NULL,
	[State] [nvarchar](100) NULL,
	[Country] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


