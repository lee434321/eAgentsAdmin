/*
  pm_lease_number   in char,
  pm_lease_version  in char,
  pm_lease_termfrom in date,
  pm_lease_termto   in date,
  pm_asofdate       in date,
  rental_level    in char,  -- L: Lease; F: Floor; U: Unit
  pm_floor_code   in char,
  pm_unit_code    in char
*/

select Get_MonthlyFace_Rent(lease_number,lease_version
,lease_term_from
,lease_term_to
,sysdate,'L','','') as MonthlyRentalAmt
,lease_number
,lease_term_from
,lease_term_to
from lm_lease l
where 1=1
  and lease_number='CVCT00155' -- WG0300139 ACBV00189  CVCT00155 2015-02-16
  and status='A' and active='A'
--   and l.customer_number='E000750'
;

select * from ar_statement_detail asd 
where 1=1
  and asd.transaction_type=3
  and asd.statement_day >=to_date('2017-12-01','yyyy-mm-dd')
--  and asd.customer_number='E00075001'
  and asd.lease_number='CVCT00155' 
  

--select * from vw_lm_lease_charge_item

