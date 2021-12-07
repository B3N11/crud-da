IF DB_ID(CarCRUD) IS NULL
	CREATE DATABASE CarCRUD;
USE CarCRUD;

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [CarBrands] (
    [ID] int NOT NULL IDENTITY,
    [name] nvarchar(max) NULL,
    CONSTRAINT [PK_CarBrands] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [UserRequests] (
    [ID] int NOT NULL IDENTITY,
    [accountRemove] bit NOT NULL,
    [brandAttach] nvarchar(max) NULL,
    CONSTRAINT [PK_UserRequests] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [CarType] (
    [ID] int NOT NULL IDENTITY,
    [brandID] int NULL,
    [name] nvarchar(max) NULL,
    CONSTRAINT [PK_CarType] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_CarType_CarBrands_brandID] FOREIGN KEY ([brandID]) REFERENCES [CarBrands] ([ID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Users] (
    [ID] int NOT NULL IDENTITY,
    [username] nvarchar(max) NULL,
    [password] nvarchar(max) NULL,
    [fullname] nvarchar(max) NULL,
    [passwordAttempts] int NOT NULL,
    [type] int NOT NULL,
    [active] bit NOT NULL,
    [requestID] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Users_UserRequests_requestID] FOREIGN KEY ([requestID]) REFERENCES [UserRequests] ([ID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [FavouriteCars] (
    [ID] int NOT NULL IDENTITY,
    [carTypeID] int NULL,
    [userDataID] int NULL,
    [year] int NOT NULL,
    [color] nvarchar(max) NULL,
    [fuel] nvarchar(max) NULL,
    CONSTRAINT [PK_FavouriteCars] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_FavouriteCars_CarType_carTypeID] FOREIGN KEY ([carTypeID]) REFERENCES [CarType] ([ID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_FavouriteCars_Users_userDataID] FOREIGN KEY ([userDataID]) REFERENCES [Users] ([ID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CarImages] (
    [ID] int NOT NULL IDENTITY,
    [favouriteCarID] int NULL,
    [image] varbinary(max) NULL,
    CONSTRAINT [PK_CarImages] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_CarImages_FavouriteCars_favouriteCarID] FOREIGN KEY ([favouriteCarID]) REFERENCES [FavouriteCars] ([ID]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_CarImages_favouriteCarID] ON [CarImages] ([favouriteCarID]);
GO

CREATE INDEX [IX_CarType_brandID] ON [CarType] ([brandID]);
GO

CREATE INDEX [IX_FavouriteCars_carTypeID] ON [FavouriteCars] ([carTypeID]);
GO

CREATE INDEX [IX_FavouriteCars_userDataID] ON [FavouriteCars] ([userDataID]);
GO

CREATE INDEX [IX_Users_requestID] ON [Users] ([requestID]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211207154741_Initial', N'5.0.5');
GO

COMMIT;
GO