USE [S4_Agenda_PEMA]
GO 

CREATE OR ALTER TRIGGER TR_Fakturace_FakturaVydana_AfterUpdate
ON Fakturace_FakturaVydana
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	UPDATE Finance_PokladniDoklad SET 
		Faze = 0,
		SumaCelkem = ROUND(CASE 
				WHEN FV.SumaCelkem >= 10000 THEN FV.SumaCelkem / 100 * 1.5
				WHEN FV.SumaCelkem >= 5000 THEN FV.SumaCelkem / 100 * 1
			END, 0)
	FROM Finance_PokladniDoklad AS PD
	INNER JOIN inserted AS FV ON PD.ParovaciSymbol = FV.CisloDokladu
	SET NOCOUNT OFF;
END