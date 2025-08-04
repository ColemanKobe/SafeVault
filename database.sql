-- Updated Users table schema with password support
-- Final schema after migrations:
-- CREATE TABLE [Users] (
--     [Id] int NOT NULL IDENTITY,
--     [Username] nvarchar(50) NOT NULL,
--     [Email] nvarchar(100) NOT NULL,
--     [PasswordHash] nvarchar(500) NOT NULL,
--     [Salt] nvarchar(100) NOT NULL,
--     [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
--     [UpdatedAt] datetime2 NULL,
--     [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
--     CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
-- );

-- Password columns have been added via migration
-- ALTER TABLE [Users] ADD [PasswordHash] nvarchar(500) NULL, [Salt] nvarchar(100) NULL;

SELECT * FROM [Users];