--liquibase formatted sql

--changeset uamangeldiev:10
create table public.background_tasks
(
    id             bigint,
    name           varchar(255),
    service_id     bigint    null,
    expire_date    timestamp null,
    is_locked      bool default false,
    lock_timestamp timestamp null,
    is_done        bool default true,

    constraint pk_tasks_id primary key (id)
);
--rollback drop table public.background_tasks;

--changeset uamangeldiev:20
insert into public.background_tasks (id, name)
values (1, 'Обновить статус финмониторинга пользователей после обновления черных списков');
--rollback delete from public.background_tasks where id = 1;