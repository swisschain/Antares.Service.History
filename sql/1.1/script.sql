alter table orders alter column volume type numeric;
alter table orders alter column price type numeric;
alter table orders alter column remaining_volume type numeric;
alter table orders alter column lower_limit_price type numeric;
alter table orders alter column lower_price type numeric;
alter table orders alter column upper_limit_price type numeric;
alter table orders alter column upper_price type numeric;

alter table history alter column volume type numeric;