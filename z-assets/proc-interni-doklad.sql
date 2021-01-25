USE S4_Agenda_PEMA
GO

CREATE OR ALTER PROCEDURE dbo.Scripter_ZalozSekJakoInterniDoklad (
	@User_ID uniqueidentifier,
	@Create_date datetime = null,
	@Modify_date datetime = null,
	@Firma_ID uniqueidentifier, 
	@Jmeno nvarchar(max) = null,
	@Nazev nvarchar(max) = null,
	@Poznamka nvarchar(max) = null,
	@Castka float = null,
	@ParovaciSymbol nvarchar(max), /* cislo fa ze ktere se tvori sek */
	@Vystavil nvarchar(max) = null,
	@Deleted bit = 0,
	@Doklad_ID uniqueidentifier OUTPUT

) AS BEGIN

SET NOCOUNT ON

IF @ParovaciSymbol is null OR @ParovaciSymbol = '' BEGIN
	PRINT '@VariabilniSymbol nemuze byt prazdny.'
	RETURN 1
END

IF @User_ID is null BEGIN
	PRINT '@User_ID nemuze byt prazdny.'
	RETURN 1
END

IF @Modify_date is null SET @Modify_date = GetDate()
IF @Create_date is null SET @Create_date = GetDate()

DECLARE	@Group_ID uniqueidentifier,
		@Ciselniky_ObdobiCiselneRady_ID uniqueidentifier,
		@Ciselniky_CiselnaRada_ID uniqueidentifier,
		@Ciselniky_ObdobiCiselneRady_Cislo int, 
		@Ciselniky_ObdobiCiselneRady_CisloDokladu varchar(max),
		@Mena_ID uniqueidentifier, 
		@DruhDokladu_ID uniqueidentifier,
		@Pokladna_ID uniqueidentifier, 
		@PrimarniUcet_ID uniqueidentifier,
		@FirmaIC nvarchar(max),
		@FirmaDIC nvarchar(max),
		@FirmaNazev nvarchar(max),
		@Predkontace_ID uniqueidentifier, 
		@CleneniDPH_ID uniqueidentifier,
		@RegistraceDPH_ID uniqueidentifier,
		@MojeFirma_ID uniqueidentifier,
		@MojeFirmaIC nvarchar(max),
		@MojeFirmaDIC nvarchar(max),
		@MojeFirmaNazev nvarchar(max)

SET @Group_ID = (SELECT TOP 1 ID FROM System_Groups WHERE System_Groups.Kod = N'SEKY')

SELECT
	@FirmaIC = ICO,
	@FirmaDIC = DIC,
	@FirmaNazev = ObchNazev
FROM Adresar_Firma WHERE Adresar_Firma.ID = @Firma_ID

SELECT @Doklad_ID = Ucetnictvi_InterniDoklad.ID
FROM Ucetnictvi_InterniDoklad 
WHERE Ucetnictvi_InterniDoklad.Group_ID = @Group_ID AND Ucetnictvi_InterniDoklad.ParovaciSymbol = @ParovaciSymbol

