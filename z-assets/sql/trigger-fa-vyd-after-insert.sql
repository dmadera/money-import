USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER VytvorSek
ON Fakturace_FakturaVydana
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE
		@Doklad_ID uniqueidentifier,
		@RetDoklad_ID uniqueidentifier

	DECLARE MY_CURSOR CURSOR 
	  LOCAL STATIC READ_ONLY FORWARD_ONLY
	FOR 
	SELECT DISTINCT inserted.ID 
	FROM inserted
	INNER JOIN System_Groups ON System_Groups.ID = inserted.Group_ID
	INNER JOIN Adresar_Firma ON Adresar_Firma.ID = inserted.Firma_ID
	WHERE System_Groups.Kod = N'FAVYD' AND Adresar_Firma.VlastniSleva = 0

	OPEN MY_CURSOR FETCH NEXT FROM MY_CURSOR INTO @Doklad_ID
	WHILE @@FETCH_STATUS = 0
	BEGIN 
		DECLARE
			@return_value int,
			@now datetime = GetDate(),
			@User_ID uniqueidentifier,
			@Firma_ID uniqueidentifier,
			@Jmeno nvarchar(max),
			@Castka decimal,
			@ParovaciSymbol nvarchar(20),
			@Vystavil nvarchar(max),
			@Nazev nvarchar(max),
			@Poznamka nvarchar(max),
			@Deleted bit

		SELECT
			@User_ID = Create_ID,
			@Firma_ID = Firma_ID,
			@Jmeno = AdresaKontaktniOsobaJmeno,
			@Nazev = N'Vygenerovany sek',
			@Poznamka = N'',
			@Castka = SumaZaklad * 0.05,
			@ParovaciSymbol = CisloDokladu,
			@Vystavil = Vystavil,
			@Deleted = Deleted
		FROM inserted WHERE ID = @Doklad_ID

		EXEC 
			@return_value = Scripter_ZalozSekJakoInterniDoklad
			@Create_date = @now,
			@Jmeno = @Jmeno,
			@Nazev = @Nazev,
			@Poznamka = @Poznamka,
			@User_ID = @User_ID,
			@Firma_ID = @Firma_ID,
			@Castka = @Castka,
			@ParovaciSymbol = @ParovaciSymbol,
			@Vystavil = @Vystavil,
			@Deleted = @Deleted,
			@Doklad_ID = @RetDoklad_ID OUTPUT

		UPDATE Fakturace_FakturaVydana SET
			Fakturace_FakturaVydana.Sek_UserData = (
				SELECT CisloDokladu
				FROM Ucetnictvi_InterniDoklad 
				WHERE ID = @RetDoklad_ID
			)
		FROM Fakturace_FakturaVydana
		WHERE ID = @Doklad_ID

		FETCH NEXT FROM MY_CURSOR INTO @Doklad_ID
	END
	CLOSE MY_CURSOR
	DEALLOCATE MY_CURSOR	
END