USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Ceniky_PolozkaCeniku_AfterInsertUpdate
ON Ceniky_PolozkaCeniku
AFTER INSERT, UPDATE
AS
BEGIN
    
	UPDATE Ceniky_PolozkaCeniku SET
		SkladovaCena_UserData = StavCena.JednotkovaSkladovaCena,
		Marze_UserData = IIF(StavCena.JednotkovaSkladovaCena = 0, 0, ROUND(100/StavCena.JednotkovaSkladovaCena*(Cena.Cena-StavCena.JednotkovaSkladovaCena), 2))
	FROM Ceniky_PolozkaCeniku AS Cena 
	INNER JOIN inserted ON inserted.ID = Cena.ID
	INNER JOIN CSW_BI_StavSkladuVCenach AS StavCena ON StavCena.Artikl_ID = Cena.Artikl_ID AND StavCena.Sklad_ID = Cena.Sklad_ID

	UPDATE Ceniky_PolozkaCeniku SET
		NepodlehatSleveDokladu = IIF(Cenik.Kod = '_PRODEJ', 0, 1)
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN inserted ON inserted.ID = Cena.ID
	INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = Cena.Cenik_ID

	UPDATE Ceniky_PolozkaCeniku SET 
		ID = Cena.ID,
		CisloDokladu_UserData = ISNULL(SUB.CisloDokladu, ''),
		DatumZmenyZasoby_UserData = IIF(SUB.Datum IS NULL, '', FORMAT(SUB.Datum, 'yyyy.MM.dd HH:mm:ss'))
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN inserted ON inserted.ID = Cena.ID
	INNER JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID AND Zasoba.Sklad_ID = Cena.Sklad_ID
	LEFT JOIN (
		SELECT 
			MAX(Pohyb.CisloDokladu) AS CisloDokladu,
			ISNULL(MAX(Pohyb.Modify_Date), MAX(Pohyb.Create_Date)) AS Datum,
			ObPol.Zasoba_ID AS Zasoba_ID
		FROM S5_Sklady_SkladovaPolozka AS Pohyb
		INNER JOIN Obchod_ObsahPolozkySArtiklem AS ObPol ON ObPol.ID = Pohyb.ObsahPolozky_ID
		WHERE Pohyb.DruhPohybu = 0
		GROUP BY Zasoba_ID
	) AS SUB ON SUB.Zasoba_ID = Zasoba.ID
	
END