USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Adresar_Firma_AfterInsertUpdate
ON Adresar_Firma
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE Adresar_Firma SET 
		Doprava_UserData = ISNULL(Doprava.Nazev, ''),
		Platba_UserData = ISNULL(Platba.Nazev, ''),
		NahradniPlneni_UserData = IIF(Firma.EvidovatNahradniPlneni = 1, 'NP', ''),
		Sek_UserData = ISNULL(FirAdrKlic.Kod, ''),
		HlavniCinnost_UserData = ISNULL(FirCinnost.Kod, '')
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
		SELECT Firma_ID, Cinnost.Kod AS Kod, MIN(FirCinnost.Poradi) AS Poradi
		FROM Adresar_FirmaCinnost AS FirCinnost
		INNER JOIN Ciselniky_Cinnost AS Cinnost ON Cinnost.ID = FirCinnost.Cinnost_ID
		GROUP BY Firma_ID, Cinnost.Kod
	) AS FirCinnost ON FirCinnost.Firma_ID = Firma.ID

	SET NOCOUNT OFF;
END