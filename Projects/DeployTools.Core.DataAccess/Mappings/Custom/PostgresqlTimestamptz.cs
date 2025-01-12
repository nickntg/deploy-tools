using System;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DeployTools.Core.DataAccess.Mappings.Custom
{
    /// <summary>
    /// Marks a type as postgresql timestamptz.
    /// </summary>
    /// <remarks>
    /// This damn thing is necessary because timestamptz of Postgresql is not
    /// a true date/time+timezone thing. While there is no problem getting
    /// a DateTimeOffset into the database, there is an exception when trying
    /// to read it back from the database into DateTimeOffset. Hence this
    /// bullshit here.
    ///
    /// Thanks to https://stackoverflow.com/questions/38716692/mapping-a-custom-type-property-in-fluent-nhibernate
    /// </remarks>
    [Serializable]
    public class PostgresqlTimestamptz : IUserType
    {
        public SqlType[] SqlTypes => [SqlTypeFactory.DateTimeOffSet];

        public Type ReturnedType => typeof(DateTimeOffset?);

        public bool IsMutable => false;

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                return 0;
            }

            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            if (names.Length == 0)
            {
                throw new ArgumentException("Expecting at least one column");
            }

            var utc = NHibernateUtil.UtcDbTimestamp.NullSafeGet(rs, names[0], session);

            if (utc == null)
            {
                return null;
            }

            var dt = new DateTimeOffset(Convert.ToDateTime(utc));
            return dt != DateTimeOffset.MinValue ? dt : null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = cmd.Parameters[index];

            parameter.Value = value ?? DateTimeOffset.MinValue;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }
    }
}