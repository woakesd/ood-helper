CREATE TABLE [dbo].[calendar_series_join] (
    [sid] INT NOT NULL,
    [rid] INT NOT NULL,
    CONSTRAINT [PK_calendar_series_join] PRIMARY KEY CLUSTERED ([sid] ASC, [rid] ASC),
    CONSTRAINT [FK_calendar_series_join_calendar] FOREIGN KEY ([sid]) REFERENCES [dbo].[calendar] ([rid]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_calendar_series_join_series] FOREIGN KEY ([sid]) REFERENCES [dbo].[series] ([sid])
);

