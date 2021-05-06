USE [S4_Agenda_PEMA]
GO 

CREATE OR ALTER TRIGGER TR_SkladovyDoklad_ProdejkaVydana_AfterUpdate
ON SkladovyDoklad_ProdejkaVydana
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	-- aktualizovat finance-pokladni-doklad aby se spustil trigger na pokladni doklady (seky)
	UPDATE Ucetnictvi_InterniDoklad SET 
		ID = PD.ID
	FROM Ucetnictvi_InterniDoklad AS PD
	INNER JOIN inserted AS PR ON PD.ParovaciSymbol = PR.CisloDokladu
	SET NOCOUNT OFF;
END
