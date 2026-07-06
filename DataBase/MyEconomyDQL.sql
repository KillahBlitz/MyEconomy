use MyEconomy
go

insert into Usuarios values('JMONROY', 'Olakase2')

insert into Categoria values
('OTROS')
('RENTA'),
('DESPENSA')

insert into Saldos values
('tonteria de SWITCH', 179.00, '2026-07-05', 1, 8),
('restante tarjeta', 5640.00, '2026-07-05', 1, null),
('dinero de renta', 5500.00, '2026-07-05', 1, null),
('pago julio RENTA', 5500.00, '2026-07-05', 1, 1),
('restante efectivo', 2500.00, '2026-07-05', 1, null)


INSERT INTO Deudas (id_usuario, tipo_deuda, descripcion, cantidad, fcha_registro)
VALUES
(1, 1, 'Deuda en cuenta COPPEL', 30187.37, '2026-07-04'),
(1, 0, 'Deuda cubierta', 26216.30, '2026-07-04');

select * from Deudas
select * from Usuarios
select * from Saldos
select * from Categoria