IF @Doklad_ID is null 
BEGIN
	SET @Mena_ID = (SELECT TOP 1 ID FROM Meny_Mena WHERE Meny_Mena.Kod = N'CZK')
	SET @DruhDokladu_ID = (SELECT TOP 1 ID FROM EconomicBase_DruhDokladu WHERE EconomicBase_DruhDokladu.Kod = N'POKDOK')
	/*SET @Pokladna_ID = (SELECT TOP 1 ID FROM Finance_Pokladna WHERE Finance_Pokladna.Kod = N'SEK_INT')*/
	SET @PrimarniUcet_ID = (SELECT TOP 1 ID FROM Ucetnictvi_Ucet WHERE Ucetnictvi_Ucet.CisloUctu = N'999999')
	SET @Predkontace_ID = (SELECT TOP 1 ID FROM Ucetnictvi_Predkontace WHERE Ucetnictvi_Predkontace.Kod = N'SEKY')
	SET @CleneniDPH_ID = (SELECT TOP 1 ID FROM Dane_CleneniDPH WHERE Dane_CleneniDPH.Kod = N'_Ř000 U')
	SET @MojeFirma_ID = (SELECT TOP 1 MojeFirma_ID FROM System_AgendaDetail)
	SELECT
		@MojeFirmaIC = Adresar_Firma.ICO,
		@MojeFirmaDIC = Adresar_Firma.DIC,
		@MojeFirmaNazev = Adresar_Firma.ObchNazev
	FROM Adresar_Firma WHERE Adresar_Firma.ID = @MojeFirma_ID

	SET @RegistraceDPH_ID = (SELECT TOP 1 ID FROM Dane_DanovaRegistrace WHERE Dane_DanovaRegistrace.DIC = @MojeFirmaDIC ORDER BY PlatnostOd DESC)

	/* ciselna rada a cislo dokladi */
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
		WHERE Ciselniky_CiselnaRada.Kod = N'SEK_VYD'
		ORDER BY Ciselniky_ObdobiCiselneRady.Zacatek DESC

		DECLARE @IDs TABLE(ID uniqueidentifier)
		/* ulozi interni doklad jako sek */
		INSERT INTO Ucetnictvi_InterniDoklad ( 
			Group_ID, deleted, locked, create_ID, create_date, modify_ID, modify_date, 
			ciselnarada_ID, cislodokladu, cislorady, 
			datumvystaveni, domacimena_ID, druhdokladu_ID, faze, 
			firma_ID, ic, dic, AdresaNazev, jmeno, mena_ID, nazev, poznamka, schvaleno, stav, 
			SumaCelkem, ucetnikurzkurz, 
			ParovaciSymbol, vystavil, zauctovano, uhradyzbyva, uhradyzbyvacm, 
			typdokladu, pocetpolozek, celkovacastka, celkovacastkacm, 
			vyrizeno, eetevidovattrzbu, schvalil_ID, zauctoval_ID,
			Predkontace_ID, CleneniDPH_ID, PredkontaceZaokrouhleni_ID, PrimarniUcetMD_ID, PrimarniUcetDal_ID,
			RegistraceDPH_ID, MojeFirma_ID, MojeFirmaIC, MojeFirmaDIC, MojeFirmaNazev
		) OUTPUT inserted.ID INTO @IDs(ID) VALUES (
			@Group_ID, 0, 0, @User_ID, @Create_date, @User_ID, @Modify_date,
			@Ciselniky_CiselnaRada_ID, @Ciselniky_ObdobiCiselneRady_CisloDokladu, @Ciselniky_ObdobiCiselneRady_Cislo, 
			@Create_date, @Mena_ID, @DruhDokladu_ID, 0,
			@Firma_ID, @FirmaIC, @FirmaDIC, @FirmaNazev, @Jmeno, @Mena_ID, @Nazev, @Poznamka, 1, 0,
			@Castka, 1.000000,
			@ParovaciSymbol, @Vystavil, 1, 0, 0,
			1, 1, @Castka, @Castka,
			0, 0, @User_ID, @User_ID,
			@Predkontace_ID, @CleneniDPH_ID, @Predkontace_ID, @PrimarniUcet_ID, @PrimarniUcet_ID,
			@RegistraceDPH_ID, @MojeFirma_ID, @MojeFirmaIC, @MojeFirmaDIC, @MojeFirmaNazev
		)

		SET @Doklad_ID = (SELECT ID FROM @IDs)

		/* update noveho cisla v rade */
		UPDATE Ciselniky_ObdobiCiselneRady 
			SET Ciselniky_ObdobiCiselneRady.Cislo = @Ciselniky_ObdobiCiselneRady_Cislo + 1
			WHERE Ciselniky_ObdobiCiselneRady.ID = @Ciselniky_ObdobiCiselneRady_ID

	PRINT 'sek byl vygenerovan pod cislem ' + @Ciselniky_ObdobiCiselneRady_CisloDokladu

END
ELSE
BEGIN
	UPDATE Ucetnictvi_InterniDoklad SET 
		Modify_ID = @User_ID,
		Modify_date = @Modify_date,
		Firma_ID = IsNull(@Firma_ID, Firma_ID),
		IC = IsNull(@FirmaIC, IC),
		DIC = IsNull(@FirmaDIC, DIC),
		Jmeno = IsNull(@Jmeno, Jmeno),
		Nazev = IsNull(@Nazev, Nazev),
		Poznamka = IsNull(@Poznamka, Poznamka),
		SumaCelkem = IsNull(@Castka, SumaCelkem),
		Vystavil = IsNull(@Vystavil, Vystavil),
		Deleted = @Deleted
	WHERE Ucetnictvi_InterniDoklad.Group_ID = @Group_ID AND Ucetnictvi_InterniDoklad.ParovaciSymbol = @ParovaciSymbol

PRINT 'sek ' + @Ciselniky_ObdobiCiselneRady_CisloDokladu + ' byl upraven'
		
END

UPDATE Ucetnictvi_InterniDoklad SET
	SumaCelkemCM = SumaCelkem,
	SumaZaklad = SumaCelkem,
	SumaZakladCM = SumaCelkem,
	CelkovaCastka = SumaCelkem,
	CelkovaCastkaCM = SumaCelkem,
	DatumSchvaleni = Create_Date,
	DatumZauctovani = Create_Date,
	DatumPlatby = Create_Date, 
	DatumPlneni = Create_Date,
	DatumUcetnihoPripadu = Create_Date,
	DatumUplatneni = Create_Date
WHERE Ucetnictvi_InterniDoklad.ID = @Doklad_ID

SET NOCOUNT OFF

RETURN 0

END