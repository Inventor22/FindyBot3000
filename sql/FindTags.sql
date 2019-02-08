SELECT i.Name, i.Quantity, i.Row, i.Col, t.TagsMatched
FROM dbo.Item i JOIN
(
    SELECT Name, COUNT(Name) TagsMatched 
    FROM dbo.Tags
    WHERE Tag IN ('Battery', 'AA')
    GROUP BY Name
) t ON i.Name = t.Name
 ORDER BY t.TagsMatched DESC