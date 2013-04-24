CREATE TABLE [dbo].[select_rules] (
    [id]            UNIQUEIDENTIFIER NOT NULL,
    [name]          NVARCHAR (255)   NULL,
    [parent]        UNIQUEIDENTIFIER NULL,
    [application]   INT              NULL,
    [field]         NVARCHAR (255)   NULL,
    [condition]     INT              NULL,
    [string_value]  NVARCHAR (255)   NULL,
    [number_bound1] NUMERIC (18, 4)  NULL,
    [number_bound2] NUMERIC (18, 4)  NULL,
    CONSTRAINT [PK_select_rule] PRIMARY KEY CLUSTERED ([id] ASC)
);

