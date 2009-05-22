/**/
Create Procedure SynFileItem(
@FileName	nvarchar(255),
@FileDir	nvarchar(255),
@FileSize	bingint,
@HashCode	nvarchar(50),
@BinData	image,
@FileVersion	float,
@CreateDate	datetime,
@LastChangeDate	datetime
)
AS

GO