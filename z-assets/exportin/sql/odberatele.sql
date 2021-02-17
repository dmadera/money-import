SET NOCOUNT ON

USE S4_Agenda_PEMA
GO

SELECT DISTINCT
	SUBSTRING(F.Kod, 4, 100) AS ID, 
	F.ObchNazev AS Nazev, 
	'' AS Nazev2,
	F.ObchUlice AS Ulice, 
	F.ObchPsc AS PSC, 
	F.ObchMisto AS Mesto, 
	F.ICO AS ICO, 
	ISNULL(TRIM(SUBSTRING(F.DIC, 1, 2)),'') AS DIC1, 
	ISNULL(TRIM(SUBSTRING(F.DIC, 3, 100)),'') AS DIC2,
	ISNULL(Osoba.Prijmeni,'') AS Prebirajici,
	ISNULL(F.Tel1Cislo,'') AS Telefon,
	ISNULL(Spoj.SpojeniCislo,'') AS Mail,
	'N' AS Odesilat,
	ISNULL(Spoj1.SpojeniCislo,'') AS MailFA,  
	IIF(F.PouzivatKredit = 1, 'N', 'A') AS KupniSmlouva,
	FORMAT(F.HodnotaSlevy, '0.00') AS RabatO, 
	FORMAT(IIF(F.SlevaUvadena_UserData != 0, F.SlevaUvadena_UserData, F.HodnotaSlevy), '0.00') AS PRabatO,
	-- pokud nejvyssi prioritu ceniku ma zakladni => prirazka=N, v eshopu: pokud sleva<0 a prirazka=N tak sleva je 0
	IIF(FirmaCenik.Kod = '_ZAKL', 'N', 'A') AS Prirazka,
	-- vybere specialni cenik
	ISNULL(FirmaCenik1.Kod,'') AS CisloSkup,
	F.KodOdb_UserData AS KodOdb,
	''
FROM Adresar_Firma AS F
LEFT JOIN System_Groups AS Grp ON Grp.ID = F.Group_ID
LEFT JOIN (
	SELECT Osoba.Parent_ID, STRING_AGG(Osoba.Prijmeni, ', ') AS Prijmeni
	FROM Adresar_Osoba AS Osoba 
	INNER JOIN Adresar_FunkceOsoby AS FceOs ON FceOs.ID = Osoba.FunkceOsoby_ID AND FceOs.Code = 'PRE'
	GROUP BY Osoba.Parent_ID
) AS Osoba ON Osoba.Parent_ID = F.ID
LEFT JOIN (
	SELECT
		MIN(FirmaCenik.Poradi) AS Poradi, FirmaCenik.Firma_ID AS Firma_ID, MIN(Cenik.Kod) AS Kod
	FROM Adresar_FirmaCenik AS FirmaCenik
	INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = FirmaCenik.Cenik_ID
	GROUP BY FirmaCenik.Firma_ID
) AS FirmaCenik ON FirmaCenik.Firma_ID = F.ID
LEFT JOIN (
	SELECT
		MIN(FirmaCenik.Poradi) AS Poradi, FirmaCenik.Firma_ID AS Firma_ID, MIN(Cenik.Kod) AS Kod
	FROM Adresar_FirmaCenik AS FirmaCenik
	INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = FirmaCenik.Cenik_ID
	WHERE Cenik.Kod NOT LIKE '\_%' ESCAPE '\'
	GROUP BY FirmaCenik.Firma_ID
) AS FirmaCenik1 ON FirmaCenik1.Firma_ID = F.ID
LEFT JOIN (
	SELECT 
		STRING_AGG(SpojeniCislo, ',') AS SpojeniCislo, Spoj.Parent_ID
	FROM Adresar_Spojeni AS Spoj
	INNER JOIN Adresar_TypSpojeni AS TypSpoj ON TypSpoj.ID = Spoj.TypSpojeni_ID AND TypSpoj.Kod = 'E-mail'
	GROUP BY Spoj.Parent_ID
) AS Spoj ON Spoj.Parent_ID = F.ID
LEFT JOIN (
	SELECT 
		STRING_AGG(SpojeniCislo, ',') AS SpojeniCislo, Spoj.Parent_ID
	FROM Adresar_Spojeni AS Spoj
	INNER JOIN Adresar_TypSpojeni AS TypSpoj ON TypSpoj.ID = Spoj.TypSpojeni_ID AND TypSpoj.Kod = 'E-mailFA'
	GROUP BY Spoj.Parent_ID
) AS Spoj1 ON Spoj1.Parent_ID = F.ID
WHERE 
	F.Kod NOT LIKE 'ADR2%'
	AND (Grp.Kod != 'ZRUS' OR Grp.Kod IS NULL)
ORDER BY ID