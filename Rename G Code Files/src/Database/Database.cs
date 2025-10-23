using System.Configuration;
using System.Data.Common;
using System.Data;

namespace Rename_G_Code_Files.src.Database
{
    abstract class Database
    {
        public abstract DbConnection Connection { get; }
        public abstract DbCommand Command { get; }

        public abstract void Execute(string sql);
        public abstract DbDataReader Read(string sql);
        public abstract DateTime GetLastModifiedDate();
    }
}