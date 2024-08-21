CREATE NONCLUSTERED INDEX [_dta_index_ADDRESS_DEFAULT_GEOCODE{1}] ON [{0}].[ADDRESS_DEFAULT_GEOCODE{1}] ([address_detail_pid] ASC)
	WITH (
			SORT_IN_TEMPDB = OFF
			,DROP_EXISTING = OFF
			,ONLINE = OFF
			) ON [PRIMARY];

CREATE STATISTICS [_dta_stat_1] ON [{0}].[LOCALITY{1}] (
	[locality_pid]
	,[state_pid]
	);

CREATE STATISTICS [_dta_stat_2] ON [{0}].[LOCALITY{1}] (
	[locality_name]
	,[locality_pid]
	);

CREATE STATISTICS [_dta_stat_3] ON [{0}].[STREET_LOCALITY{1}] (
	[street_name]
	,[street_locality_pid]
	);

CREATE STATISTICS [_dta_stat_4] ON [{0}].[ADDRESS_DETAIL{1}] (
	[confidence]
	,[address_detail_pid]
	);

CREATE STATISTICS [_dta_stat_5] ON [{0}].[ADDRESS_DETAIL{1}] (
	[level_type_code]
	,[confidence]
	,[address_detail_pid]
	);

CREATE STATISTICS [_dta_stat_6] ON [{0}].[ADDRESS_DETAIL{1}] (
	[flat_type_code]
	,[confidence]
	,[address_detail_pid]
	,[level_type_code]
	);

CREATE STATISTICS [_dta_stat_7] ON [{0}].[ADDRESS_DETAIL{1}] (
	[confidence]
	,[street_locality_pid]
	,[address_detail_pid]
	,[level_type_code]
	,[locality_pid]
	);

CREATE STATISTICS [_dta_stat_8] ON [{0}].[ADDRESS_DETAIL{1}] (
	[address_detail_pid]
	,[level_type_code]
	,[locality_pid]
	,[flat_type_code]
	,[street_locality_pid]
	);

CREATE STATISTICS [_dta_stat_9] ON [{0}].[ADDRESS_DETAIL{1}] (
	[locality_pid]
	,[confidence]
	,[address_detail_pid]
	,[level_type_code]
	,[flat_type_code]
	,[street_locality_pid]
	);

CREATE STATISTICS [_dta_stat_10] ON [{0}].[STREET_LOCALITY{1}] (
	[street_type_code]
	,[street_locality_pid]
	);
