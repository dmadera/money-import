USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate
ON Sklady_Zasoba
AFTER INSERT, UPDATE
AS
BEGIN
	ALTER TABLE Ceniky_PolozkaCeniku DISABLE TRIGGER TR_Ceniky_PolozkaCeniku_AfterInsertUpdate

	UPDATE Ceniky_PolozkaCeniku SET 
		SkladovaCena_UserData = StavCena.JednotkovaSkladovaCena,
		Marze_UserData = IIF(StavCena.JednotkovaSkladovaCena = 0, 0, ROUND(100/StavCena.JednotkovaSkladovaCena*(Cena.Cena-StavCena.JednotkovaSkladovaCena), 2)),
		Cena = ROUND(Cena.Cena, 2),
		CisloDokladu_UserData = ISNULL(SUB.CisloDokladu, ''),
		DatumZmenyZasoby_UserData = IIF(SUB.Datum IS NULL, '', FORMAT(SUB.Datum, 'yyyy.MM.dd HH:mm:ss'))
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN CSW_BI_StavSkladuVCenach AS StavCena ON StavCena.Artikl_ID = Cena.Artikl_ID AND StavCena.Sklad_ID = Cena.Sklad_ID
	INNER JOIN inserted AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID AND Zasoba.Sklad_ID = Cena.Sklad_ID
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

	ALTER TABLE Ceniky_PolozkaCeniku ENABLE TRIGGER TR_Ceniky_PolozkaCeniku_AfterInsertUpdate

	DECLARE @PocetDniMinZasoby AS INT
	SET @PocetDniMinZasoby = 20

	ALTER TABLE Artikly_Artikl DISABLE TRIGGER TR_Artikly_Artikl_AfterInsertUpdate

	UPDATE Artikly_Artikl SET	
		Marze_UserData = Cena.Marze_UserData,
		NakupniCena_UserData = Cena.SkladovaCena_UserData
	FROM Artikly_Artikl AS Artikl
	INNER JOIN inserted AS Zasoba ON Zasoba.Artikl_ID = Artikl.ID
	INNER JOIN Ceniky_PolozkaCeniku AS Cena ON Cena.Artikl_ID = Artikl.ID 
	WHERE Cena.Cenik_ID = (SELECT TOP 1 Agenda.VychoziCenik_ID FROM System_AgendaDetail AS Agenda)	

	ALTER TABLE Artikly_Artikl ENABLE TRIGGER TR_Artikly_Artikl_AfterInsertUpdate

	-- zapise k zasobe denni prodej AVG a MED
	UPDATE Sklady_Zasoba SET
		Marze_UserData = Artikl.Marze_UserData,
		NakupniCena_UserData = Artikl.NakupniCena_UserData,
		ProdejMinAVG_UserData = CASE
			WHEN PocitatProdej = 0 THEN -1
			WHEN ZasobaAVG.Zasoba_ID IS NULL THEN 0
			ELSE ROUND(ZasobaAVG.SumMnozstvi / DATEDIFF(dd, ZasobaAVG.PrvniPohyb, GETDATE()) * @PocetDniMinZasoby, 0) END,
		ProdejMinMED_UserData = CASE
			WHEN PocitatProdej = 0 THEN -1
			WHEN ZasobaMED.Zasoba_ID IS NULL THEN 0
			ELSE ROUND(ZasobaMED.SumMedian / DATEDIFF(dd, ZasobaMED.PrvniPohyb, GETDATE()) * @PocetDniMinZasoby, 0) END
	FROM Sklady_Zasoba AS Zasoba
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
	LEFT JOIN (
		SELECT 
			Artikl.ID AS ID,
			ISNULL(MIN(CAST(PocitatProdejZasob_UserData AS INT)), 0) AS PocitatProdej,
			MIN(Artikl.NakupniCena_UserData) AS NakupniCena_UserData,
			MIN(Marze_UserData) AS Marze_UserData
		FROM Artikly_Artikl AS Artikl
		LEFT JOIN Artikly_ArtiklProduktovyKlic AS ArtProdKlic ON ArtProdKlic.Parent_ID = Artikl.ID
		LEFT JOIN Artikly_ProduktovyKlic AS ProdKlic ON ProdKlic.ID = ArtProdKlic.ProduktovyKlic_ID
		GROUP BY Artikl.ID
	) AS Artikl ON Artikl.ID = Zasoba.Artikl_ID
	LEFT JOIN (
		SELECT 
			Pohyb.Konto_ID AS Zasoba_ID, 
			SUM(Pohyb.Mnozstvi) AS SumMnozstvi,
			MIN(Pohyb.Datum) AS PrvniPohyb
		FROM Sklady_PohybZasoby AS Pohyb
		WHERE Pohyb.DruhPohybu = 1
		GROUP BY Pohyb.Konto_ID
	) AS ZasobaAVG ON ZasobaAVG.Zasoba_ID = Zasoba.ID
	LEFT JOIN (
		SELECT 
			Zas.ID AS Zasoba_ID, 
			MAX(Median.Median) * COUNT(Zas.ID) AS SumMedian, 
			Min(Median.DatumPohybu) AS PrvniPohyb
		FROM Sklady_Zasoba AS Zas
		INNER JOIN (
			SELECT 
				Pohyb.Konto_ID AS Zasoba_ID, 
				Datum AS DatumPohybu, 
				PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY Mnozstvi) OVER (PARTITION BY Pohyb.Konto_ID) AS Median 
			FROM Sklady_PohybZasoby AS Pohyb
			WHERE Pohyb.DruhPohybu = 1
		) AS Median ON Median.Zasoba_ID = Zas.ID
		GROUP BY Zas.ID
	) AS ZasobaMED ON ZasobaMED.Zasoba_ID = Zasoba.ID

END
