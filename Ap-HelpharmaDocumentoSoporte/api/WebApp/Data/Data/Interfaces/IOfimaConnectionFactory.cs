using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Interfaces
{
    public interface IOfimaConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
