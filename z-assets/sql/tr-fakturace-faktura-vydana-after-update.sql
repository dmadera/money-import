USE [S4_Agenda_PEMA]
GO 

CREATE OR ALTER TRIGGER TR_Fakturace_FakturaVydana_AfterUpdate
ON Fakturace_FakturaVydana
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	-- aktualizovat finance-pokladni-doklad aby se spustil trigger na pokladni doklady (seky)
	UPDATE Finance_PokladniDoklad SET 
		ID = PD.ID
	FROM Finance_PokladniDoklad AS PD
	INNER JOIN inserted AS FV ON PD.ParovaciSymbol = FV.CisloDokladu
	SET NOCOUNT OFF;
END