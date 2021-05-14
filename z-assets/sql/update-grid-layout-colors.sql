USE S4_System;
GO
IF NOT EXISTS(SELECT TOP 1 ID FROM MetaData_GridColors WHERE Name = 'Z�soba je objedn�na')
	INSERT INTO MetaData_GridColors 
		(Object_ID, Name, IsUser, IsGenerated)
		SELECT TOP 1 Obj.ID, 'Z�soba je objedn�na', 1, 0
		FROM MetaData_Objects AS Obj
		WHERE Obj.ObjectName = 'Zasoba';

UPDATE MetaData_GridColors SET
	Condition = '([Objednano] > 0)',
	Priority = 10,
	BackColor = -1, 
	FontColor = -16748352, 
	FontStyle = 0
	WHERE Name = 'Z�soba je objedn�na'	

IF NOT EXISTS(SELECT TOP 1 ID FROM MetaData_GridColors WHERE Name = 'Z�soba je pod minimem')
	INSERT INTO MetaData_GridColors 
		(Object_ID, Name, IsUser, IsGenerated)
		SELECT TOP 1 Obj.ID, 'Z�soba je pod minimem', 1, 0
		FROM MetaData_Objects AS Obj
		WHERE Obj.ObjectName = 'Zasoba';

UPDATE MetaData_GridColors SET
	Condition = '([ProdejMinMED_UserData] > [ZustatekMnozstvi] + [Objednano])',
	Priority = 100,
	BackColor = -1, 
	FontColor = 16711935, 
	FontStyle = 0
	WHERE Name = 'Z�soba je pod minimem'

IF NOT EXISTS(SELECT TOP 1 ID FROM MetaData_GridColors WHERE Name = 'Nekontrolovat minimum')
	INSERT INTO MetaData_GridColors 
		(Object_ID, Name, IsUser, IsGenerated)
		SELECT TOP 1 Obj.ID, 'Nekontrolovat minimum', 1, 0
		FROM MetaData_Objects AS Obj
		WHERE Obj.ObjectName = 'Zasoba';

UPDATE MetaData_GridColors SET
	Condition = '([ProdejMinMED_UserData] = -1)',
	Priority = 100,
	BackColor = -1, 
	FontColor = -8355712, 
	FontStyle = 0
	WHERE Name = 'Nekontrolovat minimum'