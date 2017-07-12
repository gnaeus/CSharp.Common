using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Common.Entities
{
    /// <summary>
    /// This interface is implemented by entities which wanted
    /// to store all modifications in <see cref="TransactionLog"/>.
    /// </summary>
    public interface ITransactionLoggable { }

    public class TransactionLog
    {
        // TODO
    }
}
