﻿CREATE TABLE [dbo].[races] (
    [rid]                  INT          NOT NULL,
    [bid]                  INT          NOT NULL,
    [start_date]           DATETIME     NULL,
    [finish_code]          NVARCHAR (5) NULL,
    [finish_date]          DATETIME     NULL,
    [interim_date]         DATETIME     NULL,
    [last_edit]            DATETIME     NULL,
    [laps]                 INT          NULL,
    [place]                INT          NULL,
    [points]               FLOAT (53)   NULL,
    [override_points]      FLOAT (53)   NULL,
    [elapsed]              INT          NULL,
    [corrected]            FLOAT (53)   NULL,
    [standard_corrected]   FLOAT (53)   NULL,
    [handicap_status]      NVARCHAR (2) NULL,
    [open_handicap]        INT          NULL,
    [rolling_handicap]     INT          NULL,
    [achieved_handicap]    INT          NULL,
    [new_rolling_handicap] INT          NULL,
    [performance_index]    INT          NULL,
    [a]                    NVARCHAR (1) NULL,
    [c]                    NVARCHAR (1) NULL,
    CONSTRAINT [PK_races] PRIMARY KEY NONCLUSTERED ([rid] ASC, [bid] ASC),
    CONSTRAINT [FK_races_boats] FOREIGN KEY ([bid]) REFERENCES [dbo].[boats] ([bid]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_races_calendar] FOREIGN KEY ([rid]) REFERENCES [dbo].[calendar] ([rid]) ON DELETE CASCADE ON UPDATE CASCADE
);

