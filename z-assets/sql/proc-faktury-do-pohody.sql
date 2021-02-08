USE S4_Agenda_PEMA
GO

CREATE OR ALTER PROCEDURE dbo.Scripter_Import_faktur_Pohoda (
	@Pocet int OUTPUT
) AS BEGIN

-- vybere vsechny faktury z Money starsi 4 dni a ty co nejsou uz ve fakturach Pohody
-- a vlozi je do faktur Pohody

SELECT
	1 AS RelTpFak,
	17 AS RelPK,
	480 AS RelCR,
	Fa.Dph0Zaklad AS Kc0,
	Fa.Dph1Zaklad AS Kc1,
	Fa.Dph2Zaklad AS Kc2,
	0 AS Kc3,
	0 AS KcP, -- TODO
	Fa.Dph1Dan AS KcDPH1,
	Fa.Dph2Dan AS KcDPH2,
	0 AS KcDPH3,
	0 AS KcU,
	0 AS KcZaloha,
	0 AS KcKRZaloha,
	0 AS KcZaokr,
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
	PhsDPH.ID AS RelTpDPH, -- TODO
	CASE WHEN Fa.EvidovatNahradniPlneni = 1 THEN PhsSTR.ID ELSE 0 END AS RefStr,
	Fa.DatumVystaveni AS Datum,
	Fa.DatumVystaveni AS DatUCP,
    Fa.DatumVystaveni AS DatZdPln,
    Fa.DatumSplatnosti AS DatSplat,
	CASE WHEN ZP.Kod = 'SF' THEN 'Fakturujeme Vám za zboží dle dodacího listu' ELSE 'Fakturujeme Vám za zboží dle vaší objednávky' END AS SText,
	-- TODO
	Fa.IC AS IC,
	Fa.DIC AS DIC, 
	Fa.FakturacniAdresaNazev AS Firma,
	Fa.FakturacniAdresaUlice AS Ulice,
	Fa.FakturacniAdresaMisto AS Mesto,
	Fa.FakturacniAdresaPSC AS PSC
FROM S4_Agenda_PEMA.dbo.Fakturace_FakturaVydana AS Fa
LEFT JOIN StwPh_46713301_202001.dbo.sDPH AS PhsDPH ON PhsDPH.IDS = 'UD'
LEFT JOIN StwPh_46713301_202001.dbo.sSTR AS PhsSTR ON PhsSTR.IDS = 'NP'
INNER JOIN S4_Agenda_PEMA.dbo.System_Groups AS Grp ON Grp.ID = Fa.Group_ID
INNER JOIN S4_Agenda_PEMA.dbo.Ciselniky_ZpusobPlatby AS ZP ON ZP.ID = Fa.ZpusobPlatby_ID
WHERE 
	Grp.Kod != 'FASEKY' 
	AND Fa.DatumVystaveni < DATEADD(day, -4, GETDATE())
	AND Fa.CisloDokladu NOT IN (SELECT PhFa.Cislo FROM StwPh_46713301_202001.dbo.FA AS PhFa)

