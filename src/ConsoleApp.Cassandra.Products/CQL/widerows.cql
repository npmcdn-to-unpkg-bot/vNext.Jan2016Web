CREATE KEYSPACE IF NOT EXISTS wideproductrow
WITH replication = {'class':'SimpleStrategy', 'replication_factor' : 1};

use wideproductrow;

CREATE TABLE IF NOT EXISTS Bubbles (
	Id uuid,
	bubbleChainId uuid,
	Name text,
	DeviceId uuid,
	DeviceIdText text,
   PRIMARY KEY (Id, DeviceId)
);


CREATE TABLE IF NOT EXISTS ProductTemplates (
	Id uuid,
  	Type text,
  	version text,
	document text,
   PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS ProductInstances (
	Id uuid,
  	Type text,
  	version text,
	document text,
	bubbleId uuid,
   PRIMARY KEY (Id)
);
