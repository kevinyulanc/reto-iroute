USE RetoCommerce;
GO

CREATE TYPE dbo.commerce_type AS TABLE (
    pc_processdate  DATE           NOT NULL,
    pc_tipodoc      NVARCHAR(20)   NULL,
    pc_numdoc       NVARCHAR(50)   NULL,
    pc_nomcomred    NVARCHAR(200)  NULL,
    pc_razonsocial  NVARCHAR(200)  NULL,
    pc_direccion    NVARCHAR(300)  NULL,
    pc_telefono     NVARCHAR(50)   NULL,
    pc_email        NVARCHAR(150)  NULL
);
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce_batch
    @rows dbo.commerce_type READONLY
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.commerce (pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
                              pc_razonsocial, pc_direccion, pc_telefono, pc_email)
    SELECT pc_processdate, pc_tipodoc, pc_numdoc, pc_nomcomred,
           pc_razonsocial, pc_direccion, pc_telefono, pc_email
    FROM @rows;

    SELECT @@ROWCOUNT AS registros_insertados;
END
GO