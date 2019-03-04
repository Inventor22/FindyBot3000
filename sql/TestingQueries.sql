--SELECT * FROM dbo.Items
--SELECT * FROM dbo.Tags

SELECT Name,Quantity,Row,Col FROM dbo.Items WHERE Items.NameKey LIKE 'aa battery'

--update dbo.Tags
--set NameKey = LOWER(Name)


--update dbo.Items
--set NameKey = LOWER(Name)