USE [S4_Agenda_PEMA]
GO

DECLARE	@return_value int,
		@return_value1 int,
		@now datetime = GetDate(),
		@User_ID uniqueidentifier = (SELECT CONVERT(uniqueidentifier,'5B5FD594-59EB-433D-BF98-9272BE38E9F3')),
		@Firma_ID uniqueidentifier = (SELECT CONVERT(uniqueidentifier,'40172DA3-72FF-4087-860D-C4D587473AEB'))

EXEC	@return_value = Scripter_ZalozSekJakoPokladniDoklad
		@User_ID = @User_ID,
		@Create_date = @now,
		@Firma_ID = @Firma_ID,
		@Jmeno = N'Daniel Madìra',
		@Nazev = N'Vygenerovaný šek',
		@Poznamka = N'',
		@Castka = 1200,
		@ParovaciSymbol = N'FS00003',
		@Vystavil = N'Administrator'

EXEC	@return_value1 = Scripter_ZalozSekJakoPokladniDoklad
		@User_ID = @User_ID,
		@Castka = 580,
		@ParovaciSymbol = N'FS00003',
		@Vystavil = N'Administrator'

SELECT	'Return Value' = @return_value, 'Return Value 1' = @return_value1 

GO 

/*SELECT * FROM Finance_PokladniDoklad;*/
SELECT * FROM Finance_PokladniDoklad WHERE ParovaciSymbol IN (N'FS00003', N'FS00002', N'FS00001')

GO