drop database if exists rpg;

create database rpg;

use rpg;

create table users(
    id INT AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE NOT NULL,
    user_password VARCHAR(255) NOT NULL,
    user_name VARCHAR(30) NOT NULL,
    last_character_id INT,
    last_world_id INT,

    primary key (id)
);


create table characters(
    id INT AUTO_INCREMENT,
    user_id INT NOT NULL,
    character_name VARCHAR(30) NOT NULL,
    character_class INT NOT NULL,
    gender BOOL NOT NULL,
    character_level INT NOT NULL,
    xp INT NOT NULL,

    primary key (id),
    foreign key (user_id) references users(id)
);

create table character_stat(
    id INT AUTO_INCREMENT,
    character_id INT NOT NULL,
    hp FLOAT NOT NULL,
    atk FLOAT NOT NULL,
    matk FLOAT NOT NULL,
    def FLOAT NOT NULL,
    speed FLOAT NOT NULL,

    primary key (id),
    foreign key (character_id) references characters(id)
);

create table world(
    id INT AUTO_INCREMENT,
    world_name VARCHAR(50) NOT NULL,
    world_explan VARCHAR(255) NOT NULL,
    open_level INT NOT NULL,

    primary key (id)
);

INSERT INTO world (world_name, world_explan, open_level) VALUES (
    'forest of beginning', 'The Beginner''s Forest is the first hunting ground accessible from level 0.', 0
), (
    'Adventurer of Desert', 'The Adventurer''s Desert is a practical hunting ground accessible from level 15.', 15
), (
    'final volcano', 'The Final Volcano is the ultimate dungeon accessible from level 30.', 30
);

create table npc(
    id INT AUTO_INCREMENT,
    world_id INT NOT NULL,
    pos_x INT NOT NULL,
    pos_y INT NOT NULL,
    pos_z INT NOT NULL,
    npc_name VARCHAR(30) NOT NULL,
    reward INT NOT NULL,

    primary key(id),
    foreign key(world_id) references world(id)
);

INSERT INTO npc (world_id, pos_x, pos_y, pos_z, npc_name, reward) VALUES (
    1, 3, 0, 3, 'Jake', 1
);

create table inventory(
    id INT AUTO_INCREMENT,
    character_id INT NOT NULL,
    slot_index INT NOT NULL,
    item_code INT NOT NULL,

    primary key(id),
    foreign key(character_id) references characters(id)
)