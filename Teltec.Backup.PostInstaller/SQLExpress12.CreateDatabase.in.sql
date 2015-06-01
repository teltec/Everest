-- Create database
CREATE DATABASE [{{database_name}}]
 CONTAINMENT = NONE
GO

-- Alter database file properties
ALTER DATABASE [{{database_name}}] MODIFY FILE
 ( NAME = N'{{database_name}}',  SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
GO

-- Alter database log properties
ALTER DATABASE [{{database_name}}] MODIFY FILE
 ( NAME = N'{{database_name}}_log', SIZE = 6272KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

-- USE IT!
USE [{{database_name}}]
GO

-- Create login and password
CREATE LOGIN [{{username}}] WITH PASSWORD=N'{{password}}'
GO

-- Create user for login
CREATE USER [{{username}}] FOR LOGIN [{{username}}]
	WITH DEFAULT_SCHEMA=[{{schema_name}}]
GO

-- Create role
CREATE ROLE [{{user_role}}] AUTHORIZATION [{{username}}]
GO

-- Create schema
--CREATE SCHEMA [{{schema_name}}] AUTHORIZATION [{{username}}]
GO

-- Alter schema ownership
--ALTER AUTHORIZATION ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO

-- Apply permissions to schemas
GRANT EXECUTE ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT ALTER ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT CONTROL ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT SELECT ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT DELETE ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT INSERT ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT UPDATE ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO
GRANT REFERENCES ON SCHEMA::[{{schema_name}}] TO [{{user_role}}]
GO

-- Ensure role membership is correct
EXEC sp_addrolemember [{{user_role}}], [{{username}}]
GO

-- Allow users to create tables in dbo
GRANT CREATE TABLE TO [{{user_role}}]
GO

-- Allow user to connect to database
GRANT CONNECT TO [{{username}}]

--
-- Alter database
--

ALTER DATABASE [{{database_name}}] SET COMPATIBILITY_LEVEL = 120
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [{{database_name}}].[{{schema_name}}].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [{{database_name}}] SET ANSI_NULL_DEFAULT OFF
GO

ALTER DATABASE [{{database_name}}] SET ANSI_NULLS OFF
GO

ALTER DATABASE [{{database_name}}] SET ANSI_PADDING OFF
GO

ALTER DATABASE [{{database_name}}] SET ANSI_WARNINGS OFF
GO

ALTER DATABASE [{{database_name}}] SET ARITHABORT OFF
GO

ALTER DATABASE [{{database_name}}] SET AUTO_CLOSE OFF
GO

ALTER DATABASE [{{database_name}}] SET AUTO_SHRINK OFF
GO

ALTER DATABASE [{{database_name}}] SET AUTO_UPDATE_STATISTICS ON
GO

ALTER DATABASE [{{database_name}}] SET CURSOR_CLOSE_ON_COMMIT OFF
GO

ALTER DATABASE [{{database_name}}] SET CURSOR_DEFAULT  GLOBAL
GO

ALTER DATABASE [{{database_name}}] SET CONCAT_NULL_YIELDS_NULL OFF
GO

ALTER DATABASE [{{database_name}}] SET NUMERIC_ROUNDABORT OFF
GO

ALTER DATABASE [{{database_name}}] SET QUOTED_IDENTIFIER OFF
GO

ALTER DATABASE [{{database_name}}] SET RECURSIVE_TRIGGERS OFF
GO

ALTER DATABASE [{{database_name}}] SET  DISABLE_BROKER
GO

ALTER DATABASE [{{database_name}}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO

ALTER DATABASE [{{database_name}}] SET DATE_CORRELATION_OPTIMIZATION OFF
GO

ALTER DATABASE [{{database_name}}] SET TRUSTWORTHY OFF
GO

ALTER DATABASE [{{database_name}}] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO

ALTER DATABASE [{{database_name}}] SET PARAMETERIZATION SIMPLE
GO

ALTER DATABASE [{{database_name}}] SET READ_COMMITTED_SNAPSHOT OFF
GO

ALTER DATABASE [{{database_name}}] SET HONOR_BROKER_PRIORITY OFF
GO

ALTER DATABASE [{{database_name}}] SET RECOVERY SIMPLE
GO

ALTER DATABASE [{{database_name}}] SET  MULTI_USER
GO

ALTER DATABASE [{{database_name}}] SET PAGE_VERIFY CHECKSUM
GO

ALTER DATABASE [{{database_name}}] SET DB_CHAINING OFF
GO

ALTER DATABASE [{{database_name}}] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )
GO

ALTER DATABASE [{{database_name}}] SET TARGET_RECOVERY_TIME = 0 SECONDS
GO

ALTER DATABASE [{{database_name}}] SET DELAYED_DURABILITY = DISABLED
GO

ALTER DATABASE [{{database_name}}] SET  READ_WRITE
GO
