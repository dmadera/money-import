USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Ceniky_PolozkaCeniku_AfterInsertUpdate
ON Ceniky_PolozkaCeniku
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	UPDATE Ceniky_PolozkaCeniku SET
		VychCena = StavCena.JednotkovaSkladovaCena,
		VypocetVyseZmeny = IIF(StavCena.JednotkovaSkladovaCena = 0, 0, ROUND(100/StavCena.JednotkovaSkladovaCena*(Cena.Cena-StavCena.JednotkovaSkladovaCena), 2)) * -1
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN inserted ON inserted.ID = Cena.ID
	INNER JOIN CSW_BI_StavSkladuVCenach AS StavCena ON StavCena.Artikl_ID = Cena.Artikl_ID
	INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = Cena.Cenik_ID
	WHERE Cenik.ZdrojTypZdroje = 0 and Cena.VypocetZpusobVypoctu = 0
	SET NOCOUNT OFF;
END