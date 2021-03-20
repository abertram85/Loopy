using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loopy
{
    public class LoopyConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("digitReplacements")]
        public DigitReplacementCollection DigitReplacements
        {
            get
            {
                return this["digitReplacements"] as DigitReplacementCollection;
            }
        }

    }

    public class DigitReplacementElement : ConfigurationElement
    {
        [ConfigurationProperty("multipleOf", IsRequired = true)]
        public int MultipleOf
        {
            get
            {
                return (int)this["multipleOf"];
            }
        }

        [ConfigurationProperty("replaceWith", IsRequired = true)]
        public string ReplaceWith
        {
            get
            {
                return this["replaceWith"] as string;
            }
        }

        [ConfigurationProperty("textColor")]
        public string TextColor
        {
            get
            {
                return this["textColor"] as string;
            }
        }

    }
    public class DigitReplacementCollection : ConfigurationElementCollection, IEnumerable<DigitReplacementElement>
        {
            public DigitReplacementElement this[int index]
            {
                get
                {
                    return base.BaseGet(index) as DigitReplacementElement;

                }
                set
                {
                    if (base.BaseGet(index) != null)
                    {
                        base.BaseRemoveAt(index);
                    }
                    this.BaseAdd(index, value);
                }

            }
            protected override ConfigurationElement CreateNewElement()
            {
                return new DigitReplacementElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((DigitReplacementElement)element).MultipleOf;
            }
            public override ConfigurationElementCollectionType CollectionType
            {
                get
                {
                    return ConfigurationElementCollectionType.BasicMap;
                }
            }
            protected override string ElementName
            {
                get
                {
                    return "digitReplacement";
                }
            }
            protected override bool IsElementName(string elementName)
            {
                return !String.IsNullOrEmpty(elementName) && elementName == "digitReplacement";
            }
            public new IEnumerator<DigitReplacementElement> GetEnumerator()
            {
                int count = base.Count;
                for (int i = 0; i < count; i++)
                {
                    yield return base.BaseGet(i) as DigitReplacementElement;
                }
            }

        }
}
