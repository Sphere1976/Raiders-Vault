create or replace view raiders_vault.vw_inventory_readiness as
select
    snapshot_date,
    rarity,
    count(*) as tracked_items,
    sum(case when needed_count = 0 then 1 else 0 end) as stocked_items,
    sum(needed_count) as total_needed
from raiders_vault.fact_inventory_gap
group by snapshot_date, rarity;

create or replace view raiders_vault.vw_audit_activity_daily as
select
    cast(occurred_at as date) as activity_date,
    area,
    event_type,
    severity,
    count(*) as event_count
from raiders_vault.fact_audit_event
group by cast(occurred_at as date), area, event_type, severity;

create or replace view raiders_vault.vw_live_condition_frequency as
select
    map_name,
    condition_name,
    count(*) as observed_count,
    max(snapshot_at) as last_observed_at
from raiders_vault.fact_live_condition
group by map_name, condition_name;
