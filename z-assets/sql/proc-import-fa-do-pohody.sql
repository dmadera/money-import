USE S4_System
GO

-- MONEY DB: S4_Agenda_PEMA
-- Pohoda DB: StwPh_46713301_202001

-- importuje faktury prijate z Money do Pohody
CREATE OR ALTER PROCEDURE User_ImportFakturDoPohody (
	@Pocet int OUTPUT
) AS BEGIN

DECLARE @Fa_INSERTED TABLE(ID INT)

-- vybere vsechny faktury vydane z Money starsi 4 dni
-- vybere vsechny faktury prijate z Money starsi 2 dnu
;WITH MoneyFaPohoda AS (
SELECT DISTINCT
		(YEAR(Fa.DatumVystaveni) % 100) * 10000000 + 200000 + CAST(Fa.VariabilniSymbol AS int) AS Cislo,
		(YEAR(Fa.DatumVystaveni) % 100) * 10000000 + 200000 + CAST(Fa.VariabilniSymbol AS int) AS VarSym,
		1 AS RelTpFak,
		17 AS RelPK,
		480 AS RelCR,
		Fa.Dph0Zaklad AS Kc0,
		Fa.Dph1Zaklad AS Kc1,
		Fa.Dph2Zaklad AS Kc2,
		0 AS Kc3, 
		ISNULL((
			SELECT SUM(CelkovaCena) 
			FROM S4_Agenda_PEMA.dbo.Fakturace_PolozkaFakturyVydane AS FaPol 
			WHERE FaPol.Parent_ID = Fa.ID AND FaPol.PreneseniDane_ID IS NOT NULL
		), 0) AS KcP,
		-- Fa.Dph0Dan AS KcDPH0,
		Fa.Dph1Dan AS KcDPH1,
		Fa.Dph2Dan AS KcDPH2,
		0 AS KcDPH3,
		0 AS KcU,
		0 AS KcZaloha,
		0 AS KcKRZaloha,
		Fa.Korekce0Celkem + Fa.Korekce1Celkem + Fa.Korekce2Celkem AS KcZaokr,
		0 AS ZaokrFV,
		0 AS RelZpVypDPH,
		Fa.CelkovaCastka AS KcCelkem,
		Fa.CelkovaCastka AS KcLikv,
		CASE
			WHEN ZP.Kod = 'H' THEN 2
			WHEN ZP.Kod = 'P' THEN 1
			WHEN ZP.Kod = 'D' THEN 4
			ELSE 1
		END AS RelForUh,
		CASE
			WHEN ZP.Kod = 'H' THEN 1
			WHEN ZP.Kod = 'P' THEN 2
			WHEN ZP.Kod = 'D' THEN 1
			ELSE 2
		END AS RefUcet,
		CASE WHEN ZP.Kod = 'SF' THEN 1 ELSE 0 END AS RefCin,
		PhsDPH.ID AS RelTpDPH,
		CASE WHEN Fa.EvidovatNahradniPlneni = 1 THEN PhsSTR.ID ELSE 0 END AS RefStr,
		Fa.DatumVystaveni AS DatCreate,
		Fa.DatumVystaveni AS DatSave,
		Fa.DatumVystaveni AS Datum,
		Fa.DatumVystaveni AS DatUcP,
		Fa.DatumVystaveni AS DatZdPln,
		Fa.DatumSplatnosti AS DatSplat,
		CASE WHEN ZP.Kod = 'SF' THEN 'Fakturujeme Vám za zboží dle dodacího listu' ELSE 'Fakturujeme Vám za zboží dle vaší objednávky' END AS SText,
		Fa.IC AS ICO,
		Fa.DIC AS DIC, 
		Fa.FakturacniAdresaNazev AS Firma,
		Fa.FakturacniAdresaUlice AS Ulice,
		Fa.FakturacniAdresaMisto AS Obec,
		Fa.FakturacniAdresaPSC AS PSC,
		ISNULL((
			SELECT TOP 1 SpojeniCislo 
			FROM S4_Agenda_PEMA.dbo.Adresar_Spojeni AS Spoj 
			INNER JOIN S4_Agenda_PEMA.dbo.Adresar_TypSpojeni AS TypSpoj ON TypSpoj.ID = Spoj.TypSpojeni_ID
			WHERE Spoj.Parent_ID = Fa.Firma_ID AND TypSpoj.Kod = 'E-mailNP'
		), Fa.AdresaKoncovehoPrijemceEmail) AS Email,
		'' AS Ucet,
		'' AS KodBanky,
		'' AS KonstSym,
		'' AS SpecSym,
		'' AS PDoklad,
		0 AS RelTpCalcDPH
	FROM S4_Agenda_PEMA.dbo.Fakturace_FakturaVydana AS Fa
	LEFT JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UD'
	LEFT JOIN StwPh_46713301_202001.dbo.sSTR AS PhsSTR ON PhsSTR.IDS = 'NP'
	LEFT JOIN S4_Agenda_PEMA.dbo.Ciselniky_ZpusobPlatby AS ZP ON ZP.ID = Fa.ZpusobPlatby_ID
	LEFT JOIN S4_Agenda_PEMA.dbo.Fakturace_PolozkaFakturyVydane AS FaPol ON FaPol.Parent_ID = Fa.ID AND FaPol.PreneseniDane_ID IS NOT NULL
	INNER JOIN S4_Agenda_PEMA.dbo.System_Groups AS Grp ON Grp.ID = Fa.Group_ID
	WHERE
		(Grp.Kod != 'FASEKY' OR Grp.Kod IS NULL)
		AND Fa.DatumVystaveni < DATEADD(day, -4, GETDATE())

	UNION ALL

	SELECT DISTINCT
		(YEAR(Fa.DatumVystaveni) % 100) * 20000000 + 200000 + CAST(Fa.VariabilniSymbol AS int) AS Cislo,
		(YEAR(Fa.DatumVystaveni) % 100) * 20000000 + 200000 + CAST(Fa.VariabilniSymbol AS int) AS VarSym,
		11 AS RelTpFak,
		518 AS RelPK,
		695 AS RelCR,
		Fa.Dph0Zaklad AS Kc0,
		Fa.Dph1Zaklad AS Kc1,
		Fa.Dph2Zaklad AS Kc2,
		0 AS Kc3, 
		ISNULL((
			SELECT SUM(CelkovaCena) 
			FROM S4_Agenda_PEMA.dbo.Fakturace_PolozkaFakturyVydane AS FaPol 
			WHERE FaPol.Parent_ID = Fa.ID AND FaPol.PreneseniDane_ID IS NOT NULL
		), 0) AS KcP,
		-- Fa.Dph0Dan AS KcDPH0,
		Fa.Dph1Dan AS KcDPH1,
		Fa.Dph2Dan AS KcDPH2,
		0 AS KcDPH3,
		0 AS KcU,
		0 AS KcZaloha,
		0 AS KcKRZaloha,
		Fa.Korekce0Celkem + Fa.Korekce1Celkem + Fa.Korekce2Celkem AS KcZaokr,
		0 AS ZaokrFV,
		0 AS RelZpVypDPH,
		Fa.CelkovaCastka AS KcCelkem,
		Fa.CelkovaCastka AS KcLikv,
		1 AS RelForUh,
		0 AS RefUcet,
		0 AS RefCin,
		PhsDPH.ID AS RelTpDPH,
		0 AS RefStr,
		Fa.DatumVystaveni AS DatCreate,
		Fa.DatumVystaveni AS DatSave,
		Fa.DatumVystaveni AS Datum,
		Fa.DatumVystaveni AS DatUcP,
		Fa.DatumVystaveni AS DatZdPln,
		Fa.DatumSplatnosti AS DatSplat,
		'Zboží' AS SText,
		Fa.IC AS ICO,
		Fa.DIC AS DIC, 
		Fa.FakturacniAdresaNazev AS Firma,
		Fa.FakturacniAdresaUlice AS Ulice,
		Fa.FakturacniAdresaMisto AS Obec,
		Fa.FakturacniAdresaPSC AS PSC,
		Fa.AdresaKoncovehoPrijemceEmail AS Email,
		Fa.BankovniSpojeniCisloUctu AS Ucet,
		Banka.CiselnyKod AS KodBanky,
		Fa.KonstantniSymbolText AS KonstSym,
		Fa.SpecifickySymbol AS SpecSym,
		Fa.CisloDokladu AS PDoklad,
		2 AS RelTpCalcDPH
	FROM S4_Agenda_PEMA.dbo.Fakturace_FakturaPrijata AS Fa
	LEFT JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'PD'
	LEFT JOIN S4_Agenda_PEMA.dbo.Ciselniky_Banka AS Banka ON Banka.ID = Fa.Banka_ID
	LEFT JOIN S4_Agenda_PEMA.dbo.Fakturace_PolozkaFakturyVydane AS FaPol ON FaPol.Parent_ID = Fa.ID AND FaPol.PreneseniDane_ID IS NOT NULL
	INNER JOIN S4_Agenda_PEMA.dbo.System_Groups AS Grp ON Grp.ID = Fa.Group_ID
	WHERE Fa.DatumVystaveni < DATEADD(day, -2, GETDATE())
)

