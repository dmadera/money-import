-- FakturaVydana.ID = VazObj.Cil_ID, FV-VazObj
SELECT Cil_ID, Zdroj_ID FROM EconomicBase_VazbaObjektu AS VazObj WITH(NOLOCK) WHERE (Deleted = 0)
-- VazObj.Zdroj_ID = DLV.ID, VazObj-DLV
SELECT ID, AdresaKontaktniOsobaNazev, CisloDokladu, SumaZaklad, SumaCelkem, DodaciAdresaNazev, DodaciAdresaMisto, Odkaz FROM SkladovyDoklad_DodaciListVydany AS DLV WITH(NOLOCK) WHERE (Deleted = 0)
-- DLV.ID = PolDLV.Parent_ID, DLV-PolDLV
SELECT Parent_ID, Nazev, Jednotka, Mnozstvi, JednCena, DphSazba, CelkovaCena, ObsahPolozky_ID, RPDP_UserData FROM SkladovyDoklad_PolozkaDodacihoListuVydaneho AS PolDLV WITH(NOLOCK) WHERE (Deleted = 0)
-- PolDLV.ObsahPolozky_ID = ObPolArt.ID, PolDLV-ObPolArt
SELECT ID, Artikl_ID FROM Obchod_ObsahPolozkySArtiklem AS ObPolArt WITH(NOLOCK) WHERE (Deleted = 0)
-- ObPolArt.Artikl_ID = Art.ID, ObPolArt-Art
SELECT ID, Kod, BaleniMnozstvi_UserData, BaleniJednotky_UserData, ProdejniJednotka_ID, Priznaky_UserData FROM Artikly_Artikl AS Art WITH(NOLOCK) WHERE (Deleted = 0)
-- Art.ProdejniJednotka_ID = ArtJed.Parent_ID, Art-ArtJed
SELECT ID, Kod, Parent_ID FROM Artikly_ArtiklJednotka AS ArtJed WITH(NOLOCK) WHERE (Deleted = 0)

-- FakturaVydana.ID = SazDPHDokl.Parent_ID, FV-SazDPHDokl
SELECT ID, RozpisDPH_ID, Parent_ID FROM Dane_SazbaDPHNaDoklade AS SazDPHDokl WITH(NOLOCK) WHERE (Deleted = 0)
-- SazDPHDokl.RozpisDPH_ID = RozDPH.ID, SazDPHDokl-RozDPH
SELECT ID, Sazba, SumaZaklad, SumaDph, SumaCelkem FROM Dane_RozpisDPH AS RozDPH WITH(NOLOCK) WHERE (Deleted = 0)

-- FakturaVydana.ID = VazObj.Zdroj_ID, FV-VazObj1
SELECT Cil_ID, Zdroj_ID FROM EconomicBase_VazbaObjektu AS VazObj1 WITH(NOLOCK) WHERE (Deleted = 0)
-- VazObj.Cil_ID = PoklDokl.ID, VazObj1-PD
SELECT ID, CelkovaCastka, DatumPlatby FROM Finance_PokladniDoklad AS PD WITH(NOLOCK) WHERE (Deleted = 0)

-- FakturaVydana.ID = FV.ID, FakVyd-FV
SELECT ID, Group_ID FROM Fakturace_FakturaVydana AS FV WITH(NOLOCK) WHERE (Deleted = 0)
-- FV.Group_ID = Grp.ID, FV-Grp
SELECT ID, Kod, Nazev FROM System_Groups AS Grp WITH(NOLOCK)

GO
