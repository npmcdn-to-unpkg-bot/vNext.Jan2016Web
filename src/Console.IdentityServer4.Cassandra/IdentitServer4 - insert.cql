use identityserver4;

INSERT INTO Clients JSON '
{
	"id": "ac4d5da0-8295-44ff-a448-e6d4119ea3ff",
	"client":{
		"clientid":"ac4d5da0-8295-44ff-a448-e6d4119ea3ff",
		"clientname":"myClientName",
		"allowedscopes":[
			"api1",
            "api2"
		],
		"clientsecrets":[
			{
				"description":"",
				"expiration": "2019-02-15T00:00:00Z",
				"type": "SharedSecret",
                "value": "K7gNU3sdo+OL0wNhqoVWhr3g6s1xYv72ol/pe/Unols="
			}			
		],
		"flow":3,
		"accesstokentype":1
	}
}';
 
