USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER AktualizujArtiklyAfterInsertUpdate
ON Artikly_Artikl
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	/* produktove klice - spoji jako retezec do UserData pole */
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

	/* jednotky - nastavi prodejni jednotku první pod hlavní, pocet prodejni jednotky nastavi do UserData pole */
	UPDATE Artikly_Artikl SET
	Artikly_Artikl.ProdJednotkaMnozstvi_UserData = ISNULL(SQ.ProdJednotkaMnozstvi_UserData, Artikly_ArtiklJednotka.NedelitelneMnozstvi), 
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


	/* jednotky - nastavi u vedlejsich jednotek aktualizovanych artiklu nedelitelne mnozstvi na 0 */
	UPDATE Artikly_ArtiklJednotka SET
		Artikly_ArtiklJednotka.NedelitelneMnozstvi = 0.0
	FROM (
		SELECT ID FROM inserted
	) AS SQ
	WHERE Parent_ID = SQ.ID AND ParentJednotka_ID IS NOT NULL	
END