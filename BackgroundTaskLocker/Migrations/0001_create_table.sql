--liquibase formatted sql

--changeset uamangeldiev:10
create table public.background_tasks
(
    id             bigint,
    service_id     bigint null,
    expire_date    timestamp null,
    is_locked      bool default false,
    lock_timestamp timestamp null,

    constraint pk_tasks_id primary key (id)
);
--rollback drop table public.background_tasks;

--changeset uamangeldiev:20
insert into public.background_tasks (id)
values (1);
--rollback delete from public.background_tasks where id = 1;