using System;
using Nest;

namespace SL.Elasticsearch.EsContextLiteInside
{
    public class KeywordLcAttribute : KeywordAttribute
    {
        public KeywordLcAttribute()
        {
            Normalizer = "lc";
        }
    }

    public class ObjectMlKwAttribute : ElasticsearchCorePropertyAttributeBase, IObjectProperty, ICoreProperty, IProperty, IFieldMapping
    {
        public ObjectMlKwAttribute() : base(FieldType.Object)
        {
            _properties = new Properties
            {
                {"ru", new KeywordProperty() {Normalizer = "lc"}},
                {"en", new KeywordProperty() {Normalizer = "lc"}}
            };
        }

        public Union<bool, DynamicMapping> Dynamic { get; set; }
        public bool? Enabled { get; set; }
        public bool? IncludeInAll { get; set; }

        private readonly IProperties _properties;
        public IProperties Properties { get { return _properties; } set { } }
    }

    public class ObjectMlTxAttribute : ElasticsearchCorePropertyAttributeBase, IObjectProperty, ICoreProperty, IProperty, IFieldMapping
    {
        public ObjectMlTxAttribute() : base(FieldType.Object)
        {
            _properties = new Properties
            {
                {"ru", new TextProperty()},
                {"en", new TextProperty()}
            };
        }

        public Union<bool, DynamicMapping> Dynamic { get; set; }
        public bool? Enabled { get; set; }
        public bool? IncludeInAll { get; set; }

        private readonly IProperties _properties;
        public IProperties Properties { get { return _properties; } set { } }
    }
}