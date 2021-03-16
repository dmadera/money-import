USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Fakturace_PolozkaFakturyVydane_AfterInsertUpdate
ON Fakturace_PolozkaFakturyVydane
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	
	/* marze, nakupni cena do polozky */
	UPDATE Fakturace_PolozkaFakturyVydane SET
		Marze_UserData = IIF(Cena.JednotkovaSkladovaCena = 0, 0, ROUND(100/Cena.JednotkovaSkladovaCena*(Pol.JednCena-Cena.JednotkovaSkladovaCena), 2)),
		NakupniCena_UserData = Cena.JednotkovaSkladovaCena
	FROM Fakturace_PolozkaFakturyVydane AS Pol
	INNER JOIN inserted ON inserted.ID = Pol.ID
	INNER JOIN Obchod_ObsahPolozkySArtiklem AS Obsah ON Obsah.ID = Pol.ObsahPolozky_ID
	INNER JOIN CSW_BI_StavSkladuVCenach AS Cena ON Cena.Artikl_ID = Obsah.Artikl_ID

	SET NOCOUNT OFF;
END