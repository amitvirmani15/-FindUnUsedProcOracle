using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace FindUnusedProcedures
{
    public class StringConfiguration : SerializableConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public IgnoreElement IgnoreList
        {
            get { return (IgnoreElement) this[""]; }

        }
    }

    public class IgnoreElement : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoredElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoredElement) element).Value;
        }
    }

    public class IgnoredElement : ConfigurationElement
    {
        /// <summary>
        ///     The  value of the question
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true, IsKey = true)]
        public string Value
        {
            get => (string)this["value"];
            set => this["value"] = value;
        }
    }
}
