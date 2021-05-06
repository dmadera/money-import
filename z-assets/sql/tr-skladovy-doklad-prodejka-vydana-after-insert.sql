USE [S4_Agenda_PEMA]
GO 

CREATE OR ALTER TRIGGER TR_SkladovyDoklad_ProdejkaVydana_AfterInsertUpdate
ON SkladovyDoklad_ProdejkaVydana
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

	
	UPDATE Ucetnictvi_InterniDoklad SET
		ID = ID.ID
	FROM Ucetnictvi_InterniDoklad AS ID
	INNER JOIN inserted ON inserted.CisloDokladu = ID.ParovaciSymbol

	INSERT INTO Ucetnictvi_InterniDoklad (
            Parent_ID, Root_ID, Group_ID, Locked, Cinnost_ID, 
            CleneniDPH_ID, DomaciMena_ID, Dph0Dan, Dph0DanCM, Dph0Sazba, Dph0Zaklad, Dph0ZakladCM, Dph1Celkem, Dph1CelkemCM, Dph1Dan, Dph1DanCM, 
            Dph1Sazba, Dph1Zaklad, Dph1ZakladCM, Dph2Celkem, Dph2CelkemCM, Dph2Dan, Dph2DanCM, Dph2Sazba, Dph2Zaklad, Dph2ZakladCM, 
            DruhDokladu_ID, KurzMnozstvi, Mena_ID, Poznamka, Predkontace_ID, PredkontaceZaokrouhleni_ID, RegistraceDPH_ID, 
            SazbaDPH0_ID, SazbaDPH1_ID, SazbaDPH2_ID, Schvaleno, SpecifickySymbol, Stat_ID, Stav, Storno, Stredisko_ID, SumaDan, SumaDanCM, 
            UcetDal_ID, UcetMD_ID, UcetniKurzKurz, Uhrady, UhradyCM, VariabilniSymbol, PrimarniUcetMD_ID, PrimarniUcetDal_ID,
            Zakazka_ID, Zauctovano, ZjednodusenyDanovyDoklad, UhradyZbyva, UhradyZbyvaCM, KonstantniSymbol_ID, 
            KonstantniSymbolText, TypDokladu, KUhrade, KUhradeCM, Systemovy, Zaverkovy, PocetPolozek, DatumUplatneni, CelkovaCastka, 
            CelkovaCastkaCM, ZapornyPohyb, PriznakVyrizeno,
            ZaokrouhleniCelkovaCastka_ID, ZaokrouhleniDPH_ID, ZaokrouhleniDruhSazbyDPH, ZaokrouhleniPrevazujiciSazbaDPH, MojeFirmaFirma_ID, 
            ICDPH, MojeFirmaICDPH, ObecneProdejniMisto_ID, Vyrizeno, ZpusobUplatneniOdpoctuDPH, AdresaStat_ID, 
            MojeFirmaStat_ID, ZaokrouhleniSazbaDPH_ID, PreneseniDane_ID, KombinovanaNomenklatura_ID, PreneseniDaneKombinovanaNomenklaturaKod, 
            PreneseniDanePomerMnozstviMJ, Obchodnik_ID, EETEvidovatTrzbu, PrijatyDoklad, System_Komentar, System_Priznak, 
            KorekceEditovatDomaciMenuBezPrepoctu, EkasaUctenka_ID, ZpusobVypoctuDPH, DatumSchvaleni, DatumZauctovani, Schvalil_ID, Zauctoval_ID, 
            Deleted, Faze, ParovaciSymbol, Create_ID, Create_Date, Modify_ID, Modify_Date, Vystavil,
			Nazev, Odkaz, 
			CiselnaRada_ID,
            DatumPlneni, DatumUcetnihoPripadu, DatumVystaveni, DatumPlatby,
			SumaCelkem, 
			AdresaMisto, AdresaNazev, AdresaPSC, AdresaStat, AdresaUlice,
			AdresaKontaktniOsobaJmeno, AdresaKontaktniOsobaNazev, AdresaKontaktniOsobaPrijmeni, 
			Osoba_ID, Firma_ID, IC, Jmeno, DIC
        ) SELECT TOP 1
            ID.Parent_ID, ID.Root_ID, ID.Group_ID, ID.Locked, ID.Cinnost_ID, 
            ID.CleneniDPH_ID, ID.DomaciMena_ID, ID.Dph0Dan, ID.Dph0DanCM, ID.Dph0Sazba, ID.Dph0Zaklad, ID.Dph0ZakladCM, ID.Dph1Celkem, ID.Dph1CelkemCM, ID.Dph1Dan, ID.Dph1DanCM, 
            ID.Dph1Sazba, ID.Dph1Zaklad, ID.Dph1ZakladCM, ID.Dph2Celkem, ID.Dph2CelkemCM, ID.Dph2Dan, ID.Dph2DanCM, ID.Dph2Sazba, ID.Dph2Zaklad, ID.Dph2ZakladCM, 
            ID.DruhDokladu_ID, ID.KurzMnozstvi, ID.Mena_ID, ID.Poznamka, ID.Predkontace_ID, ID.PredkontaceZaokrouhleni_ID, ID.RegistraceDPH_ID, 
            ID.SazbaDPH0_ID, ID.SazbaDPH1_ID, ID.SazbaDPH2_ID, ID.Schvaleno, ID.SpecifickySymbol, ID.Stat_ID, ID.Stav, ID.Storno, ID.Stredisko_ID, ID.SumaDan, ID.SumaDanCM, 
            ID.UcetDal_ID, ID.UcetMD_ID, ID.UcetniKurzKurz, ID.Uhrady, ID.UhradyCM, ID.VariabilniSymbol, ID.PrimarniUcetMD_ID, ID.PrimarniUcetDal_ID,
            ID.Zakazka_ID, ID.Zauctovano, ID.ZjednodusenyDanovyDoklad, ID.UhradyZbyva, ID.UhradyZbyvaCM, ID.KonstantniSymbol_ID, 
            ID.KonstantniSymbolText, ID.TypDokladu, ID.KUhrade, ID.KUhradeCM, ID.Systemovy, ID.Zaverkovy, ID.PocetPolozek, ID.DatumUplatneni, ID.CelkovaCastka, 
            ID.CelkovaCastkaCM, ID.ZapornyPohyb, ID.PriznakVyrizeno,
            ID.ZaokrouhleniCelkovaCastka_ID, ID.ZaokrouhleniDPH_ID, ID.ZaokrouhleniDruhSazbyDPH, ID.ZaokrouhleniPrevazujiciSazbaDPH, ID.MojeFirmaFirma_ID, 
			ID.ICDPH, ID.MojeFirmaICDPH, ID.ObecneProdejniMisto_ID, ID.Vyrizeno, ID.ZpusobUplatneniOdpoctuDPH, ID.AdresaStat_ID, 
            ID.MojeFirmaStat_ID, ID.ZaokrouhleniSazbaDPH_ID, ID.PreneseniDane_ID, ID.KombinovanaNomenklatura_ID, ID.PreneseniDaneKombinovanaNomenklaturaKod, 
            ID.PreneseniDanePomerMnozstviMJ, ID.Obchodnik_ID, ID.EETEvidovatTrzbu, ID.PrijatyDoklad, ID.System_Komentar, ID.System_Priznak, 
            ID.KorekceEditovatDomaciMenuBezPrepoctu, ID.EkasaUctenka_ID, ID.ZpusobVypoctuDPH, ID.DatumSchvaleni, ID.DatumZauctovani, ID.Schvalil_ID, ID.Zauctoval_ID, 
            0, 0, PR.CisloDokladu, PR.Create_ID, GETDATE(), PR.Modify_ID, GETDATE(), PR.Vystavil, 
			'Šek vydaný', '',
			(SELECT ID FROM Ciselniky_CiselnaRada AS CisRada WHERE CisRada.Kod = '_SEKY'),
            PR.DatumVystaveni, PR.DatumVystaveni, PR.DatumVystaveni, null,
			ROUND(CASE 
				WHEN PR.SumaCelkem >= 10000 THEN PR.SumaCelkem / 100 * 1.5
				WHEN PR.SumaCelkem >= 5000 THEN PR.SumaCelkem / 100 * 1
				ELSE 0
			END, 0),
			PR.AdresaMisto, PR.AdresaNazev, PR.AdresaPSC, PR.AdresaStat, PR.AdresaUlice,
			PR.AdresaKontaktniOsobaJmeno, PR.AdresaKontaktniOsobaNazev, PR.AdresaKontaktniOsobaPrijmeni, 
			PR.Osoba_ID, PR.Firma_ID, PR.IC, PR.Jmeno, PR.DIC
        FROM inserted as PR
        INNER JOIN Adresar_Firma AS Fir ON Fir.ID = PR.Firma_ID
		INNER JOIN Adresar_FirmaAdresniKlic AS FirAdrKl ON FirAdrKl.Parent_ID = PR.Firma_ID
		INNER JOIN Adresar_AdresniKlic AS AdrKlic ON AdrKlic.ID = FirAdrKl.AdresniKlic_ID
		LEFT JOIN Ucetnictvi_InterniDoklad AS IDa ON IDa.ParovaciSymbol = PR.CisloDokladu
        LEFT JOIN Ucetnictvi_InterniDoklad AS ID ON ID.CisloDokladu = '_SK000000'
        WHERE AdrKlic.Kod != '-SEK' AND PR.SumaCelkem >= 5000 AND (Fir.VlastniSleva = 0 OR Fir.HodnotaSlevy = 0) AND IDa.ID IS NULL

	SET NOCOUNT OFF;
END