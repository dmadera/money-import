USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Adresar_Firma_AfterInsertUpdate
ON Adresar_Firma
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	-- pokud u firmy neexistuje osoba "PREB", vytvoøí kopií osobu 
	INSERT INTO Adresar_Osoba (
		Parent_ID, Root_ID, Group_ID, Deleted, Locked, Create_ID, Create_Date, Modify_ID, Modify_Date, Kod, Poznamka, 
		Jmeno, Prijmeni, TitulPred, TitulZa, Pohlavi, Funkce, AdresaMisto, AdresaNazev, AdresaPsc, AdresaStat, AdresaUlice, 
		CisloOsoby, CisloS3, DatumPosty, Email, KrestniJmeno, Nazev, Osloveni, PosilatPostu, Spojeni, 
		Tel1Cislo, Tel1Klapka, Tel1MistniCislo, Tel1Predvolba, Tel1Typ, 
		Tel2Cislo, Tel2Klapka, Tel2MistniCislo, Tel2Predvolba, Tel2Typ, 
		Tel3Cislo, Tel3Klapka, Tel3MistniCislo, Tel3Predvolba, Tel3Typ, 
		Tel4Cislo, Tel4Klapka, Tel4MistniCislo, Tel4Predvolba, Tel4Typ, 
		AdresaStat_ID, AdresaPsc_ID, Tel1PredvolbaStat, Tel2PredvolbaStat, Tel3PredvolbaStat, Tel4PredvolbaStat, 
		Hidden, Tel1StatID, Tel2StatID, Tel3StatID, Tel4StatID, FaxCislo, FaxMistniCislo, FaxPredvolba, FaxPredvolbaStat, 
		FaxStatID, FaxKlapka, Attachments, EmailSpojeni_ID, FaxSpojeni_ID, TelefonSpojeni1_ID, TelefonSpojeni2_ID, TelefonSpojeni3_ID, 
		TelefonSpojeni4_ID, FunkceOsoby_ID, OsloveniProEmail, DatumNarozeni, System_Komentar, System_Priznak, TitulPred_ID, TitulZa_ID		
	)
	SELECT 
		inserted.ID, inserted.ID, Osoba.Group_ID, Osoba.Deleted, Osoba.Locked, inserted.Modify_ID, inserted.Modify_Date, inserted.Modify_ID, inserted.Modify_Date, Osoba.Kod, Osoba.Poznamka, 
		Osoba.Jmeno, Osoba.Prijmeni, Osoba.TitulPred, Osoba.TitulZa, Osoba.Pohlavi, Osoba.Funkce, Osoba.AdresaMisto, Osoba.AdresaNazev, Osoba.AdresaPsc, Osoba.AdresaStat, Osoba.AdresaUlice, 
		Osoba.CisloOsoby, Osoba.CisloS3, Osoba.DatumPosty, Osoba.Email, Osoba.KrestniJmeno, Osoba.Nazev, Osoba.Osloveni, Osoba.PosilatPostu, Osoba.Spojeni, 
		Osoba.Tel1Cislo, Osoba.Tel1Klapka, Osoba.Tel1MistniCislo, Osoba.Tel1Predvolba, Osoba.Tel1Typ, 
		Osoba.Tel2Cislo, Osoba.Tel2Klapka, Osoba.Tel2MistniCislo, Osoba.Tel2Predvolba, Osoba.Tel2Typ, 
		Osoba.Tel3Cislo, Osoba.Tel3Klapka, Osoba.Tel3MistniCislo, Osoba.Tel3Predvolba, Osoba.Tel3Typ, 
		Osoba.Tel4Cislo, Osoba.Tel4Klapka, Osoba.Tel4MistniCislo, Osoba.Tel4Predvolba, Osoba.Tel4Typ, 
		Osoba.AdresaStat_ID, Osoba.AdresaPsc_ID, Osoba.Tel1PredvolbaStat, Osoba.Tel2PredvolbaStat, Osoba.Tel3PredvolbaStat, Osoba.Tel4PredvolbaStat, 
		Osoba.Hidden, Osoba.Tel1StatID, Osoba.Tel2StatID, Osoba.Tel3StatID, Osoba.Tel4StatID, Osoba.FaxCislo, Osoba.FaxMistniCislo, Osoba.FaxPredvolba, Osoba.FaxPredvolbaStat, 
		Osoba.FaxStatID, Osoba.FaxKlapka, Osoba.Attachments, Osoba.EmailSpojeni_ID, Osoba.FaxSpojeni_ID, Osoba.TelefonSpojeni1_ID, Osoba.TelefonSpojeni2_ID, Osoba.TelefonSpojeni3_ID, 
		Osoba.TelefonSpojeni4_ID, Osoba.FunkceOsoby_ID, Osoba.OsloveniProEmail, Osoba.DatumNarozeni, Osoba.System_Komentar, Osoba.System_Priznak, Osoba.TitulPred_ID, Osoba.TitulZa_ID
	FROM inserted
	LEFT JOIN ( 
		SELECT DISTINCT Firma.ID AS ID
		FROM inserted AS Firma
		INNER JOIN Adresar_Osoba AS Osoba ON Osoba.Parent_ID = Firma.ID
		WHERE Osoba.FunkceOsoby_ID = (SELECT TOP 1 ID FROM Adresar_FunkceOsoby AS Funkce WHERE Funkce.Code = 'PRE')
	) AS NotFirma ON NotFirma.ID = inserted.ID
	INNER JOIN Adresar_Osoba AS Osoba ON Osoba.Kod = 'AD00001PRE'
	WHERE NotFirma.ID IS NULL

	-- aktualizuje data prebirajici osoby
	UPDATE Adresar_Osoba SET
		Kod = CONCAT(Firma.Kod, 'PRE'),
		Jmeno = '',
		Prijmeni = IIF(SpojeniPreb.ID IS NULL, SpojeniTel.Popis, SpojeniPreb.Popis),
		Nazev = IIF(SpojeniPreb.ID IS NULL, SpojeniTel.Popis, SpojeniPreb.Popis),
		Tel1Cislo = IIF(SpojeniPreb.ID IS NULL, SpojeniTel.SpojeniCislo, SpojeniPreb.SpojeniCislo),
		TelefonSpojeni1_ID = IIF(SpojeniPreb.ID IS NULL, SpojeniTel.ID, SpojeniPreb.ID)
	FROM Adresar_Osoba AS Osoba
	INNER JOIN inserted AS Firma ON Firma.ID = Osoba.Parent_ID
	LEFT JOIN (
		SELECT Spojeni.ID, Spojeni.Parent_ID, Spojeni.Popis, Spojeni.SpojeniCislo
		FROM Adresar_Spojeni AS Spojeni
		INNER JOIN Adresar_TypSpojeni AS TypSpojeni ON TypSpojeni.ID = Spojeni.TypSpojeni_ID
		WHERE TypSpojeni.Kod = 'TelPreb'
	) AS SpojeniPreb ON SpojeniPreb.Parent_ID = Firma.ID 
	LEFT JOIN (
		SELECT Spojeni.ID, Spojeni.Parent_ID, Spojeni.Popis, Spojeni.SpojeniCislo
		FROM Adresar_Spojeni AS Spojeni
		INNER JOIN Adresar_TypSpojeni AS TypSpojeni ON TypSpojeni.ID = Spojeni.TypSpojeni_ID
		WHERE TypSpojeni.Kod = 'Tel'
	) AS SpojeniTel ON SpojeniTel.Parent_ID = Firma.ID 

	UPDATE Adresar_Firma SET 
		Doprava_UserData = ISNULL(Doprava.Nazev, ''),
		Platba_UserData = ISNULL(Platba.Nazev, ''),
		NahradniPlneni_UserData = IIF(Firma.EvidovatNahradniPlneni = 1, 'NP', ''),
		Sek_UserData = ISNULL(FirAdrKlic.Kod, ''),
		HlavniCinnost_UserData = ISNULL(FirCinnost.Kod, ''),
		HlavniOsoba_ID = Osoba.ID,
		UvadetNaDokladech = 1,
		KreditFa_UserData = CASE
			WHEN Firma.PouzivatKredit = 0 THEN 'FA'
			WHEN Firma.PouzivatKredit = 1 AND Firma.HodnotaKreditu = 0 THEN '' 
			WHEN Firma.PouzivatKredit = 1 AND Firma.HodnotaKreditu > 0 THEN CONCAT('FA ', FORMAT(Firma.HodnotaKreditu / 1000, '#')) END
	FROM Adresar_Firma AS Firma
	INNER JOIN inserted ON inserted.ID = Firma.ID
	LEFT JOIN Ciselniky_ZpusobDopravy AS Doprava ON Doprava.ID = Firma.ZpusobDopravy_ID
	LEFT JOIN Ciselniky_ZpusobPlatby AS Platba ON Platba.ID = Firma.ZpusobPlatby_ID
	LEFT JOIN (
		SELECT FirAdrKlic.Parent_ID, Kod
		FROM Adresar_FirmaAdresniKlic AS FirAdrKlic
		INNER JOIN Adresar_AdresniKlic AS AdrKlic ON AdrKlic.ID = FirAdrKlic.AdresniKlic_ID
		WHERE AdrKlic.Kod = '-SEK'
	) AS FirAdrKlic ON FirAdrKlic.Parent_ID = Firma.ID
	LEFT JOIN (
		SELECT FirCinnost.Parent_ID, Cinnost.Kod AS Kod, MIN(FirCinnost.Poradi) AS Poradi
		FROM Adresar_FirmaCinnost AS FirCinnost
		INNER JOIN Ciselniky_Cinnost AS Cinnost ON Cinnost.ID = FirCinnost.Cinnost_ID
		GROUP BY FirCinnost.Parent_ID, Cinnost.Kod
	) AS FirCinnost ON FirCinnost.Parent_ID = Firma.ID
	LEFT JOIN Adresar_Osoba AS Osoba ON Osoba.Parent_ID = Firma.ID AND Osoba.FunkceOsoby_ID = (SELECT TOP 1 ID FROM Adresar_FunkceOsoby AS Funkce WHERE Funkce.Code = 'PRE')		

	SET NOCOUNT OFF;
END