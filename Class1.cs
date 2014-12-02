using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentSql
{
    public class Select
    {
        public Top Top { get; set; }
        public Fields Fields { get; set; }
        public Joins Joins { get; set; }

        public Select()
        {
            Joins = new Joins();
            Fields = new Fields();
        }

        public static SelectBuilder Constant(ConstantExpression expression)
        {
            var select = new Select { Fields = new Fields(new[] { new ConstantField(expression) }) };
            return new SelectBuilder(select);
        }

        public static SelectBuilder Constant(object value)
        {
            return Constant(new ConstantExpression(value));
        }

        public static SelectBuilder All()
        {
            var select = new Select { Fields = new Fields(new[] { new AllField() }) };
            return new SelectBuilder(select);
        }

        public static SelectBuilder TopCount(int count)
        {
            var select = new Select { Top = new Top(count, TopKind.Count) };
            return new SelectBuilder(select);
        }

        public static SelectBuilder TopPercent(decimal percent)
        {
            var select = new Select { Top = new Top(percent, TopKind.Percent) };
            return new SelectBuilder(select);
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

    public class SelectBuilder
    { 
        public Select Select { get; set; }

        public SelectBuilder(Select @select)
        {
            Select = @select;
        }

        public SelectBuilder From(Table table)
        {
            Select.Joins.Add(new SingleTableJoin(table));
            return this;
        }

        public SelectBuilder From(string table)
        {
            return From(new Table(table));
        }

        public SelectBuilder All()
        {
            Select.Fields.Add(new AllField());

            return this;
        }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Select(Select);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public class Top
    {
        public Top(decimal value, TopKind kind)
        {
            Value = value;
            Kind = kind;
        }

        public decimal Value { get; set; }
        public TopKind Kind { get; set; }

        public string ToString(SqlDialect dialect)
        {
            return dialect.Top(this);
        }

        public override string ToString()
        {
            return ToString(SqlDialect.Default);
        }
    }

    public enum TopKind
    {
        Count, Percent
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

            if (select.Top != null)
            {
                sb.Append(select.Top.ToString(this));
            }

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

        public virtual string Top(Top top)
        {
            if (top.Kind == TopKind.Percent)
            {
                return string.Format("TOP {0} PERCENT ", top.Value);
            }

            return string.Format("TOP {0} ", top.Value);
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
            return "\r\nFROM " + string.Concat(joins.Select(j => j.ToString(this)));
        }
    }
}
