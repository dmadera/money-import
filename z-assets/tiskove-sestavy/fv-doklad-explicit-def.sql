-- ProdejkaVydana.ID = SazDPHDokl.Parent_ID, PR-SazDPHDokl
SELECT ID, RozpisDPH_ID, Parent_ID FROM Dane_SazbaDPHNaDoklade AS SazDPHDokl WITH(NOLOCK) WHERE (Deleted = 0)
-- SazDPHDokl.RozpisDPH_ID = RozDPH.ID, SazDPHDokl-RozDPH
SELECT ID, Sazba, SumaZaklad, SumaDph, SumaCelkem FROM Dane_RozpisDPH AS RozDPH WITH(NOLOCK) WHERE (Deleted = 0)

GO
