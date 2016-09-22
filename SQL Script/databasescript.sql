IF OBJECT_ID('UserInfo', 'U') IS NOT NULL DROP TABLE UserInfo;

CREATE TABLE [dbo].[UserInfo](
	[UserId] [varchar](50) NOT NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[PhoneNumber] [varchar](50) NULL,
	[Entity] [varchar](50) NULL,
	[EntityType] [varchar](50) NULL,
	[Status] [varchar](50) NULL,
	[Value] [money] NOT NULL,
	[AvailableDate] [date] NOT NULL,
	PRIMARY KEY (UserId) 
)
GO

INSERT dbo.UserInfo VALUES
('56788','Miles','Rogerson','6170009111','Business Claim','Claim','Approved',8000.00, '2016-07-01'),
('56789','Major','Haywood','9780001111','Student Grant','Grant','In Progress',0.00, '2016-07-02'),
('56790','Winston','Easton','7819807891','Claim','Claim','Approved',10000.00,'2016-07-01'),
('56791','Mikki','Tyrrell','8045412341','Student Grant','Grant','Denied',0.00,'2016-07-05')
GO
