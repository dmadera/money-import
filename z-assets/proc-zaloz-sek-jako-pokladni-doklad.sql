USE S4_Agenda_PEMA
GO

CREATE OR ALTER PROCEDURE dbo.Scripter_ZalozSekJakoPokladniDoklad (
	@User_ID uniqueidentifier,
	@Create_date datetime = null,
	@Modify_date datetime = null,
	@Firma_ID uniqueidentifier = null,
	@Jmeno nvarchar(max) = null,
	@Nazev nvarchar(max) = null,
	@Poznamka nvarchar(max) = null,
	@Castka float = null,
	@ParovaciSymbol nvarchar(max), /* èíslo faktury ze které se tvoøí šek */
	@Vystavil nvarchar(max) = null

) AS BEGIN

SET NOCOUNT ON

IF @ParovaciSymbol is null OR @ParovaciSymbol = '' BEGIN
	PRINT '@VariabilniSymbol nemùže prázdný.'
	RETURN 1
END

IF @User_ID is null BEGIN
	PRINT '@User_ID nemùže být prázdný.'
	RETURN 1
END

IF @Modify_date is null SET @Modify_date = GetDate()
IF @Create_date is null SET @Create_date = GetDate()

/* Deklarace promìnných */
DECLARE	@Group_ID uniqueidentifier,
		@Ciselniky_ObdobiCiselneRady_ID uniqueidentifier,
		@Ciselniky_CiselnaRada_ID uniqueidentifier,
		@Ciselniky_ObdobiCiselneRady_Cislo int, 
		@Ciselniky_ObdobiCiselneRady_CisloDokladu varchar(max),
		@Mena_ID uniqueidentifier, 
		@DruhDokladu_ID uniqueidentifier,
		@Pokladna_ID uniqueidentifier, 
		@PrimarniUcet_ID uniqueidentifier,
		@PokladniDoklad_ID uniqueidentifier,
		@IC nvarchar(max),
		@DIC nvarchar(max),
		@CleneniDPH_ID uniqueidentifier,
		@Predkontace_ID uniqueidentifier


/* Zjistí identifikátory použité v pokladním dokladu */
SET @Group_ID = (SELECT TOP 1 ID FROM System_Groups WHERE System_Groups.Kod = N'SEKY')
SET @Mena_ID = (SELECT TOP 1 ID FROM Meny_Mena WHERE Meny_Mena.Kod = N'CZK')
SET @DruhDokladu_ID = (SELECT TOP 1 ID FROM EconomicBase_DruhDokladu WHERE EconomicBase_DruhDokladu.Kod = N'POKDOK')
SET @Pokladna_ID = (SELECT TOP 1 ID FROM Finance_Pokladna WHERE Finance_Pokladna.Kod = N'SEK_INT')
SET @PrimarniUcet_ID = (SELECT TOP 1 ID FROM Ucetnictvi_Ucet WHERE Ucetnictvi_Ucet.CisloUctu = N'999999')
SET @IC = (SELECT TOP 1 ICO FROM Adresar_Firma WHERE Adresar_Firma.ID = @Firma_ID)
SET @DIC = (SELECT TOP 1 DIC FROM Adresar_Firma WHERE Adresar_Firma.ID = @Firma_ID)
SET @CleneniDPH_ID = (SELECT TOP 1 ID FROM Dane_CleneniDPH WHERE Dane_CleneniDPH.Kod = N'_Ø000 P') 
SET @Predkontace_ID = (SELECT TOP 1 ID FROM Ucetnictvi_Predkontace WHERE Ucetnictvi_Predkontace.Kod = N'SEKY')


SELECT @PokladniDoklad_ID = Finance_PokladniDoklad.ID
FROM Finance_PokladniDoklad 
WHERE Finance_PokladniDoklad.Group_ID = @Group_ID AND Finance_PokladniDoklad.ParovaciSymbol = @ParovaciSymbol

