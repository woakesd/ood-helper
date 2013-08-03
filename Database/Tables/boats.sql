﻿CREATE TABLE [dbo].[boats] (
    [bid]                       INT              IDENTITY (1, 1) NOT NULL,
    [id]                        INT              NULL,
    [boatname]                  NVARCHAR (20)    NULL,
    [boatclass]                 NVARCHAR (20)    NULL,
    [sailno]                    NVARCHAR (8)     NULL,
    [dinghy]                    BIT              NULL,
    [hulltype]                  NVARCHAR (1)     NULL,
    [distance]                  INT              NULL,
    [crewname]                  NVARCHAR (30)    NULL,
    [open_handicap]             INT              NULL,
    [handicap_status]           NVARCHAR (2)     NULL,
    [rolling_handicap]          INT              NULL,
    [crew_skill_factor]         INT              NULL,
    [small_cat_handicap_rating] NUMERIC (4, 3)   NULL,
    [engine_propeller]          NVARCHAR (3)     NULL,
    [keel]                      NVARCHAR (2)     NULL,
    [deviations]                NVARCHAR (30)    NULL,
    [subscription]              NVARCHAR (26)    NULL,
    [boatmemo]                  NTEXT            NULL,
    [berth]                     NVARCHAR (6)     NULL,
    [hired]                     BIT              NULL,
    [p]                         NVARCHAR (1)     NULL,
    [s]                         BIT              NULL,
    [beaten]                    INT              NULL,
    [uid]                       UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_boats] PRIMARY KEY CLUSTERED ([bid] ASC),
    CONSTRAINT [FK_boats_people] FOREIGN KEY ([id]) REFERENCES [dbo].[people] ([id]) ON DELETE SET NULL ON UPDATE CASCADE
);

