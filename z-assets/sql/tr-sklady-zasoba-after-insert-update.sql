USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate
ON Sklady_Zasoba
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	UPDATE Ceniky_PolozkaCeniku SET ID = Cena.ID
	FROM Ceniky_PolozkaCeniku AS Cena
	INNER JOIN Sklady_Zasoba AS Zasoba ON Zasoba.Artikl_ID = Cena.Artikl_ID
	INNER JOIN inserted ON inserted.ID = Zasoba.ID
	INNER JOIN Artikly_Artikl AS Artikl ON Artikl.ID = Cena.Artikl_ID
	SET NOCOUNT OFF;
END
