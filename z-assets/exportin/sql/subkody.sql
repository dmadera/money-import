SET NOCOUNT ON

USE S4_Agenda_PEMA
GO

SELECT
	K.Nazev,
	SUBSTRING(K.Kod, 5, 4) AS PodKod,
	SUBSTRING(K.Kod, 1, 4) AS Kod,
	''
FROM Artikly_KategorieArtiklu AS K
WHERE 
	LEN(K.Kod) = 8
ORDER BY K.Kod