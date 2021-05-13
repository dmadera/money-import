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
		CisloDokladu_UserData = ISNULL(SUB.CisloDokladu, ''),
		DatumZmenyZasoby_UserData = IIF(SUB.Datum IS NULL, '', FORMAT(SUB.Datum, 'yyyy.MM.dd HH:mm:ss'))
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID AND Zasoba.Sklad_ID = Cena.Sklad_ID
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
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
	SET NOCOUNT OFF;
END
