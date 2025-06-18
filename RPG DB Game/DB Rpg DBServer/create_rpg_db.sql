use mysql;

DROP USER IF EXISTS 'rpg'@'localhost';
create user 'rpg'@'localhost' identified by 'rpg';

select User,Host from user where User='rpg';

flush privileges;

grant all on rpg.* to 'rpg'@'localhost';

flush privileges;

select '' as 'show newly created databases';
show databases;