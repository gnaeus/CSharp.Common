using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Common.Entities
{
    /// <summary>
    /// This interface is implemented by entities which wanted
    /// to store all modifications and entity-editors in <see cref="AuditLog"/>
    /// </summary>
    public interface IAuditLoggable { }
    
    public class AuditLog
    {
        // TODO
    }
}
