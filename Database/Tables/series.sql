CREATE TABLE [dbo].[series] (
    [sid]      INT            IDENTITY (1, 1) NOT NULL,
    [sname]    NVARCHAR (255) NOT NULL,
    [discards] NVARCHAR (255) NULL,
    CONSTRAINT [PK_series] PRIMARY KEY CLUSTERED ([sid] ASC)
);

