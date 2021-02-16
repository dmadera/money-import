USE S4_Agenda_PEMA
GO

SELECT DISTINCT
	SUBSTRING(A.Kod, 4, 100) AS ID,
	A.Nazev AS Nazev,
	-- vybere minimalni cenu z ceniku letakoveho, specialniho a zakladniho
	Cena.Cena AS ProdCena,
	ISNULL(ArtJed.VychoziMnozstvi, 0) AS VKart,
	ISNULL(ArtJed1.VychoziMnozstvi, 0) AS VFol,
	IIF(ArtiklJednotka.NedelitelneMnozstvi = ArtJed1.VychoziMnozstvi, 'A', 'N') AS MinFol,
	A.SkladovaPozice_UserData AS Pozice,
	CASE 
		WHEN ArtDPH.SazbaVystup = 1 THEN 21
		WHEN ArtDPH.SazbaVystup = 0 THEN 15
		ELSE 0
	END AS Sazba,
	ArtiklJednotka.Kod AS Jednotka,
	ISNULL((SELECT TOP 1 SUBSTRING(Kod,1,4) FROM Artikly_KategorieArtiklu INNER JOIN STRING_SPLIT(A.Kategorie, '|') AS Split ON Split.value = CAST(Artikly_KategorieArtiklu.ID AS varchar(100)) ORDER BY LEN(Kod) DESC), '0000') AS Kod, 
	ISNULL((SELECT TOP 1 SUBSTRING(Kod,5,4) FROM Artikly_KategorieArtiklu INNER JOIN STRING_SPLIT(A.Kategorie, '|') AS Split ON Split.value = CAST(Artikly_KategorieArtiklu.ID AS varchar(100)) ORDER BY LEN(Kod) DESC), '0000') AS PodKod,
	IIF(Zasoba.DostupneMnozstvi > 0, Zasoba.DostupneMnozstvi, 0) AS Mnozstvi,
	IIF(ProdKlic.Kod IS NULL, 'A', 'N') AS Zobrazovat,
	ISNULL(ProdKlic1.Kod, '') AS Priznak,
	'00000' AS CisloDodavatele
FROM Artikly_Artikl AS A
LEFT JOIN System_Groups AS Grp ON Grp.ID = A.Group_ID
LEFT JOIN Artikly_ArtiklJednotka AS ArtiklJednotka ON ArtiklJednotka.ID = A.HlavniJednotka_ID
LEFT JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = A.ID
LEFT JOIN (
	SELECT ArtProdKlic.Parent_ID AS Parent_ID, ProdKlic.Kod AS Kod
	FROM Artikly_ArtiklProduktovyKlic AS ArtProdKlic
	INNER JOIN Artikly_ProduktovyKlic AS ProdKlic ON ProdKlic.ID = ArtProdKlic.ProduktovyKlic_ID AND ProdKlic.Kod = 'NZ'
) AS ProdKlic ON ProdKlic.Parent_ID = A.ID
LEFT JOIN (
	SELECT ArtProdKlic.Parent_ID AS Parent_ID, MIN(ProdKlic.Kod) AS Kod
	FROM Artikly_ArtiklProduktovyKlic AS ArtProdKlic
	INNER JOIN Artikly_ProduktovyKlic AS ProdKlic ON ProdKlic.ID = ArtProdKlic.ProduktovyKlic_ID AND ProdKlic.Kod IN ('A', 'S', 'N', 'O')
	GROUP BY ArtProdKlic.Parent_ID
) AS ProdKlic1 ON ProdKlic1.Parent_ID = A.ID
LEFT JOIN (
	SELECT ArtDPH.SazbaVystup AS SazbaVystup, ArtDPH.Parent_ID AS Parent_ID
	FROM Artikly_ArtiklDPH AS ArtDPH
	INNER JOIN (
		SELECT MAX(Zacatek) AS Zacatek, Parent_ID
		FROM Artikly_ArtiklDPH AS ArtDPH
		GROUP BY ArtDPH.Parent_ID
	) AS SQ ON ArtDPH.Zacatek = SQ.Zacatek AND ArtDPH.Parent_ID = SQ.Parent_ID
) AS ArtDPH ON ArtDPH.Parent_ID = A.ID
INNER JOIN (
	SELECT 
		Cena.Artikl_ID AS Artikl_ID, MIN(Cena.Cena) AS Cena
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = Cena.Cenik_ID AND Cenik.Kod != '_NAKUP' AND Cenik.Kod LIKE '\_%' ESCAPE '\'
	GROUP BY Cena.Artikl_ID
) AS Cena ON Cena.Artikl_ID = A.ID
LEFT JOIN (
	SELECT ArtJed.Parent_ID AS Parent_ID, ArtJed.VychoziMnozstvi
	FROM Artikly_ArtiklJednotka AS ArtJed 
	INNER JOIN Ciselniky_Jednotka AS Jednotka ON Jednotka.ID = ArtJed.Jednotka_ID AND Jednotka.Kod = 'kar'
) AS ArtJed ON ArtJed.Parent_ID = A.ID 
LEFT JOIN (
	SELECT ArtJed.Parent_ID AS Parent_ID, ArtJed.VychoziMnozstvi
	FROM Artikly_ArtiklJednotka AS ArtJed 
	INNER JOIN Ciselniky_Jednotka AS Jednotka ON Jednotka.ID = ArtJed.Jednotka_ID AND Jednotka.Kod = 'fol'
) AS ArtJed1 ON ArtJed1.Parent_ID = A.ID 
WHERE 
	(Grp.Kod != 'ZRUS' OR Grp.Kod IS NULL)
	AND (Grp.Kod != 'LOM' OR Grp.Kod IS NULL)
	AND (Grp.Kod != 'OBA' OR Grp.Kod IS NULL)
	AND A.ExistujeKategorie = 1
	-- TODO filtrovat - pouze ty, co maji prirazeny el. obchod
ORDER BY ID