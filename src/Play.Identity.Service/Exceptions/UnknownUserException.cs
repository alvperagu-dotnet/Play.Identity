using System;

namespace Play.Identity.Service.Exceptions;

[Serializable]
internal class UnknownUserException : Exception
{
    public UnknownUserException(Guid userId) 
    :base($"Unknown user '{userId}'")
    {
        this.userId = userId;
    }

    public Guid userId {get;}
}