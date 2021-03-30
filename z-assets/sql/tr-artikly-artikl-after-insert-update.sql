USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Artikly_Artikl_AfterInsertUpdate
ON Artikly_Artikl
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	-- produktove klice - spoji jako retezec do UserData pole
	UPDATE Artikly_Artikl SET 
		Artikly_Artikl.Priznaky_UserData=SQ.Priznaky_UserData
	FROM (
		SELECT
			inserted.ID as ID,
			STRING_AGG(CONVERT(NVARCHAR(max), ISNULL(Artikly_ProduktovyKlic.Kod, '')), ' ') WITHIN GROUP (ORDER BY Artikly_ProduktovyKlic.Kod ASC) AS Priznaky_UserData
		FROM inserted
		RIGHT JOIN Artikly_ArtiklProduktovyKlic ON Artikly_ArtiklProduktovyKlic.Parent_ID=inserted.ID
		INNER JOIN Artikly_ProduktovyKlic ON Artikly_ProduktovyKlic.ID = Artikly_ArtiklProduktovyKlic.ProduktovyKlic_ID
		WHERE LEN(Artikly_ProduktovyKlic.Kod) = 1
		GROUP BY inserted.ID
	) AS SQ
	WHERE Artikly_Artikl.ID = SQ.ID

	-- jednotky - nastavi prodejni jednotku prvni pod hlavni, pocet prodejni jednotky nastavi do UserData pole
	UPDATE Artikly_Artikl SET
	Artikly_Artikl.ProdejniJednotka_ID = ISNULL(SQ.ProdejniJednotka_ID, Artikly_ArtiklJednotka.ID),
	Artikly_Artikl.NakupniJednotka_ID = ISNULL(SQ.ProdejniJednotka_ID, Artikly_ArtiklJednotka.ID)
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
		Artikly_ArtiklJednotka.Deleted = 0 AND
		Artikly_Artikl.ID IN (SELECT ID FROM inserted)

	UPDATE Artikly_Artikl SET
		BaleniMnozstvi_UserData = Bal.Mnozstvi,
		BaleniJednotky_UserData = Bal.Jednotky
	FROM Artikly_Artikl AS Art
	INNER JOIN (
		SELECT 
			ArtJed.Parent_ID as Parent_ID, 
			STRING_AGG(FORMAT(ArtJed.VychoziMnozstvi, '#' ), '/') WITHIN GROUP (ORDER BY VychoziMnozstvi DESC) AS Mnozstvi,
			STRING_AGG(ArtJed.Kod, '/') WITHIN GROUP (ORDER BY VychoziMnozstvi DESC)  AS Jednotky
		FROM (
			SELECT AJ.VychoziMnozstvi, AJ.Kod, AJ.Parent_ID, AJ.Jednotka_ID, ParentJednotka_ID
			FROM Artikly_ArtiklJednotka AS AJ
			INNER JOIN inserted ON inserted.ID = AJ.Parent_ID 
		) AS ArtJed
		INNER JOIN Ciselniky_Jednotka AS Jed ON Jed.ID = ArtJed.Jednotka_ID
		WHERE ArtJed.ParentJednotka_ID IS NOT NULL AND Jed.KartonovaJednotka_UserData = 1
		GROUP BY ArtJed.Parent_ID
	) Bal ON Art.ID = Bal.Parent_ID

	-- jednotky - u dodavatele nastavi prodejni jednotku
	UPDATE Artikly_ArtiklDodavatel SET
		Jednotka_ID = ArtJed.Jednotka_ID,
		ArtiklJednotka_ID = ArtJed.ID
	FROM Artikly_ArtiklDodavatel AS ArtDod
	INNER JOIN inserted ON inserted.ID = ArtDod.Parent_ID
	INNER JOIN Artikly_Artikl AS Artikl ON Artikl.ID = ArtDod.Parent_ID
	INNER JOIN Artikly_ArtiklJednotka AS ArtJed ON ArtJed.ID = Artikl.NakupniJednotka_ID

	-- jednotky - nastavi u vedlejsich jednotek aktualizovanych artiklu nedelitelne mnozstvi na 0
	UPDATE Artikly_ArtiklJednotka SET
		Artikly_ArtiklJednotka.NedelitelneMnozstvi = 0.0
	FROM (
		SELECT ID FROM inserted
	) AS SQ
	WHERE Parent_ID = SQ.ID AND ParentJednotka_ID IS NOT NULL

	-- kategorie - naplnit uzivatelsky sloupec kategorii
	;WITH Tree (ID, Nazev, ParentObject_ID, Level, KompletniCesta, ListID) AS (
		-- anchor:
		SELECT 
			ID, Nazev, ParentObject_ID, 0, CAST(Nazev AS varchar(max)), CAST(ID AS varchar(max))
		FROM Artikly_KategorieArtiklu WHERE ParentObject_ID IS NULL
		UNION ALL
		-- recursive:
		SELECT 
			t.ID, t.Nazev, t.ParentObject_ID, Tree.Level + 1, 
			CAST(CONCAT(Tree.KompletniCesta, ' > ', t.Nazev) AS VARCHAR(max)),
			CAST(CONCAT(Tree.ListID, '|', t.ID) AS VARCHAR(max))
		FROM Tree 
		INNER JOIN Artikly_KategorieArtiklu AS t ON t.ParentObject_ID = Tree.ID
	)

	UPDATE Artikly_Artikl SET
		Kategorie_UserData = SQ.Kategorie_UserData,
		Kategorie = SQ.Kategorie
	FROM Artikly_Artikl AS Art
	INNER JOIN inserted ON inserted.ID = Art.ID
	RIGHT JOIN (
		SELECT 
			Art.ID AS ID,
			STRING_AGG(Kat.KompletniCesta, ' @ ') AS Kategorie_UserData,
			LOWER(STRING_AGG(Kat.ListID, '|')) AS Kategorie
		FROM Artikly_Artikl AS Art
		CROSS APPLY STRING_SPLIT(Art.Kategorie, '|') AS ArtKat
		INNER JOIN (
			SELECT 
				ID, KompletniCesta, ListID 
			FROM Tree
			WHERE ParentObject_ID IS NOT NULL
		) AS Kat ON CAST(Kat.ID AS varchar(100)) = ArtKat.value
		GROUP BY Art.ID
	) AS SQ ON SQ.ID = Art.ID

	OPTION (MAXRECURSION 5);

END