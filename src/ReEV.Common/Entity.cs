namespace ReEV.Common
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

        protected Entity(Guid id)
        {
            Id = id;
            CreatedAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        protected Entity() : this(Guid.NewGuid()) { }
    }
}
