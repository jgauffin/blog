create table Users
(
	Id int not null identity primary key,
	UserName varchar(40) not null,
	FirstName varchar(40) not null,
	LastName varchar(40) not null
);

create table Posts
(
	Id int not null identity primary key,
	Title varchar(40) not null
);

insert into Users (UserName, FirstName, LastName) VALUES('jgauffin', 'Jonas', 'Gauffin');
insert into Posts (Title) VALUES('Building an ORM');