/*
    "CisloVydej AS _id" +
    ",CisloOdber as _idOdber" +
    ",DatumVydej AS _datum" +
    ",Sdani AS _sdani" +
    ",Splatnost AS _splatnost" +
    ",IIF(NahradniPl <> 0, 1, 0) AS _nahradni" +
    ",1 AS RelTpFak" +
    ",17 AS RelPK" +
    ",480 AS RelCR" +
    ",CisloObjed AS CisloObj" +
    ",Kc0" +
    ",Kc1" +
    ",Kc2" +
    ",KcP" +
    ",0 AS Kc3" +
    ",KcDPH1" +
    ",KcDPH2" +
    ",0 AS KcDPH3" +
    ",0 AS KcU" +
    ",0 as KcZaloha" +
    ",0 AS KcKRZaloha" +
    ",KcZaokr" +
    ",KcCelkem" +
    ",KcCelkem AS KcLikv" +
    ",IIF(FormaUhrad = 'P', 1, IIF(FormaUhrad = 'H', 2, IIF(FormaUhrad = 'D', 4, 1))) AS RelForUh" +
    ",IIF(FormaUhrad = 'P', 2, IIF(FormaUhrad = 'H', 1, IIF(FormaUhrad = 'D', 1, 2))) AS RefUcet" +
    ",0 AS ZaokrFV" +
    ",IIF(TiskSum <> '0', 1, 0) AS RefCin" +
    ",KodOdb AS CisloZAK" +
    ",0 AS RelZpVypDPH",

    int UD = getIdFromSql("sDPH", "IDS", "UD");
    int UDdodEU = getIdFromSql("sDPH", "IDS", "UD");
    int NP = getIdFromSql("sSTR", "IDS", "NP");

    int id = Int32.Parse(data["_id"]);
    // int refAd = Int32.Parse(data["_idOdber"]);
    DateTime date = DateTime.Parse(data["_datum"]);
    id += (date.Year % 100) * 10000000 + 100000;

    string sdani = data["_sdani"];
    bool nahradni = data["_nahradni"] == "1";
    DateTime payupDate = date.AddDays(Int32.Parse(data["_splatnost"]));
    string dateStr = date.ToString("yyyy-MM-dd");
    string payupDateStr = payupDate.ToString("yyyy-MM-dd");

    data.Remove("_id");
    // data.Remove("_idOdber");
    data.Remove("_sdani");
    data.Remove("_datum");
    data.Remove("_splatnost");
    data.Remove("_nahradni");
    data.Add("Cislo", id.ToString());
    data.Add("RefAd", refAd.ToString());
    data.Add("VarSym", id.ToString());
    data.Add("RefStr", nahradni ? NP.ToString() : "0");
    data.Add("RelTpDPH", sdani == "V" ? UDdodEU.ToString() : UD.ToString());
    data.Add("Datum", dateStr);
    data.Add("DatUCP", dateStr);
    data.Add("DatZdPln", dateStr);
    data.Add("DatSplat", payupDateStr);
    data.Add("SText", data["RefCin"] == "1" ? "Fakturujeme Vám za zboží dle dodacího listu" : "Fakturujeme Vám za zboží dle vaší objednávky");
*/



/*
-- vlozi polozky k fakturam podle cleneni DPH nove pridane fa
-- aby se propsalo do přiznání k DPH částka za zboží osvobozeno od DPH
-- a v přenesené daňové evidenci
INSERT INTO FApol (
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
        FA.ID, 'DPH21',
        'Zboží v 21% sazbě DPH', 1, 0,
        FA.DatCreate, FA.DatSave,
        2, sDPH.ID,
        FA.Kc2, FA.Kc2, FA.KcDPH2, FA.Kc2, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM FA INNER JOIN sDPH ON sDPH.IDS = 'UD' 
    WHERE FA.Cislo = '{0}' AND FA.Kc2 > 0
    UNION ALL
    -- 15% DPH
    SELECT TOP 1
        FA.ID, 'DPH15',
        'Zboží v 15% sazbě DPH', 2, 0,
        FA.DatCreate, FA.DatSave,
        1, sDPH.ID,
        FA.Kc1, FA.Kc1, FA.KcDPH1, FA.Kc1, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM FA INNER JOIN sDPH ON sDPH.IDS = 'UD' 
    WHERE FA.Cislo = '{0}' AND FA.Kc1 > 0
    UNION ALL
    -- 0% DPH
    SELECT TOP 1
        FA.ID, 'DPH0',
        'Zboží osvobozené od DPH', 3, 0,
        FA.DatCreate, FA.DatSave,
        0, sDPH.ID,
        FA.Kc0, FA.Kc0, 0, FA.Kc0, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM FA INNER JOIN sDPH ON sDPH.IDS = 'UNost' 
    WHERE FA.Cislo = '{0}' AND FA.Kc0 > 0
    UNION ALL
    -- 0% DPH prenesena danova povinnost
    SELECT TOP 1
        FA.ID, 'DPH0P',
        'Zboží v přenesené daňové povinnosti', 4, 1,
        FA.DatCreate, FA.DatSave,
        0, sDPH.ID,
        FA.KcP, FA.KcP, 0, FA.KcP, 1,
        0, 0, 0, 0, 0,
        0, 0, 0, 1,
        0, 0, 0, 0, 0
    FROM FA INNER JOIN sDPH ON sDPH.IDS = 'UDpdp' 
    WHERE FA.Cislo = '{0}' AND FA.KcP > 0
)
*/

SET NOCOUNT OFF

RETURN 0

END