USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Adresar_Osoba_AfterInsertUpdate
ON Adresar_Osoba
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE Adresar_Osoba SET 
		Nazev = CONCAT(Osoba.Prijmeni, IIF(Osoba.Jmeno IS NULL OR Osoba.Jmeno = '', NULL, CONCAT(' ', Osoba.Jmeno)))
	FROM Adresar_Osoba AS Osoba
	INNER JOIN inserted ON inserted.ID = Osoba.ID

	UPDATE Adresar_Spojeni SET
		Popis = Osoba.Nazev
	FROM Adresar_Spojeni AS Spoj
	INNER JOIN Adresar_Osoba AS Osoba ON Osoba.ID = Spoj.Osoba_ID
	INNER JOIN inserted ON inserted.ID = Osoba.ID

	SET NOCOUNT OFF;
END