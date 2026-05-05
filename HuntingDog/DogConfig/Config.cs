using System;
using System.ComponentModel;
using System.IO;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HuntingDog.Config
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string _key;
        public LocalizedDisplayNameAttribute(string key) : base(key) { _key = key; }
        public override string DisplayName => Loc.T(_key);
    }

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _key;
        public LocalizedDescriptionAttribute(string key) : base(key) { _key = key; }
        public override string Description => Loc.T(_key);
    }

    public enum EAlterOrCreate
    {
        Alter,
        Create,
        CreateOrAlter
    }

    public enum EOrderBy
    {
        None,
        Ascending,
        Descending
    }

    public class DogConfig
    {
        private int _selectTopXTable;
        private int _limitSearch;
        private int _fontSize;

        public DogConfig()
        {
            FontSize = 14;

            ScriptIndexes = true;
            ScriptTriggers = true;
            ScriptForeignKeys = false;

            SelectTopX = 200;

            AddNoLock = false;
            IncludeAllColumns = true;
            AddWhereClauseFor = false;
            OrderBy = EOrderBy.Descending;

            AlterOrCreate = EAlterOrCreate.Create;

            LimitSearch = 500;

            HideAfterAction = false;
            ShowAfterOpen = true;

            Language = "en";
        }

        [Category("SELECT")]
        [LocalizedDisplayName("Prop_IncludeAllColumns")]
        [LocalizedDescription("Desc_IncludeAllColumns")]
        [PropertyOrder(1)]
        public bool IncludeAllColumns { get; set; }

        [Category("SCRIPT")]
        [LocalizedDisplayName("Prop_ScriptIndexes")]
        [LocalizedDescription("Desc_ScriptIndexes")]
        [PropertyOrder(10)]
        public bool ScriptIndexes { get; set; }

        [Category("SCRIPT")]
        [LocalizedDisplayName("Prop_ScriptTriggers")]
        [LocalizedDescription("Desc_ScriptTriggers")]
        [PropertyOrder(11)]
        public bool ScriptTriggers { get; set; }

        [Category("SCRIPT")]
        [LocalizedDisplayName("Prop_ScriptForeignKeys")]
        [LocalizedDescription("Desc_ScriptForeignKeys")]
        [PropertyOrder(12)]
        public bool ScriptForeignKeys { get; set; }

        [Category("SELECT")]
        [LocalizedDisplayName("Prop_AddWhereClauseFor")]
        [LocalizedDescription("Desc_AddWhereClauseFor")]
        [PropertyOrder(2)]
        public bool AddWhereClauseFor { get; set; }

        [Category("SELECT")]
        [LocalizedDisplayName("Prop_AddNoLock")]
        [LocalizedDescription("Desc_AddNoLock")]
        [PropertyOrder(3)]
        public bool AddNoLock { get; set; }

        [Category("SELECT")]
        [LocalizedDisplayName("Prop_OrderBy")]
        [LocalizedDescription("Desc_OrderBy")]
        [PropertyOrder(4)]
        public EOrderBy OrderBy { get; set; }

        [Category("SELECT")]
        [LocalizedDisplayName("Prop_SelectTopX")]
        [LocalizedDescription("Desc_SelectTopX")]
        [PropertyOrder(5)]
        public int SelectTopX
        {
            get { return _selectTopXTable; }
            set
            {
                if (value <= 0)
                    throw new InvalidDataException("Must be greater than zero");
                _selectTopXTable = value;
            }
        }

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_LimitSearch")]
        [LocalizedDescription("Desc_LimitSearch")]
        [PropertyOrder(20)]
        public int LimitSearch
        {
            get { return _limitSearch; }
            set
            {
                if (value <= 0)
                    throw new InvalidDataException("Must be greater than zero");
                _limitSearch = value;
            }
        }

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_FontSize")]
        [LocalizedDescription("Desc_FontSize")]
        [PropertyOrder(21)]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                if (_fontSize < 8)
                    _fontSize = 8;
                else if (_fontSize > 16)
                    _fontSize = 16;
            }
        }

        private string _launchingHotKey = "D";

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_LaunchingHotKey")]
        [LocalizedDescription("Desc_LaunchingHotKey")]
        [PropertyOrder(22)]
        public string LaunchingHotKey
        {
            get { return _launchingHotKey; }
            set
            {
                if (value.Length != 1)
                    throw new InvalidDataException("Must be one character");
                _launchingHotKey = value;
            }
        }

        private bool _hideAfterAction = false;

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_HideAfterAction")]
        [LocalizedDescription("Desc_HideAfterAction")]
        [PropertyOrder(23)]
        public bool HideAfterAction
        {
            get { return _hideAfterAction; }
            set { _hideAfterAction = value; }
        }

        private bool _showAfterOpen = false;

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_ShowAfterOpen")]
        [LocalizedDescription("Desc_ShowAfterOpen")]
        [PropertyOrder(24)]
        public bool ShowAfterOpen
        {
            get { return _showAfterOpen; }
            set { _showAfterOpen = value; }
        }

        [Category("MODIFY")]
        [LocalizedDisplayName("Prop_AlterOrCreate")]
        [LocalizedDescription("Desc_AlterOrCreate")]
        [PropertyOrder(30)]
        public EAlterOrCreate AlterOrCreate { get; set; }

        [Category("GENERAL")]
        [LocalizedDisplayName("Prop_Language")]
        [LocalizedDescription("Desc_Language")]
        [ItemsSource(typeof(LanguageItemsSource))]
        [PropertyOrder(25)]
        public string Language { get; set; } = "en";

        public DogConfig CloneMe()
        {
            return (DogConfig)this.MemberwiseClone();
        }
    }

    public class LanguageItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var items = new ItemCollection();
            items.Add("en", "English");
            items.Add("fr", "Français");
            return items;
        }
    }
}
