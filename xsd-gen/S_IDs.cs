//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
//This source code was auto-generated by MonoXSD
//
namespace S_IDs {
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class S5Data {
        
        private S5DataArtikl[] artiklListField;
        
        private S5DataFirma[] firmaListField;
        
        private S5DataSazbaDPH[] sazbaDPHListField;
        
        private S5DataSklad[] skladListField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("Artikl", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public S5DataArtikl[] ArtiklList {
            get {
                return this.artiklListField;
            }
            set {
                this.artiklListField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("Firma", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public S5DataFirma[] FirmaList {
            get {
                return this.firmaListField;
            }
            set {
                this.firmaListField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("SazbaDPH", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public S5DataSazbaDPH[] SazbaDPHList {
            get {
                return this.sazbaDPHListField;
            }
            set {
                this.sazbaDPHListField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("Sklad", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public S5DataSklad[] SkladList {
            get {
                return this.skladListField;
            }
            set {
                this.skladListField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class S5DataArtikl {
        
        private group groupField;
        
        private string katalogField;
        
        private string objectNameField;
        
        private string objectTypeField;
        
        private string idField;
        
        public S5DataArtikl() {
            this.objectNameField = "Artikl";
            this.objectTypeField = "Object";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public group Group {
            get {
                return this.groupField;
            }
            set {
                this.groupField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Katalog {
            get {
                return this.katalogField;
            }
            set {
                this.katalogField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectName {
            get {
                return this.objectNameField;
            }
            set {
                this.objectNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class group {
        
        private string idField;
        
        private string kodField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Kod {
            get {
                return this.kodField;
            }
            set {
                this.kodField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class S5DataFirma {
        
        private group groupField;
        
        private string kodField;
        
        private string objectNameField;
        
        private string objectTypeField;
        
        private string idField;
        
        public S5DataFirma() {
            this.objectNameField = "Firma";
            this.objectTypeField = "Object";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public group Group {
            get {
                return this.groupField;
            }
            set {
                this.groupField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Kod {
            get {
                return this.kodField;
            }
            set {
                this.kodField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectName {
            get {
                return this.objectNameField;
            }
            set {
                this.objectNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class S5DataSazbaDPH {
        
        private string sazbaField;
        
        private string objectNameField;
        
        private string objectTypeField;
        
        private string idField;
        
        public S5DataSazbaDPH() {
            this.objectNameField = "SazbaDPH";
            this.objectTypeField = "Object";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Sazba {
            get {
                return this.sazbaField;
            }
            set {
                this.sazbaField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectName {
            get {
                return this.objectNameField;
            }
            set {
                this.objectNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class S5DataSklad {
        
        private group groupField;
        
        private string kodField;
        
        private string objectNameField;
        
        private string objectTypeField;
        
        private string idField;
        
        public S5DataSklad() {
            this.objectNameField = "Sklad";
            this.objectTypeField = "Object";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public group Group {
            get {
                return this.groupField;
            }
            set {
                this.groupField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Kod {
            get {
                return this.kodField;
            }
            set {
                this.kodField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectName {
            get {
                return this.objectNameField;
            }
            set {
                this.objectNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
}