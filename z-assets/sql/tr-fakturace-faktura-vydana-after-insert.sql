USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_SkladovyDoklad_DodaciListVydany_AfterInsertUpdate
ON SkladovyDoklad_DodaciListVydany
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT * FROM DELETED) BEGIN
		UPDATE Finance_PokladniDoklad SET Finance_PokladniDoklad.Faze = 0 WHERE ParovaciSymbol IN (SELECT cisloDokladu FROM DELETED)
	END

	INSERT INTO Finance_PokladniDoklad (
            Parent_ID, Root_ID, Group_ID, Locked, Cinnost_ID, 
            CiselnaRada_ID, CleneniDPH_ID, DomaciMena_ID, Dph0Dan, Dph0DanCM, Dph0Sazba, Dph0Zaklad, Dph0ZakladCM, Dph1Celkem, Dph1CelkemCM, Dph1Dan, Dph1DanCM, 
            Dph1Sazba, Dph1Zaklad, Dph1ZakladCM, Dph2Celkem, Dph2CelkemCM, Dph2Dan, Dph2DanCM, Dph2Sazba, Dph2Zaklad, Dph2ZakladCM, 
            DruhDokladu_ID, Korekce0Celkem, Korekce0CelkemCM, Korekce0Dan, Korekce0DanCM, Korekce0Sazba, 
            Korekce0Zaklad, Korekce0ZakladCM, Korekce1Celkem, Korekce1CelkemCM, Korekce1Dan, Korekce1DanCM, Korekce1Sazba, Korekce1Zaklad, 
            Korekce1ZakladCM, Korekce2Celkem, Korekce2CelkemCM, Korekce2Dan, Korekce2DanCM, Korekce2Sazba, Korekce2Zaklad, Korekce2ZakladCM, 
            KurzMnozstvi, Mena_ID, Nazev, Odkaz, Pokladna_ID, Poznamka, Predkontace_ID, PredkontaceZaokrouhleni_ID, RegistraceDPH_ID, 
            SazbaDPH0_ID, SazbaDPH1_ID, SazbaDPH2_ID, Schvaleno, SpecifickySymbol, Stat_ID, Stav, Storno, Stredisko_ID, SumaDan, SumaDanCM, 
            UcetDal_ID, UcetMD_ID, UcetniKurzKurz, Uhrady, UhradyCM, VariabilniSymbol, 
            Zakazka_ID, Zauctovano, ZjednodusenyDanovyDoklad, DatumPlatby, UhradyZbyva, UhradyZbyvaCM, KonstantniSymbol_ID, 
            KonstantniSymbolText, TypDokladu, KUhrade, KUhradeCM, Systemovy, Zaverkovy, PocetPolozek, DatumUplatneni, CelkovaCastka, 
            CelkovaCastkaCM, ZapornyPohyb, Hidden, PriznakVyrizeno, MojeFirma_ID, MojeFirmaBankovniSpojeni_ID, MojeFirmaBankovniSpojeniCisloUctu, 
            --MojeFirmaBankovniSpojeniIBAN, MojeFirmaBankovniSpojeniKodBanky, MojeFirmaBankovniSpojeniSpecifickySymbol, MojeFirmaDIC, MojeFirmaIC, 
            --MojeFirmaKontaktniOsoba_ID, MojeFirmaKontaktniOsobaJmeno, MojeFirmaKontaktniOsobaNazev, MojeFirmaKontaktniOsobaPrijmeni, 
            --MojeFirmaMisto, MojeFirmaNazev, MojeFirmaPSC, MojeFirmaStat, MojeFirmaUlice, MojeFirmaBanka_ID, AdresaKontaktniOsoba_ID, 
            AdresaKontaktniOsobaJmeno, AdresaKontaktniOsobaNazev, AdresaKontaktniOsobaPrijmeni, MojeFirmaBankovniSpojeniSWIFT, MojeFirmaKontaktyEmail, 
            MojeFirmaKontaktyTelefon1, MojeFirmaKontaktyTelefon2, MojeFirmaKontaktyTelefon3, MojeFirmaKontaktyWWW, VygenerovanPrecenenim, 
            ZaokrouhleniCelkovaCastka_ID, ZaokrouhleniDPH_ID, ZaokrouhleniDruhSazbyDPH, ZaokrouhleniPrevazujiciSazbaDPH, MojeFirmaFirma_ID, 
            PrimarniUcet_ID, ICDPH, MojeFirmaICDPH, Attachments, ObecneProdejniMisto_ID, Vyrizeno, ZpusobUplatneniOdpoctuDPH, AdresaStat_ID, 
            MojeFirmaStat_ID, ZaokrouhleniSazbaDPH_ID, PreneseniDane_ID, KombinovanaNomenklatura_ID, PreneseniDaneKombinovanaNomenklaturaKod, 
            PreneseniDanePomerMnozstviMJ, Obchodnik_ID, EETEvidovatTrzbu, PrijatyDoklad, System_Komentar, System_Priznak, 
            KorekceEditovatDomaciMenuBezPrepoctu, EkasaUctenka_ID, ZpusobVypoctuDPH, DatumSchvaleni, DatumZauctovani, Schvalil_ID, Zauctoval_ID, 
            Deleted, Faze, ParovaciSymbol, Create_ID, Create_Date, Modify_ID, Modify_Date, Vystavil,
            DatumPlneni, DatumUcetnihoPripadu, DatumVystaveni,
			SumaCelkem, 
			AdresaMisto, AdresaNazev, AdresaPSC, AdresaStat, AdresaUlice,
			Osoba_ID, Firma_ID, IC, Jmeno, DIC
        ) SELECT TOP 1
            PD.Parent_ID, PD.Root_ID, PD.Group_ID, PD.Locked, PD.Cinnost_ID, 
            PD.CiselnaRada_ID, PD.CleneniDPH_ID, PD.DomaciMena_ID, PD.Dph0Dan, PD.Dph0DanCM, PD.Dph0Sazba, PD.Dph0Zaklad, PD.Dph0ZakladCM, PD.Dph1Celkem, PD.Dph1CelkemCM, PD.Dph1Dan, PD.Dph1DanCM, 
            PD.Dph1Sazba, PD.Dph1Zaklad, PD.Dph1ZakladCM, PD.Dph2Celkem, PD.Dph2CelkemCM, PD.Dph2Dan, PD.Dph2DanCM, PD.Dph2Sazba, PD.Dph2Zaklad, PD.Dph2ZakladCM, 
            PD.DruhDokladu_ID, PD.Korekce0Celkem, PD.Korekce0CelkemCM, PD.Korekce0Dan, PD.Korekce0DanCM, PD.Korekce0Sazba, 
            PD.Korekce0Zaklad, PD.Korekce0ZakladCM, PD.Korekce1Celkem, PD.Korekce1CelkemCM, PD.Korekce1Dan, PD.Korekce1DanCM, PD.Korekce1Sazba, PD.Korekce1Zaklad, 
            PD.Korekce1ZakladCM, PD.Korekce2Celkem, PD.Korekce2CelkemCM, PD.Korekce2Dan, PD.Korekce2DanCM, PD.Korekce2Sazba, PD.Korekce2Zaklad, PD.Korekce2ZakladCM, 
            PD.KurzMnozstvi, PD.Mena_ID, PD.Nazev, PD.Odkaz, PD.Pokladna_ID, PD.Poznamka, PD.Predkontace_ID, PD.PredkontaceZaokrouhleni_ID, PD.RegistraceDPH_ID, 
            PD.SazbaDPH0_ID, PD.SazbaDPH1_ID, PD.SazbaDPH2_ID, PD.Schvaleno, PD.SpecifickySymbol, PD.Stat_ID, PD.Stav, PD.Storno, PD.Stredisko_ID, PD.SumaDan, PD.SumaDanCM, 
            PD.UcetDal_ID, PD.UcetMD_ID, PD.UcetniKurzKurz, PD.Uhrady, PD.UhradyCM, PD.VariabilniSymbol, 
            PD.Zakazka_ID, PD.Zauctovano, PD.ZjednodusenyDanovyDoklad, PD.DatumPlatby, PD.UhradyZbyva, PD.UhradyZbyvaCM, PD.KonstantniSymbol_ID, 
            PD.KonstantniSymbolText, PD.TypDokladu, PD.KUhrade, PD.KUhradeCM, PD.Systemovy, PD.Zaverkovy, PD.PocetPolozek, PD.DatumUplatneni, PD.CelkovaCastka, 
            PD.CelkovaCastkaCM, PD.ZapornyPohyb, PD.Hidden, PD.PriznakVyrizeno, PD.MojeFirma_ID, PD.MojeFirmaBankovniSpojeni_ID, PD.MojeFirmaBankovniSpojeniCisloUctu, 
            --PD.MojeFirmaBankovniSpojeniIBAN, PD.MojeFirmaBankovniSpojeniKodBanky, PD.MojeFirmaBankovniSpojeniSpecifickySymbol, PD.MojeFirmaDIC, PD.MojeFirmaIC, 
            --PD.MojeFirmaKontaktniOsoba_ID, PD.MojeFirmaKontaktniOsobaJmeno, PD.MojeFirmaKontaktniOsobaNazev, PD.MojeFirmaKontaktniOsobaPrijmeni, 
            --PD.MojeFirmaMisto, PD.MojeFirmaNazev, PD.MojeFirmaPSC, PD.MojeFirmaStat, PD.MojeFirmaUlice, PD.MojeFirmaBanka_ID, PD.AdresaKontaktniOsoba_ID, 
            PD.AdresaKontaktniOsobaJmeno, PD.AdresaKontaktniOsobaNazev, PD.AdresaKontaktniOsobaPrijmeni, PD.MojeFirmaBankovniSpojeniSWIFT, PD.MojeFirmaKontaktyEmail, 
            PD.MojeFirmaKontaktyTelefon1, PD.MojeFirmaKontaktyTelefon2, PD.MojeFirmaKontaktyTelefon3, PD.MojeFirmaKontaktyWWW, PD.VygenerovanPrecenenim, 
            PD.ZaokrouhleniCelkovaCastka_ID, PD.ZaokrouhleniDPH_ID, PD.ZaokrouhleniDruhSazbyDPH, PD.ZaokrouhleniPrevazujiciSazbaDPH, PD.MojeFirmaFirma_ID, 
            PD.PrimarniUcet_ID, PD.ICDPH, PD.MojeFirmaICDPH, PD.Attachments, PD.ObecneProdejniMisto_ID, PD.Vyrizeno, PD.ZpusobUplatneniOdpoctuDPH, PD.AdresaStat_ID, 
            PD.MojeFirmaStat_ID, PD.ZaokrouhleniSazbaDPH_ID, PD.PreneseniDane_ID, PD.KombinovanaNomenklatura_ID, PD.PreneseniDaneKombinovanaNomenklaturaKod, 
            PD.PreneseniDanePomerMnozstviMJ, PD.Obchodnik_ID, PD.EETEvidovatTrzbu, PD.PrijatyDoklad, PD.System_Komentar, PD.System_Priznak, 
            PD.KorekceEditovatDomaciMenuBezPrepoctu, PD.EkasaUctenka_ID, PD.ZpusobVypoctuDPH, PD.DatumSchvaleni, PD.DatumZauctovani, PD.Schvalil_ID, PD.Zauctoval_ID, 
            0, IIF(FV.Deleted = 1, 1, 0), FV.CisloDokladu, FV.Create_ID, GETDATE(), FV.Modify_ID, GETDATE(), FV.Vystavil, 
            FV.DatumPlneni, FV.DatumUcetnihoPripadu, FV.DatumVystaveni, 
			ROUND(CASE 
				WHEN FV.SumaCelkem >= 10000 THEN FV.SumaCelkem / 100 * 1.5
				WHEN FV.SumaCelkem >= 5000 THEN FV.SumaCelkem / 100 * 1
			END, 0),
			FV.AdresaMisto, FV.AdresaNazev, FV.AdresaPSC, FV.AdresaStat, FV.AdresaUlice,
			FV.Osoba_ID, FV.Firma_ID, FV.IC, FV.Jmeno, FV.DIC
        FROM Fakturace_FakturaVydana as FV
		INNER JOIN Adresar_FirmaAdresniKlic AS FirAdrKl ON FirAdrKl.Parent_ID = FV.Firma_ID
		INNER JOIN Adresar_AdresniKlic AS AdrKlic ON AdrKlic.ID = FirAdrKl.AdresniKlic_ID
        LEFT JOIN Finance_PokladniDoklad AS PD ON PD.ParovaciSymbol = 'FF0000000'
        WHERE AdrKlic.Kod != '-SEK' AND FV.SumaCelkem >= 5000
		AND FV.CisloDokladu = 'FV2100022'

	SET NOCOUNT OFF;
END