INSERT INTO StwPh_46713301_202001.dbo.FA (
	VarSym, Cislo, RelTpFak, RelPk, RelCR, 
	Kc0, Kc1, Kc2, Kc3, KcP, KcDPH1, KcDPH2, KcDPH3, KcU, KcZaloha, KcKRZaloha, KcZaokr, 
	ZaokrFV, RelZpVypDPH, KcCelkem, KcLikv, RelForUh, RefUcet, RefCin, RelTpDPH, RefStr,
	DatCreate, DatSave, Datum, DatUcP, DatZdPln, DatSplat,
	SText, ICO, DIC, Firma, Ulice, Obec, PSC, Email,
	Ucet, KodBanky, KonstSym, SpecSym,
	PDoklad, RelTpCalcDPH
) 
OUTPUT INSERTED.ID INTO @Fa_INSERTED 
SELECT * 
FROM MoneyFaPohoda
WHERE Cislo NOT IN (SELECT PhFa.Cislo FROM StwPh_46713301_202001.dbo.FA AS PhFa)


-- vlozi polozky k fakturam podle cleneni DPH nove pridane fa
-- aby se propsalo do přiznání k DPH částka za zboží osvobozeno od DPH
-- a v přenesené daňové evidenci
INSERT INTO StwPh_46713301_202001.dbo.FApol (
    RefAg, Kod,
    SText, OrderFld, PDP,
    DatCreate, DatSave,
    RelSzDPH, RelTpDPH,
    KcJedn, Kc, KcDPH, JCbezDPH, Mnozstvi,
    RefSKz0, RefStr, RefCin, RelTypPolEET, RelPk,
    Sleva, KcKRozd, Prenes, MJKoef,
    Cm, CmJedn, CmDPH, CmKurs, CmMnoz
) (
    -- 21% DPH
    SELECT TOP 1
        PhFA.ID, 'DPH21',
        'Zboží v 21% sazbě DPH', 1, 0,
        PhFA.DatCreate, PhFA.DatSave,
        2, PhsDPH.ID,
        PhFA.Kc2, PhFA.Kc2, PhFA.KcDPH2, PhFA.Kc2, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM StwPh_46713301_202001.dbo.FA AS PhFA
	INNER JOIN @Fa_INSERTED AS FaIns ON FaIns.ID = PhFA.ID
	INNER JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UD' 
    WHERE PhFA.Kc2 > 0
    UNION ALL
    -- 15% DPH
    SELECT TOP 1
        PhFA.ID, 'DPH15',
        'Zboží v 15% sazbě DPH', 2, 0,
        PhFA.DatCreate, PhFA.DatSave,
        1, PhsDPH.ID,
        PhFA.Kc1, PhFA.Kc1, PhFA.KcDPH1, PhFA.Kc1, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM StwPh_46713301_202001.dbo.FA AS PhFA
	INNER JOIN @Fa_INSERTED AS FaIns ON FaIns.ID = PhFA.ID
	INNER JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UD' 
    WHERE PhFA.Kc1 > 0
    UNION ALL
    -- 0% DPH
    SELECT TOP 1
        PhFA.ID, 'DPH0',
        'Zboží osvobozené od DPH', 3, 0,
        PhFA.DatCreate, PhFA.DatSave,
        0, PhsDPH.ID,
        PhFA.Kc0, PhFA.Kc0, 0, PhFA.Kc0, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
	FROM StwPh_46713301_202001.dbo.FA AS PhFA
	INNER JOIN @Fa_INSERTED AS FaIns ON FaIns.ID = PhFA.ID
	INNER JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UNost' 
    WHERE PhFA.Kc0 > 0
    UNION ALL
    -- 0% DPH prenesena danova povinnost
    SELECT TOP 1
        PhFA.ID, 'DPH0P',
        'Zboží v přenesené daňové povinnosti', 4, 1,
        PhFA.DatCreate, PhFA.DatSave,
        0, PhsDPH.ID,
        PhFA.KcP, PhFA.KcP, 0, PhFA.KcP, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
	FROM StwPh_46713301_202001.dbo.FA AS PhFA
	INNER JOIN @Fa_INSERTED AS FaIns ON FaIns.ID = PhFA.ID
	INNER JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UDpdp' 
    WHERE PhFA.KcP > 0
)

SELECT @Pocet = COUNT(ID) FROM @Fa_INSERTED

SET NOCOUNT OFF

RETURN 0

END