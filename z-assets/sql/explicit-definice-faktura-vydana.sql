-- FakturaVydana.ID = Vazba.Cil_ID
SELECT Cil_ID, Zdroj_ID FROM EconomicBase_VazbaObjektu AS VazObj WITH(NOLOCK) WHERE (Deleted = 0)
-- VazObj.Zdroj_ID = DLV.ID
SELECT ID, AdresaKontaktniOsobaNazev, CisloDokladu, SumaZaklad, SumaCelkem, DodaciAdresaNazev, DodaciAdresaMisto, Odkaz FROM SkladovyDoklad_DodaciListVydany AS DLV WITH(NOLOCK) WHERE (Deleted = 0)
-- DLV.ID = PolDLV.Parent_ID
SELECT Parent_ID, Nazev, Jednotka, Mnozstvi, JednCena, DphSazba, CelkovaCena, ObsahPolozky_ID, RPDP_UserData FROM SkladovyDoklad_PolozkaDodacihoListuVydaneho AS PolDLV WITH(NOLOCK) WHERE (Deleted = 0)
-- PolDLV.ObsahPolozky_ID = ObPolArt.ID
SELECT ID, Artikl_ID FROM Obchod_ObsahPolozkySArtiklem AS ObPolArt WITH(NOLOCK) WHERE (Deleted = 0)
-- ObPolArt.Artikl_ID = Art.ID
SELECT ID, Kod, ProdJednotkaMnozstvi_UserData, ProdejniJednotka_ID, Priznaky_UserData FROM Artikly_Artikl AS Art WITH(NOLOCK) WHERE (Deleted = 0)
-- Art.ProdejniJednotka_ID = ArtJed.Parent_ID
SELECT ID, Kod FROM Artikly_ArtiklJednotka AS ArtJed WITH(NOLOCK) WHERE (Deleted = 0)

-- FakturaVydana.ID = SazDPHDokl.Parent_ID
SELECT ID, RozpisDPH_ID, Parent_ID FROM Dane_SazbaDPHNaDoklade AS SazDPHDokl WITH(NOLOCK) WHERE (Deleted = 0)
-- SazDPHDokl.RozpisDPH_ID = RozDPH.ID
SELECT ID, Sazba, SumaZaklad, SumaDph, SumaCelkem FROM Dane_RozpisDPH AS RozDPH WITH(NOLOCK) WHERE (Deleted = 0)
GO