IF @PokladniDoklad_ID is null 
BEGIN
	/* Zjistí èíslo dokladu a èíselnou øadu */
		SELECT TOP 1
			@Ciselniky_ObdobiCiselneRady_ID = Ciselniky_ObdobiCiselneRady.ID,
			@Ciselniky_ObdobiCiselneRady_Cislo = Ciselniky_ObdobiCiselneRady.Cislo,
			@Ciselniky_CiselnaRada_ID = Ciselniky_CiselnaRada.ID,
			@Ciselniky_ObdobiCiselneRady_CisloDokladu = CASE 
				WHEN Ciselniky_ObdobiCiselneRady.ZobrazitNuly = 1 THEN 
					CONCAT(
						Ciselniky_CiselnaRada.Prefix,
						REPLICATE('0', Ciselniky_ObdobiCiselneRady.PocetMist - LEN(
							CONCAT(
								Ciselniky_ObdobiCiselneRady.CiselnyPrefix, 
								Ciselniky_ObdobiCiselneRady.Cislo
							)
						)),
						CONCAT(Ciselniky_ObdobiCiselneRady.CiselnyPrefix, Ciselniky_ObdobiCiselneRady.Cislo)
					)
				ELSE
					CONCAT(Ciselniky_CiselnaRada.Prefix, Ciselniky_ObdobiCiselneRady.CiselnyPrefix, Ciselniky_ObdobiCiselneRady.Cislo)
			END
			FROM Ciselniky_ObdobiCiselneRady
			INNER JOIN Ciselniky_CiselnaRada ON Ciselniky_ObdobiCiselneRady.Parent_ID = Ciselniky_CiselnaRada.ID
			WHERE Ciselniky_CiselnaRada.Kod = N'SEK'
			ORDER BY Ciselniky_ObdobiCiselneRady.Zacatek DESC

		/* Uloží nový pokladní doklad jako šek */
		INSERT INTO Finance_PokladniDoklad ( 
			Group_ID, deleted, locked, create_ID, create_date, modify_ID, modify_date, 
			ciselnarada_ID, cislodokladu, cislorady, 
			datumvystaveni, domacimena_ID, druhdokladu_ID, faze, 
			firma_ID, ic, dic, jmeno, mena_ID, nazev, pokladna_ID, poznamka, schvaleno, stav, 
			sumacelkem, sumacelkemcm, sumazaklad, sumazakladcm, ucetnikurzkurz, 
			ParovaciSymbol, vystavil, zauctovano, datumplatby, uhradyzbyva, uhradyzbyvacm, 
			typdokladu, pocetpolozek, celkovacastka, celkovacastkacm, primarniucet_ID, 
			vyrizeno, eetevidovattrzbu, datumschvaleni, datumzauctovani, schvalil_ID, zauctoval_ID,
			CleneniDPH_ID, Predkontace_ID
		) VALUES (
			@Group_ID, 0, 0, @User_ID, @Create_date, @User_ID, @Modify_date,
			@Ciselniky_CiselnaRada_ID, @Ciselniky_ObdobiCiselneRady_CisloDokladu, @Ciselniky_ObdobiCiselneRady_Cislo, 
			@Create_date, @Mena_ID, @DruhDokladu_ID, 1,
			@Firma_ID, @IC, @DIC, @Jmeno, @Mena_ID, @Nazev, @Pokladna_ID, @Poznamka, 1, 0,
			@Castka, @Castka, @Castka, @Castka, 1.000000,
			@ParovaciSymbol, @Vystavil, 1, @Create_date, 0, 0,
			1, 1, @Castka, @Castka, @PrimarniUcet_ID,
			0, 0, @Create_date, @Create_date, @User_ID, @User_ID,
			@CleneniDPH_ID, @Predkontace_ID
		)

		/* Uloží nové aktuální èíslo v øadì */
		UPDATE Ciselniky_ObdobiCiselneRady 
			SET Ciselniky_ObdobiCiselneRady.Cislo = @Ciselniky_ObdobiCiselneRady_Cislo + 1
			WHERE Ciselniky_ObdobiCiselneRady.ID = @Ciselniky_ObdobiCiselneRady_ID

	PRINT 'Šek byl vygnerován pod èíslem ' + @Ciselniky_ObdobiCiselneRady_CisloDokladu + '.'

END
ELSE
BEGIN
	UPDATE Finance_PokladniDoklad SET 
		Modify_ID = @User_ID,
		Modify_date = @Modify_date,
		Firma_ID = IsNull(@Firma_ID, Firma_ID),
		IC = IsNull(@IC, IC),
		DIC = IsNull(@DIC, DIC),
		Jmeno = IsNull(@Jmeno, Jmeno),
		Nazev = IsNull(@Nazev, Nazev),
		Poznamka = IsNull(@Poznamka, Poznamka),
		SumaCelkem = IsNull(@Castka, SumaCelkem),
		SumaCelkemCM = IsNull(@Castka, SumaCelkem),
		SumaZaklad = IsNull(@Castka, SumaCelkem),
		SumaZakladCM = IsNull(@Castka, SumaCelkem),
		Vystavil = IsNull(@Vystavil, Vystavil),
		CelkovaCastka = IsNull(@Castka, SumaCelkem),
		CelkovaCastkaCM = IsNull(@Castka, SumaCelkem)
	WHERE Finance_PokladniDoklad.Group_ID = @Group_ID AND Finance_PokladniDoklad.ParovaciSymbol = @ParovaciSymbol

PRINT 'Šek è. ' + @Ciselniky_ObdobiCiselneRady_CisloDokladu + ' byl upraven.'
		
END

RETURN 0 

END