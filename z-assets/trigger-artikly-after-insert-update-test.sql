/*SELECT Artikly_Artikl.ID, Artikly_ArtiklJednotka.Parent_ID as p, VychoziMnozstvi, Artikly_ArtiklJednotka.Kod
FROM  Artikly_Artikl
RIGHT JOIN Artikly_ArtiklJednotka ON Artikly_ArtiklJednotka.Parent_ID = Artikly_Artikl.ID
WHERE Exists (
  SELECT Artikly_ArtiklJednotka.Parent_ID as Parent_ID, MIN(VychoziMnozstvi) as VychoziMnozstvi
  FROM Artikly_ArtiklJednotka
  WHERE Parent_ID is not NULL
  GROUP BY Parent_ID
) AND Artikly_ArtiklJednotka.Parent_ID IS NOT NULL
ORDER BY VychoziMnozstvi ASC */
/*
UPDATE Artikly_Artikl SET 
		Artikly_Artikl.ProdJednotkaMnozstvi_UserData = SQ.ProdJednotkaMnozstvi_UserData,
		Artikly_Artikl.ProdejniJednotka_ID = SQ.ProdejniJednotka_ID*/
	/*SELECT
		Nazev,
		SQ.ProdejniJednotka_ID,
		SQ.ProdJednotkaMnozstvi_UserData
	FROM Artikly_Artikl
	JOIN (
SELECT 
	MinMnozstviT.Parent_ID AS ID, 
			MinMnozstviT.VychoziMnozstvi AS ProdJednotkaMnozstvi_UserData, 
			Artikly_ArtiklJednotka.ID AS ProdejniJednotka_ID
FROM Artikly_Artikl
INNER JOIN (
	SELECT 
		Artikly_ArtiklJednotka.Parent_ID as Parent_ID, min(VychoziMnozstvi) as VychoziMnozstvi
	FROM  Artikly_ArtiklJednotka
	WHERE Artikly_ArtiklJednotka.ParentJednotka_ID IS NOT NULL
	GROUP BY Artikly_ArtiklJednotka.Parent_ID
) MinMnozstviT ON Artikly_Artikl.ID = MinMnozstviT.Parent_ID
INNER JOIN Artikly_ArtiklJednotka ON 
	Artikly_ArtiklJednotka.Parent_ID = Artikly_Artikl.ID AND 
	MinMnozstviT.VychoziMnozstvi = Artikly_ArtiklJednotka.VychoziMnozstvi
) AS SQ ON SQ.ID = Artikly_Artikl.ID */

/*UPDATE Artikly_Artikl SET ProdejniJednotka_ID = null, ProdJednotkaMnozstvi_UserData = 0 */

SELECT 
	ISNULL(SQ.ID, Artikly_ArtiklJednotka.Parent_ID) AS ID, 
	ISNULL(SQ.ProdJednotkaMnozstvi_UserData, Artikly_ArtiklJednotka.NedelitelneMnozstvi) AS Mnozstvi, 
	ISNULL(SQ.ProdejniJednotka_ID, Artikly_ArtiklJednotka.ID) as ArtiklJednotka_ID,
	Artikly_Artikl.Nazev,
	Artikly_ArtiklJednotka.Kod
	FROM (
		SELECT
			MinMnozstviT.Parent_ID AS ID, 
			MinMnozstviT.VychoziMnozstvi AS ProdJednotkaMnozstvi_UserData, 
			Artikly_ArtiklJednotka.ID AS ProdejniJednotka_ID
		FROM Artikly_Artikl
		INNER JOIN (
			SELECT 
				Artikly_ArtiklJednotka.Parent_ID as Parent_ID, MIN(VychoziMnozstvi) as VychoziMnozstvi
			FROM  Artikly_ArtiklJednotka
			WHERE Artikly_ArtiklJednotka.ParentJednotka_ID IS NOT NULL
			GROUP BY Artikly_ArtiklJednotka.Parent_ID
		) MinMnozstviT ON Artikly_Artikl.ID = MinMnozstviT.Parent_ID
		INNER JOIN Artikly_ArtiklJednotka ON 
			Artikly_ArtiklJednotka.Parent_ID = Artikly_Artikl.ID AND 
			MinMnozstviT.VychoziMnozstvi = Artikly_ArtiklJednotka.VychoziMnozstvi
	) AS SQ
	RIGHT JOIN Artikly_ArtiklJednotka ON Artikly_ArtiklJednotka.Parent_ID = SQ.ID
	INNER JOIN Artikly_Artikl ON Artikly_ArtiklJednotka.Parent_ID = Artikly_Artikl.ID
	WHERE 
		Artikly_ArtiklJednotka.ParentJednotka_ID IS NULL AND 
		Artikly_Artikl.ID IN (SELECT ID FROM Artikly_Artikl)
	