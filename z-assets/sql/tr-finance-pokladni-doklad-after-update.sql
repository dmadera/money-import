USE [S4_Agenda_PEMA]
GO 

CREATE OR ALTER TRIGGER TR_Finance_PokladniDoklad_AfterUpdate
ON Finance_PokladniDoklad
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	UPDATE Finance_PokladniDoklad SET 
		Nazev = CASE
			WHEN FV.Deleted = 1 
				THEN 'Neplatn� �ek - doklad zru�en'
			WHEN FV.Deleted = 0 AND (FV.SumaCelkem < 5000 OR (Fir.VlastniSleva = 1 AND Fir.HodnotaSlevy <> 0) OR (
					SELECT COUNT(FirAdrKl.ID) FROM Adresar_FirmaAdresniKlic AS FirAdrKl 
					INNER JOIN  Adresar_AdresniKlic AS AdrKlic ON AdrKlic.ID = FirAdrKl.AdresniKlic_ID
					WHERE FirAdrKl.Parent_ID = FV.Firma_ID AND AdrKlic.Kod = '-SEK') <> 0) 
				THEN 'Neplatn� �ek - doklad nespl�uje po�adavky'
			WHEN FV.Deleted = 0 AND ROUND(CASE 
					WHEN FV.SumaCelkem >= 10000 THEN FV.SumaCelkem / 100 * 1.5
					WHEN FV.SumaCelkem >= 5000 THEN FV.SumaCelkem / 100 * 1
					ELSE PD.SumaCelkem END, 0) <> PD.SumaCelkem
				THEN CONCAT('Neplatn� �ek - opravte na ��stku ', CONVERT(NUMERIC(10,2), ROUND(CASE 
					WHEN FV.SumaCelkem >= 10000 THEN FV.SumaCelkem / 100 * 1.5
					WHEN FV.SumaCelkem >= 5000 THEN FV.SumaCelkem / 100 * 1
					ELSE PD.SumaCelkem END, 0)), ' K�')
			WHEN FV.Deleted = 0 AND FV.Firma_ID <> PD.Firma_ID 
				THEN CONCAT('Neplatn� �ek - opravte na odb�ratele ', Fir.Kod)
			ELSE 'Vydan� �ek'
		END
	FROM Finance_PokladniDoklad AS PD
	INNER JOIN inserted ON inserted.ID = PD.ID
	INNER JOIN System_Groups AS Grp ON Grp.ID = PD.Group_ID
	INNER JOIN Fakturace_FakturaVydana AS FV ON FV.CisloDokladu = PD.ParovaciSymbol
	INNER JOIN Adresar_Firma AS Fir ON Fir.ID = FV.Firma_ID
	WHERE Grp.Kod = 'SEKY'
	SET NOCOUNT OFF;
END