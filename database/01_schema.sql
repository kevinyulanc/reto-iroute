IF DB_ID('RetoCommerce') IS NULL CREATE DATABASE RetoCommerce;
GO
USE RetoCommerce;
GO

-- Tabla principal (misma estructura que el CSV)
CREATE TABLE dbo.commerce (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    pc_processdate  DATE           NOT NULL,
    pc_tipodoc      NVARCHAR(20)   NULL,
    pc_numdoc       NVARCHAR(50)   NULL,
    pc_nomcomred    NVARCHAR(200)  NULL,
    pc_razonsocial  NVARCHAR(200)  NULL,
    pc_direccion    NVARCHAR(300)  NULL,
    pc_telefono     NVARCHAR(50)   NULL,
    pc_email        NVARCHAR(150)  NULL
);
CREATE INDEX IX_commerce_processdate ON dbo.commerce(pc_processdate);

-- Cuarentena: misma estructura + motivo
CREATE TABLE dbo.commerce_quarantine (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    pc_processdate  DATE           NOT NULL,
    pc_tipodoc      NVARCHAR(20)   NULL,
    pc_numdoc       NVARCHAR(50)   NULL,
    pc_nomcomred    NVARCHAR(200)  NULL,
    pc_razonsocial  NVARCHAR(200)  NULL,
    pc_direccion    NVARCHAR(300)  NULL,
    pc_telefono     NVARCHAR(50)   NULL,
    pc_email        NVARCHAR(150)  NULL,
    motivo          NVARCHAR(500)  NOT NULL
);
GO

-- Inserta un registro del CSV
CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @pc_processdate DATE,
    @pc_tipodoc     NVARCHAR(20)  = NULL,
    @pc_numdoc      NVARCHAR(50)  = NULL,
    @pc_nomcomred   NVARCHAR(200) = NULL,
    @pc_razonsocial NVARCHAR(200) = NULL,
    @pc_direccion   NVARCHAR(300) = NULL,
    @pc_telefono    NVARCHAR(50)  = NULL,
    @pc_email       NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.commerce (pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
                              pc_razonsocial, pc_direccion, pc_telefono, pc_email)
    VALUES (@pc_processdate, @pc_tipodoc, @pc_numdoc, @pc_nomcomred,
            @pc_razonsocial, @pc_direccion, @pc_telefono, @pc_email);
END
GO

-- Valida un día, mueve los inválidos a cuarentena y devuelve cuántos movió
CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @pc_processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRAN;

        SELECT c.*,
               CONCAT(
                 CASE WHEN LTRIM(RTRIM(ISNULL(c.pc_nomcomred,''))) = ''
                      THEN N'El nombre del comercio (nomcomred) se encuentra vacio. ' ELSE N'' END,
                 CASE WHEN LTRIM(RTRIM(ISNULL(c.pc_numdoc,''))) = ''
                      THEN N'El número (numdoc) se encuentra vacio. '
                      WHEN LTRIM(RTRIM(c.pc_numdoc)) LIKE N'%[^0-9]%'
                      THEN N'El número (numdoc) contiene letras o caracteres especiales. ' ELSE N'' END
               ) AS motivo
        INTO #invalid
        FROM dbo.commerce c
        WHERE c.pc_processdate = @pc_processdate
          AND ( LTRIM(RTRIM(ISNULL(c.pc_nomcomred,''))) = ''
             OR LTRIM(RTRIM(ISNULL(c.pc_numdoc,''))) = ''
             OR LTRIM(RTRIM(c.pc_numdoc)) LIKE N'%[^0-9]%' );

        INSERT INTO dbo.commerce_quarantine (pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
               pc_razonsocial, pc_direccion, pc_telefono, pc_email, motivo)
        SELECT pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
               pc_razonsocial, pc_direccion, pc_telefono, pc_email, RTRIM(motivo)
        FROM #invalid;

        DECLARE @total INT = @@ROWCOUNT;

        DELETE c FROM dbo.commerce c
        INNER JOIN #invalid i ON i.id = c.id;

        COMMIT;
        SELECT @total AS registros_en_cuarentena;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END
GO