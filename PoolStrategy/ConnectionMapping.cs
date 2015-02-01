using System.Linq;
using System.Reflection;
using System.Text;

namespace DatabasePool
{
    internal class ConnectionMapping : ConnectionBase
    {
        public string Custom;

        public ConnectionData Data = new ConnectionData();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.GetType().GetFields().ToList<FieldInfo>().ForEach(delegate(FieldInfo field)
            {
                // check if Mapping for this Property exists
                var dataField = Data.GetType().GetField(field.Name);
                if (dataField == null)
                {
                    return;
                }
                // add Mapping value and value of Property if they are not null
                var value = field.GetValue(this);
                var dataValue = dataField.GetValue(Data);
                if (value != null && dataValue != null)
                {
                    builder.AppendFormat("{0}={1};", value, dataValue);
                }
            });
            if (Custom != null)
            {
                builder.AppendFormat(Custom);
            }

            return builder.ToString();
        }
    }

}
