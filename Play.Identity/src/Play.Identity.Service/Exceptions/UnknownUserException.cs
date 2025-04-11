namespace Play.Identity.Service.Exceptions
{
    public class UnknownUserException : Exception
    {
        private Guid userId;

        public UnknownUserException()
        {
        }

        public UnknownUserException(Guid userId) : base($"Unknown user {userId}")
        {
            this.UserId = userId;
        }

        public Guid UserId { get; }

    }
}