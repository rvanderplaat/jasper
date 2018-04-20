
create table %SCHEMA%.jasper_outgoing_envelopes
(
	id uniqueidentifier not null primary key,
	owner_id int not null,
	destination varchar(250) not null,
	deliver_by datetimeoffset,
	body varbinary(max) not null
);

create table %SCHEMA%.jasper_incoming_envelopes
(
	id uniqueidentifier not null
		primary key,
	status varchar(25) not null,
	owner_id int not null,
	execution_time datetimeoffset default NULL,
	attempts int default 0 not null,
	body varbinary(max) not null
);

create table %SCHEMA%.jasper_dead_letters
(
	id uniqueidentifier not null
		primary key,

  source VARCHAR(100),
  message_type VARCHAR(250),
  explanation VARCHAR(250),
  exception_text VARCHAR(MAX),
  exception_type VARCHAR(250),
  exception_message VARCHAR(250),

	body varbinary(max) not null
);

IF NOT EXISTS(SELECT * FROM sys.table_types WHERE name = 'EnvelopeIdList')
BEGIN
    CREATE TYPE %SCHEMA%.EnvelopeIdList AS TABLE(ID UNIQUEIDENTIFIER)
END








