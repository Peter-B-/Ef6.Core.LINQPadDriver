
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 01/11/2026 19:18:28
-- Generated from EDMX file: D:\Projects\Peter-B-\Ef6.Core.LINQPadDriver\Ef6Demo\Ef6Demo.ModelFirst\DemoModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DemoModel];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Parents'
CREATE TABLE [dbo].[Parents] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Children'
CREATE TABLE [dbo].[Children] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [ParentId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Parents'
ALTER TABLE [dbo].[Parents]
ADD CONSTRAINT [PK_Parents]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Children'
ALTER TABLE [dbo].[Children]
ADD CONSTRAINT [PK_Children]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ParentId] in table 'Children'
ALTER TABLE [dbo].[Children]
ADD CONSTRAINT [FK_ParentChild]
    FOREIGN KEY ([ParentId])
    REFERENCES [dbo].[Parents]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ParentChild'
CREATE INDEX [IX_FK_ParentChild]
ON [dbo].[Children]
    ([ParentId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------