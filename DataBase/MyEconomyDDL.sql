CREATE DATABASE MyEconomy
    ON
    PRIMARY (
    NAME = MyEconomy_data,
    FILENAME = '/Users/jmonroy/Downloads/databases/MyEconomy_data.mdf',
    SIZE = 10MB,
    MAXSIZE = 500MB,
    FILEGROWTH = 5MB
    )
    LOG ON (
    NAME = MyEconomy_log,
    FILENAME = '/Users/jmonroy/Downloads/databases/MyEconomy_log.ldf',
    SIZE = 5MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
    );
GO

use MyEconomy
go


create table Usuarios(
    user_id smallint identity(1,1) primary key,
    nombre_user varchar(20) not null,
    password varchar(10) not null,
)

create table Categoria(
    categoria_id smallint identity(1,1) primary key,
    tipo_categoria varchar(10) not null
)

create table Saldos(
    saldo_id integer identity(1,1) primary key,
    concepto varchar(50) not null,
    cantidad decimal(10,2) not null,
    fcha_registro date not null,
    id_usuario smallint not null,
    id_categoria smallint,
    constraint "fk_id_categoria" foreign key(id_categoria)
    references Categoria(categoria_id),
    constraint "fk_id_user" foreign key(id_usuario)
    references Usuarios(user_id)
)

create table Tickets(
    ticket_id integer identity (1,1) primary key,
    fcha_emision date not null,
    cuenta_total decimal(8,2) not null,
    id_usuario smallint not null,
    constraint "fk_id_user_0" foreign key(id_usuario)
    references Usuarios(user_id)
)

create table Productos(
    producto_id integer identity(1,1) primary key,
    descripcion varchar(30) not null,
    cantindad smallint not null,
    precio_unitaro smallint not null,
    id_ticket integer not null,
    constraint "fk_ticket_id" foreign key(id_ticket)
    references Tickets(ticket_id)
)


create table Deudas(
    deuda_id integer not null,
    id_usuario smallint not null,
    tipo_deuda bit not null,
    descripcion varchar(100) not null,
    cantidad decimal(10,2) not null,
    fcha_registro date not null,
    constraint "fk_id_user_1" foreign key(id_usuario)
    references Usuarios(user_id)
)