USE [S4_System]
  
UPDATE MetaData_GridColumns SET
	DecimalCount = 0
FROM MetaData_GridColumns
INNER JOIN MetaData_GridTables AS T ON T.ID = Table_ID
WHERE 
	ColumnName LIKE '%mnozstvi%' 
	OR (ColumnName  LIKE '%Po.Zbyva' AND DecimalCount = 4) 
	OR ColumnName LIKE '%objednano%' 
	OR ColumnName LIKE '%rezervovano%'
-- artikl: S5ArCe.CelkoveMnozstviNaSkladech
GO
UPDATE MetaData_GridColumns SET
	DecimalCount = 2
FROM MetaData_GridColumns
INNER JOIN MetaData_GridTables AS T ON T.ID = Table_ID
WHERE ColumnName LIKE '%cena%'
GO
UPDATE MetaData_GridColumns SET
	DecimalCount = 2
FROM MetaData_GridColumns
INNER JOIN MetaData_GridTables AS T ON T.ID = Table_ID
WHERE ColumnName LIKE '%zbyva%'
GO
UPDATE MetaData_GridColumns SET
	ColumnAlignment = 1
FROM MetaData_GridColumns
INNER JOIN MetaData_GridTables AS T ON T.ID = Table_ID
WHERE 
	ColumnName LIKE '%balenimnozstvi_userdata%' 
	OR ColumnName LIKE '%balenijednotky_userdata%' 
	OR ColumnName LIKE '%sazbadph_userdata%'
	OR ColumnName LIKE '%prodej20avg_userdata%'
	OR ColumnName LIKE '%prodej20med_userdata%'
GO