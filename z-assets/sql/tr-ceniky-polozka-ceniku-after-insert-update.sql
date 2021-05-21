USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_Ceniky_PolozkaCeniku_AfterInsertUpdate
ON Ceniky_PolozkaCeniku
AFTER INSERT, UPDATE
AS
BEGIN
	DECLARE @VychoziCenik_ID AS UNIQUEIDENTIFIER
	SET @VychoziCenik_ID = (SELECT TOP 1 Agenda.VychoziCenik_ID FROM System_AgendaDetail AS Agenda)

	UPDATE Ceniky_PolozkaCeniku SET
		SkladovaCena_UserData = StavCena.JednotkovaSkladovaCena,
		Marze_UserData = IIF(StavCena.JednotkovaSkladovaCena = 0, 0, ROUND(100/StavCena.JednotkovaSkladovaCena*(Cena.Cena-StavCena.JednotkovaSkladovaCena), 2)),
		Cena = ROUND(Cena.Cena, 2),
		NepodlehatSleveDokladu = IIF(Cena.Cenik_ID = @VychoziCenik_ID, 0, 1)
	FROM Ceniky_PolozkaCeniku AS Cena 
	INNER JOIN inserted ON inserted.ID = Cena.ID
	INNER JOIN CSW_BI_StavSkladuVCenach AS StavCena ON StavCena.Artikl_ID = Cena.Artikl_ID AND StavCena.Sklad_ID = Cena.Sklad_ID

	ALTER TABLE Artikly_Artikl DISABLE TRIGGER TR_Artikly_Artikl_AfterInsertUpdate

	UPDATE Artikly_Artikl SET	
		Marze_UserData = Cena.Marze_UserData,
		NakupniCena_UserData = Cena.SkladovaCena_UserData
	FROM Artikly_Artikl AS Artikl
	INNER JOIN inserted AS Cena ON Cena.Artikl_ID = Artikl.ID 
	WHERE Cena.Cenik_ID = @VychoziCenik_ID

	ALTER TABLE Artikly_Artikl ENABLE TRIGGER TR_Artikly_Artikl_AfterInsertUpdate

	ALTER TABLE Sklady_Zasoba DISABLE TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate

	UPDATE Sklady_Zasoba SET
		Marze_UserData = Cena.Marze_UserData,
		NakupniCena_UserData = Cena.SkladovaCena_UserData
	FROM Sklady_Zasoba AS Zasoba
	INNER JOIN Artikly_Artikl AS Artikl ON Artikl.ID = Zasoba.Artikl_ID
	INNER JOIN inserted AS Cena ON Cena.Artikl_ID = Artikl.ID

	ALTER TABLE Sklady_Zasoba ENABLE TRIGGER TR_Sklady_Zasoba_AfterInsertUpdate	
END