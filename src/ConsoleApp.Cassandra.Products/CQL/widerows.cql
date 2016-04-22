CREATE KEYSPACE IF NOT EXISTS wideproductrow
WITH replication = {'class':'SimpleStrategy', 'replication_factor' : 1};

use wideproductrow;

CREATE TABLE IF NOT EXISTS Bubbles (
	Id uuid,
	bubbleChainId uuid,
	Name text,
	DeviceId uuid,
	DeviceIdText text,
   PRIMARY KEY (Id)
);


CREATE TABLE IF NOT EXISTS ProductTemplates (
	Id uuid,
  	DocumentType text,
  	DocumentVersion text,
	Document text,
   PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS ProductTemplates_by_type (
	Id uuid,
  	DocumentType text,
  	DocumentVersion text,
	Document text,
   PRIMARY KEY (DocumentType,DocumentVersion)
);

CREATE TABLE IF NOT EXISTS ProductInstances (
	Id uuid,
	label text,
	bubbleId uuid,
	ProductTemplateId uuid,
  	DocumentType text,
  	DocumentVersion text,
	Document text,
   PRIMARY KEY (Id)
);

CREATE TABLE IF NOT EXISTS ProductInstances_by_label (
	label text,
	Id uuid,
	bubbleId uuid,
	ProductTemplateId uuid,
  	DocumentType text,
  	DocumentVersion text,
	Document text,
   PRIMARY KEY (label,Id)
);