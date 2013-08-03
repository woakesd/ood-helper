CREATE TABLE [dbo].[series_results] (
    [sid]      INT           NOT NULL,
    [bid]      INT           NOT NULL,
    [division] NVARCHAR (20) NOT NULL,
    [entered]  INT           NULL,
    [gross]    FLOAT (53)    NULL,
    [nett]     FLOAT (53)    NULL,
    [place]    INT           NULL,
    CONSTRAINT [PK_series_results] PRIMARY KEY CLUSTERED ([sid] ASC, [division] ASC, [bid] ASC)
);

