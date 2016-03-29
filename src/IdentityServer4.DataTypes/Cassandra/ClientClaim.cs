namespace IdentityServer4.DataTypes.Cassandra
{
    /*
    CREATE TYPE IF NOT EXISTS Claim (
	Type text,
	Value text,
	ValueType text
);
    */
    public class ClientClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }
}
