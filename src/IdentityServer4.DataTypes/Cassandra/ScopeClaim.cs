namespace IdentityServer4.DataTypes.Cassandra
{
    /*
    CREATE TYPE IF NOT EXISTS ScopeClaim (
	Name text,
	Description text,
	AlwaysIncludeInIdToken boolean
);
    */
    public class ScopeClaim
    {

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual bool AlwaysIncludeInIdToken { get; set; }

    }
}