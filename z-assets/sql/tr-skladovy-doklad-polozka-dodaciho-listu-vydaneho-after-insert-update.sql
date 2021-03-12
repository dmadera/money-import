USE [S4_Agenda_PEMA]
GO

CREATE OR ALTER TRIGGER TR_SkladovyDoklad_PolozkaDodacihoListuVydaneho_AfterInsert
ON SkladovyDoklad_PolozkaDodacihoListuVydaneho
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
	
	/* 
		MnozstviPozn_UserData:
		* poslední kus
		- odbìr do mínusu
	*/
	UPDATE SkladovyDoklad_PolozkaDodacihoListuVydaneho SET
		MnozstviPozn_UserData = (CASE
			WHEN Zas.DostupneMnozstvi = 0 THEN '*'
			WHEN Zas.DostupneMnozstvi < 0 THEN '-'
			ELSE ''
		END)
	FROM SkladovyDoklad_PolozkaDodacihoListuVydaneho AS Pol
	INNER JOIN inserted ON inserted.ID = Pol.ID
	INNER JOIN Obchod_ObsahPolozkySArtiklem AS ObP ON ObP.ID = Pol.ObsahPolozky_ID
	INNER JOIN Sklady_Zasoba AS Zas ON Zas.ID = ObP.Zasoba_ID

	SET NOCOUNT OFF;
END