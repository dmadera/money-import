-- PokladniDoklad.ID = VazObj.Cil_ID, PD-VazObj
SELECT Cil_ID, Zdroj_ID FROM EconomicBase_VazbaObjektu AS VazObj WITH(NOLOCK) WHERE (Deleted = 0)
-- VazObj.Zdroj_ID = DLV.ID, VazObj-DLV
SELECT ID, AdresaKontaktniOsobaNazev, CisloDokladu, SumaZaklad, SumaCelkem, DodaciAdresaNazev, DodaciAdresaMisto, Odkaz FROM SkladovyDoklad_DodaciListVydany AS DLV WITH(NOLOCK) WHERE (Deleted = 0)
-- DLV.ID = PolDLV.Parent_ID, DLV-PolDLV
SELECT Parent_ID, Nazev, Jednotka, Mnozstvi, JednCena, DphSazba, CelkovaCena, ObsahPolozky_ID, RPDP_UserData FROM SkladovyDoklad_PolozkaDodacihoListuVydaneho AS PolDLV WITH(NOLOCK) WHERE (Deleted = 0)
-- PolDLV.ObsahPolozky_ID = ObPolArt.ID, PolDLV-ObPolArt
SELECT ID, Artikl_ID FROM Obchod_ObsahPolozkySArtiklem AS ObPolArt WITH(NOLOCK) WHERE (Deleted = 0)
-- ObPolArt.Artikl_ID = Art.ID, ObPolArt-Art
SELECT ID, Kod, BaleniMnozstvi_UserData, BaleniJednotky_UserData, ProdejniJednotka_ID, Priznaky_UserData FROM Artikly_Artikl AS Art WITH(NOLOCK) WHERE (Deleted = 0)

-- PokladniDoklad.ID = SazDPHDokl.Parent_ID, PD-SazDPHDokl
SELECT ID, RozpisDPH_ID, Parent_ID FROM Dane_SazbaDPHNaDoklade AS SazDPHDokl WITH(NOLOCK) WHERE (Deleted = 0)
-- SazDPHDokl.RozpisDPH_ID = RozDPH.ID, SazDPHDokl-RozDPH
SELECT ID, Sazba, SumaZaklad, SumaDph, SumaCelkem FROM Dane_RozpisDPH AS RozDPH WITH(NOLOCK) WHERE (Deleted = 0)