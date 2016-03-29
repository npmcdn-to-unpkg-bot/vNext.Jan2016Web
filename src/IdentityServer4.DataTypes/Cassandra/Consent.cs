namespace IdentityServer4.DataTypes.Cassandra
{
    /*
        CREATE TYPE IF NOT EXISTS Consent(
          SubjectId text,
          ClientId text,
          Scopes text
        );
    */

    public class Consent
    {
        public string SubjectId { get; set; }

        public string ClientId { get; set; }

        public string Scopes { get; set; }
    }
}