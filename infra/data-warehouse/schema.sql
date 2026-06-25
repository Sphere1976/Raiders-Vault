create schema if not exists raiders_vault;

create table if not exists raiders_vault.fact_audit_event (
    audit_event_id varchar(64) primary key,
    occurred_at timestamp not null,
    actor varchar(120) not null,
    area varchar(120) not null,
    event_type varchar(120) not null,
    severity varchar(40) not null
);

create table if not exists raiders_vault.fact_inventory_gap (
    inventory_item_id varchar(64) not null,
    snapshot_date date not null,
    item_name varchar(160) not null,
    rarity varchar(40) not null,
    keep_target integer not null,
    current_count integer not null,
    needed_count integer not null,
    primary key (inventory_item_id, snapshot_date)
);

create table if not exists raiders_vault.fact_live_condition (
    condition_id varchar(120) not null,
    snapshot_at timestamp not null,
    map_name varchar(120) not null,
    condition_name varchar(120) not null,
    status varchar(60) not null,
    primary key (condition_id, snapshot_at)
);
