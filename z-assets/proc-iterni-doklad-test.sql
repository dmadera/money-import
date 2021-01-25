USE [S4_Agenda_PEMA]
GO

DECLARE	@return_value int,
		@return_value1 int,
		@now datetime = GetDate(),
		@User_ID uniqueidentifier = (SELECT CONVERT(uniqueidentifier,'5B5FD594-59EB-433D-BF98-9272BE38E9F3')),
		@Firma_ID uniqueidentifier = (SELECT CONVERT(uniqueidentifier,'BD80A265-3C65-4E3E-998C-2860C6EA605C'))

EXEC	@return_value = Scripter_ZalozSekJakoInterniDoklad
		@User_ID = @User_ID,
		@Create_date = @now,
		@Firma_ID = @Firma_ID,
		@Jmeno = N'Daniel Maděra',
		@Nazev = N'Vygenerovaný šek',
		@Poznamka = N'',
		@Castka = 1200,
		@ParovaciSymbol = N'FS00007',
		@Vystavil = N'Administrator'

EXEC	@return_value1 = Scripter_ZalozSekJakoInterniDoklad
		@User_ID = @User_ID,
		@Firma_ID = @Firma_ID,
		@Castka = 580,
		@ParovaciSymbol = N'FS00005',
		@Vystavil = N'Administrator'

SELECT	'Return Value' = @return_value, 'Return Value 1' = @return_value1 

GO 

SELECT * FROM Ucetnictvi_InterniDoklad;
/*SELECT * FROM Ucetnictvi_InterniDoklad WHERE ParovaciSymbol IN (N'FS00003', N'FS00002', N'FS00001')*/

GO 