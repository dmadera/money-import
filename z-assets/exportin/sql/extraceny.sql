SET NOCOUNT ON

USE S4_Agenda_PEMA
GO

SELECT
	Cenik.Kod AS Skupina,
	SUBSTRING(C.Kod, 4, 100) AS ID,
	FORMAT(C.Cena, '0.00') AS Cena,
	''
FROM Ceniky_PolozkaCeniku AS C
INNER JOIN Ceniky_Cenik AS Cenik ON Cenik.ID = C.Cenik_ID AND Cenik.Kod NOT LIKE '\_%' ESCAPE '\'
WHERE 
	1=1
ORDER BY ID