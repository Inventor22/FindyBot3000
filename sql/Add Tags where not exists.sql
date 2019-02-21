MERGE INTO dbo.Tags AS Target
USING(VALUES (@param1, @param2),(@param1, @param3)) AS Source (Name, Tag)
ON Target.Name = Source.Name AND Target.Tag = Source.Tag
WHEN NOT MATCHED BY Target THEN
INSERT(Name, Tag) VALUES(Source.Name, Source.Tag);