USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate
ON Sklady_Zasoba
AFTER INSERT, UPDATE
AS
BEGIN

	/* Zapise do cenikove ceny posledni pohyb na zasobe */
	UPDATE Ceniky_PolozkaCeniku SET 
		ID = Cena.ID
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID AND Zasoba.Sklad_ID = Cena.Sklad_ID
	INNER JOIN inserted ON inserted.ID = Zasoba.ID

	UPDATE Artikly_Artikl SET
		ID = Artikl.ID
	FROM Artikly_Artikl AS Artikl
	INNER JOIN inserted ON inserted.Artikl_ID = Artikl.ID
	-- zapise k zasobe denni prodej AVG a MED
	UPDATE Sklady_Zasoba SET
		ProdejAVG_UserData = ISNULL(ROUND(ZasobaAVG.SumMnozstvi / DATEDIFF(dd, ZasobaAVG.PrvniPohyb, GETDATE()), 6), 0),
		ProdejMED_UserData = ISNULL(ROUND(ZasobaMED.SumMedian / DATEDIFF(dd, ZasobaMED.PrvniPohyb, GETDATE()), 6), 0)
	FROM Sklady_Zasoba AS Zasoba
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
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

	UPDATE Sklady_Zasoba SET
		ProdejMinAVG_UserData = IIF(Artikl.PocitatProdej = 0, -1, ROUND(Zasoba.ProdejAVG_UserData * 20, 0)),
		ProdejMinMED_UserData = IIF(Artikl.PocitatProdej = 0, -1, ROUND(Zasoba.ProdejMED_UserData * 20, 0))
	FROM Sklady_Zasoba AS Zasoba
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
	LEFT JOIN (
		SELECT 
			Artikl.ID AS ID, 
			MIN(CAST(PocitatProdejZasob_UserData AS INT)) AS PocitatProdej
		FROM Artikly_Artikl AS Artikl
		LEFT JOIN Artikly_ArtiklProduktovyKlic AS ArtProdKlic ON ArtProdKlic.Parent_ID = Artikl.ID
		LEFT JOIN Artikly_ProduktovyKlic AS ProdKlic ON ProdKlic.ID = ArtProdKlic.ProduktovyKlic_ID
		GROUP BY Artikl.ID
	) AS Artikl ON Artikl.ID = Zasoba.Artikl_ID

	UPDATE Sklady_Zasoba SET
		BaleniMnozstvi_UserData = Artikl.BaleniMnozstvi_UserData,
		BaleniJednotky_UserData = Artikl.BaleniJednotky_UserData,
		NakupniCena_UserData = Artikl.NakupniCena_UserData,
		Marze_UserData = Artikl.Marze_UserData,
		Priznaky_UserData = Artikl.Priznaky_UserData
	FROM Sklady_Zasoba AS Zasoba
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
	INNER JOIN Artikly_Artikl AS Artikl ON Artikl.ID = Zasoba.Artikl_ID

END
