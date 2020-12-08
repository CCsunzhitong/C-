
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/16/2020 16:49:04
-- Generated from EDMX file: C:\Users\lushunjie\source\repos\Plc\Plc\Test.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [plctest];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[T_TestSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[T_TestSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'T_TestSet'
CREATE TABLE [dbo].[T_TestSet] (
    [Id] uniqueidentifier  NOT NULL,
    [D600] nvarchar(max)  NOT NULL,
    [D610] nvarchar(max)  NOT NULL,
    [D620] nvarchar(max)  NOT NULL,
    [D640] nvarchar(max)  NOT NULL,
    [D660] nvarchar(max)  NOT NULL,
    [D670] nvarchar(max)  NOT NULL,
    [D816] real  NOT NULL,
    [D818] real  NOT NULL,
    [D820] real  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [M1007] bit  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'T_TestSet'
ALTER TABLE [dbo].[T_TestSet]
ADD CONSTRAINT [PK_T_TestSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------