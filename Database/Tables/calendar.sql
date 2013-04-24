﻿CREATE TABLE [dbo].[calendar] (
    [rid]                     INT           IDENTITY (1, 1) NOT NULL,
    [start_date]              DATETIME      NULL,
    [time_limit_type]         NVARCHAR (1)  NULL,
    [time_limit_fixed]        DATETIME      NULL,
    [time_limit_delta]        INT           NULL,
    [extension]               INT           NULL,
    [class]                   NVARCHAR (20) NULL,
    [event]                   NVARCHAR (34) NULL,
    [price_code]              NVARCHAR (1)  NULL,
    [course]                  NVARCHAR (9)  NULL,
    [ood]                     NVARCHAR (30) NULL,
    [venue]                   NVARCHAR (11) NULL,
    [racetype]                NVARCHAR (20) NULL,
    [handicapping]            NVARCHAR (1)  NULL,
    [visitors]                INT           NULL,
    [flag]                    NVARCHAR (20) NULL,
    [memo]                    NTEXT         NULL,
    [is_race]                 BIT           NULL,
    [raced]                   BIT           NULL,
    [approved]                BIT           NULL,
    [course_choice]           NVARCHAR (10) NULL,
    [laps_completed]          INT           NULL,
    [wind_speed]              NVARCHAR (10) NULL,
    [wind_direction]          NVARCHAR (10) NULL,
    [standard_corrected_time] FLOAT (53)    NULL,
    [result_calculated]       DATETIME      NULL,
    CONSTRAINT [PK_calendar] PRIMARY KEY CLUSTERED ([rid] ASC)
);
