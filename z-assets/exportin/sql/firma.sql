SET NOCOUNT ON

USE S4_Agenda_PEMA
GO

SELECT TOP 1
	LOWER(SUBSTRING(F.ObchNazev, 1, 4)) AS Firma
FROM Adresar_Firma AS F
INNER JOIN System_AgendaDetail AS A ON A.MojeFirma_ID = F.ID
