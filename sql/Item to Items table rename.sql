INSERT INTO dbo.Items (Name, Quantity, Row, Col, IsSmallBox, DateCreated, LastUpdated)
SELECT Name, Quantity, Row, Col, SmallBox, DateCreated, LastUpdated
FROM dbo.Item