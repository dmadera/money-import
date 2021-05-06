USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate
ON Sklady_Zasoba
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	UPDATE Ceniky_PolozkaCeniku SET 
		ID = Cena.ID,
		CisloDokladu_UserData = (
			SELECT TOP 1 Pohyb.CisloDokladu FROM S5_Sklady_SkladovaPolozka AS Pohyb
			INNER JOIN Obchod_ObsahPolozkySArtiklem AS ObPol ON ObPol.ID = Pohyb.ObsahPolozky_ID
			WHERE Pohyb.DruhPohybu = 0 AND ObPol.Zasoba_ID = Zasoba.ID
			ORDER BY ISNULL(Pohyb.Modify_Date, Pohyb.Create_Date) DESC),
		DatumZmenyZasoby_UserData = (
			SELECT TOP 1 
				FORMAT(Pohyb.Create_Date, 'yyyy.MM.dd HH:mm:ss')
			FROM S5_Sklady_SkladovaPolozka AS Pohyb
			INNER JOIN Obchod_ObsahPolozkySArtiklem AS ObPol ON ObPol.ID = Pohyb.ObsahPolozky_ID
			WHERE Pohyb.DruhPohybu = 0 AND ObPol.Zasoba_ID = Zasoba.ID
			ORDER BY ISNULL(Pohyb.Modify_Date, Pohyb.Create_Date) DESC)
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID AND Zasoba.Sklad_ID = Cena.Sklad_ID
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
	SET NOCOUNT OFF;
END
