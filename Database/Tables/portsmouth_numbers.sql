CREATE TABLE [dbo].[portsmouth_numbers] (
    [id]         UNIQUEIDENTIFIER NOT NULL,
    [class_name] NVARCHAR (100)   NULL,
    [no_of_crew] INT              NULL,
    [rig]        NVARCHAR (1)     NULL,
    [spinnaker]  NVARCHAR (1)     NULL,
    [engine]     NVARCHAR (3)     NULL,
    [keel]       NVARCHAR (1)     NULL,
    [number]     INT              NULL,
    [status]     NVARCHAR (1)     NULL,
    [notes]      NTEXT            NULL,
    CONSTRAINT [PK_portsmouth_numbers] PRIMARY KEY CLUSTERED ([id] ASC)
);

