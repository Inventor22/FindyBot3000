SELECT COUNT(Name), Tag FROM dbo.Tags WHERE Tag IN ('Battery', 'AA')

SELECT Tags.Name, COUNT(Tags.Name)
FROM dbo.Tags
LEFT JOIN dbo.Item ON Item.Name = Tags.Name
WHERE Tag IN ('Battery', 'AA')
GROUP BY Tags.Name