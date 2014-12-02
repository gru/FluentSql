using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentSql
{
    public class Select
    {
        public Fields Fields { get; set; }
        public Joins Joins { get; set; }

        public Select()
        {
            Joins = new Joins();
            Fields = new Fields();
            
        }

        public static Select Constant(ConstantExpression expression)
        {
            return new Select { Fields = new Fields(new[] { new ConstantField(expression) }) };
        }

        public static Select Constant(object value)
        {
            return Constant(new ConstantExpression(value));
        }

        public static Select All()
        {
            return new Select { Fields = new Fields(new[] { new AllField() }) };
        }

        public Select From(Table table)
        {
            Joins.Add(new SingleTableJoin(table));
            return this;
        }

        public Select From(string table)
        {
            return From(new Table(table));
        }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Select(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class Joins : List<Join>
    {
        public string ToString(SqlDialect dialect)
        {
            return dialect.Joins(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class SingleTableJoin : Join
    {
        public Table Table { get; set; }

        public SingleTableJoin(Table table)
        {
            Table = table;
        }

        public override string ToString(SqlDialect dialect)
        {
            return dialect.SingleTableJoin(this);
        }
    }

    //public class MultiTableJoin : Join
    //{
    //    public Join Left { get; set; }
    //    public Table Right { get; set; }
    //    public JoinKind Kind { get; set; }
    //}

    public abstract class Join
    {
        public abstract string ToString(SqlDialect dialect);
    }

    public enum JoinKind
    {
        Inner, Left, Right, Cross
    }

    public class Table
    {
        public string Name { get; set; }

        public Table(string name)
        {
            Name = name;
        }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Table(this);
        }
    }

    public class ConstantField : Field
    {
        public ConstantExpression Expression { get; set; }

        public ConstantField(ConstantExpression expression)
        {
            Expression = expression;
        }

        public override string ToString(SqlDialect dialect)
        {
            return Expression.ToString(dialect);
        }

        public override string ToString()
        {
            return Expression.ToString(SqlDialect.Default);
        }
    }

    public abstract class Field
    {
        public abstract string ToString(SqlDialect dialect);
    }

    public class AllField : Field
    {
        public override string ToString(SqlDialect dialect)
        {
            return dialect.AllFields();
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class TableField : Field
    {
        public string Name { get; set; }


        public TableField(string name)
        {
            Name = name;
        }

        public override string ToString(SqlDialect dialect)
        {
            return dialect.TableField(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class Fields : List<Field>
    {
        public Fields()
            : base(Enumerable.Empty<Field>())
        {
        }

        public Fields(IEnumerable<Field> fields)
        {
            AddRange(fields);
        }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Fields(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class Expression
    {
    }

    public class ConstantExpression : Expression
    {
        public object Value { get; set; }

        public ConstantExpression(object value)
        {
            Value = value;
        }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Constant(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class SqlDialect
    {
        private static SqlDialect @default = 
            new SqlDialect();

        public static SqlDialect Default
        {
            get { return @default; }
            set { @default = value; }
        }

        public virtual string Select(Select select)
        {
            var sb = new StringBuilder("SELECT ");

            if (select.Fields.Any())
            {
                sb.Append(select.Fields.ToString(this));
            }

            if (select.Joins.Any())
            {
                sb.Append(select.Joins.ToString(this));
            }

            return sb.ToString();
        }

        public virtual string Fields(Fields fields)
        {
            return string.Join(", ", fields.Select(f => f.ToString(this)));
        }

        public virtual string Constant(ConstantExpression expression)
        {
            return expression.Value.ToString();
        }

        public virtual string TableField(TableField field)
        {
            return string.Format("[{0}]", field.Name);
        }

        public virtual string AllFields()
        {
            return "*";
        }

        public virtual string Table(Table table)
        {
            return table.Name;
        }

        public virtual string SingleTableJoin(SingleTableJoin join)
        {
            return join.Table.ToString(this);
        }

        public string Joins(Joins joins)
        {
            return "FROM " + joins.Select(j => j.ToString(this));
        }
    }
}
