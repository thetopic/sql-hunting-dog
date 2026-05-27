using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HuntingDog.Config;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace HuntingDog.DogFace
{
    /// <summary>
    /// Fenêtre de configuration Hunting Dog.
    /// Prend en charge le mode sombre synchronisé avec le thème SSMS.
    /// </summary>
    public partial class DialogWindow : Window
    {
        // ─── Brushes sombres (statiques, créées une seule fois) ──────────────────
        private static readonly SolidColorBrush _dkWindow = Freeze(Color.FromRgb(0x25, 0x25, 0x26));
        private static readonly SolidColorBrush _dkControl = Freeze(Color.FromRgb(0x3C, 0x3C, 0x3C));
        private static readonly SolidColorBrush _dkFg      = Freeze(Color.FromRgb(0xCC, 0xCC, 0xCC));
        private static readonly SolidColorBrush _dkBorder  = Freeze(Color.FromRgb(0x55, 0x55, 0x55));
        private static readonly SolidColorBrush _dkCatHdr  = Freeze(Color.FromRgb(0x2D, 0x2D, 0x30));
        private static readonly SolidColorBrush _dkSelect  = Freeze(Color.FromArgb(0x80, 0x7E, 0xAE, 0xFF));

        private bool _isDark;
        private bool _refreshingGrid;

        // ─── Constructeur ────────────────────────────────────────────────────────
        public DialogWindow()
        {
            InitializeComponent();
            // Parcours de l'arbre visuel après le premier rendu complet
            ContentRendered += OnContentRendered;
        }

        // ─── API publique ────────────────────────────────────────────────────────
        public DogConfig DogConfig { get; private set; }

        public void ShowConfiguration(DogConfig cfg)
        {
            DogConfig = cfg.CloneMe();
            _propertyGrid.SelectedObject = DogConfig;
            _propertyGrid.ShowSearchBox  = false;
            _propertyGrid.ShowSortOptions = false;
            _propertyGrid.ShowTitle       = false;
        }

        /// <summary>
        /// Applique le thème clair/sombre.
        /// Doit être appelé AVANT ShowDialog().
        /// </summary>
        public void ApplyTheme(bool isDark)
        {
            _isDark = isDark;

            if (isDark)
            {
                SetRes("DlgWindowBg",    _dkWindow.Color);
                SetRes("DlgControlBg",   _dkControl.Color);
                SetRes("DlgForeground",  _dkFg.Color);
                SetRes("DlgBorderBrush", _dkBorder.Color);
                SetRes("DlgButtonBg",    _dkControl.Color);
                SetRes("DlgSelect",      _dkSelect.Color);

                _propertyGrid.Background = _dkWindow;
                _propertyGrid.Foreground = _dkFg;
            }
            else
            {
                SetRes("DlgWindowBg",    Color.FromRgb(0xFF, 0xFF, 0xFF));
                SetRes("DlgControlBg",   Color.FromRgb(0xF0, 0xF0, 0xF0));
                SetRes("DlgForeground",  Color.FromRgb(0x00, 0x00, 0x00));
                SetRes("DlgBorderBrush", Color.FromRgb(0xCC, 0xCC, 0xCC));
                SetRes("DlgButtonBg",    Color.FromRgb(0xDD, 0xDD, 0xDD));
                SetRes("DlgSelect",      Color.FromArgb(0x40, 0x00, 0x78, 0xD7));

                _propertyGrid.Background = new SolidColorBrush(
                    Color.FromArgb(0x60, 0x64, 0x95, 0xED));
                _propertyGrid.Foreground = SystemColors.ControlTextBrush;
            }

            // Si la fenêtre est déjà rendue, applique immédiatement
            if (IsLoaded)
                WalkAndApply(_propertyGrid);
        }

        // ─── Gestionnaires d'événements ──────────────────────────────────────────
        private void OnContentRendered(object sender, EventArgs e)
        {
            ContentRendered -= OnContentRendered;
            if (!_isDark) return;

            // Attendre que Xceed ait fini de réaliser tous ses éditeurs avant de parcourir
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                (System.Action)(() => WalkAndApply(_propertyGrid)));
        }

        private void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (DogConfig == null || _refreshingGrid) return;
            Loc.Load(DogConfig.Language);

            _refreshingGrid = true;
            try
            {
                var cfg = DogConfig;
                _propertyGrid.SelectedObject = null;
                _propertyGrid.SelectedObject = cfg;
            }
            finally
            {
                _refreshingGrid = false;
            }

            // Après rechargement du grid, re-parcourir l'arbre
            if (_isDark)
                _propertyGrid.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    (System.Action)(() => WalkAndApply(_propertyGrid)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)   => DialogResult = true;
        private void Button_Click_1(object sender, RoutedEventArgs e) => DialogResult = false;

        // ─── Parcours de l'arbre visuel ──────────────────────────────────────────

        /// <summary>
        /// Parcourt récursivement l'arbre visuel de <paramref name="root"/>
        /// et force les couleurs sombres sur chaque contrôle trouvé.
        /// </summary>
        private void WalkAndApply(DependencyObject root)
        {
            int n = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < n; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                ApplyDarkToElement(child);
                WalkAndApply(child);
            }
        }

        private void ApplyDarkToElement(DependencyObject el)
        {
            switch (el)
            {
                // ── TextBox ───────────────────────────────────────────────────
                case TextBox tb:
                    tb.Background  = _dkControl;
                    tb.Foreground  = _dkFg;
                    tb.CaretBrush  = _dkFg;
                    tb.BorderBrush = _dkBorder;
                    break;

                // ── CheckBox (avant ToggleButton car CheckBox en hérite) ──────
                case CheckBox chk:
                    // Xceed a son propre style implicite : on force le nôtre avec
                    // le Path vectoriel DynamicResource DlgForeground.
                    chk.Style      = (Style)Resources[typeof(CheckBox)];
                    chk.Foreground = _dkFg;
                    chk.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        (System.Action)(() => WalkAndApply(chk)));
                    break;

                // ── ToggleButton interne ComboBox / autres ────────────────────
                case System.Windows.Controls.Primitives.ToggleButton tb2:
                    tb2.Background  = _dkControl;
                    tb2.Foreground  = _dkFg;
                    tb2.BorderBrush = _dkBorder;
                    break;

                // ── ComboBox ──────────────────────────────────────────────────
                case ComboBox cb:
                    cb.Background  = _dkControl;
                    cb.Foreground  = _dkFg;
                    cb.BorderBrush = _dkBorder;
                    // Force notre ControlTemplate sombre (Xceed peut avoir surchargé
                    // le style implicite via une propriété locale).
                    cb.Style = (Style)Resources[typeof(ComboBox)];
                    // Aligne l'ItemContainerStyle sur notre style implicite
                    // (couvre les éditeurs Xceed qui injectent leur propre style)
                    cb.ItemContainerStyle = (Style)Resources[typeof(ComboBoxItem)];
                    // Quand Xceed utilise IItemsSource il pose DisplayMemberPath="Value"
                    // sans ItemTemplate → SelectionBoxItemTemplate reste null dans notre
                    // ContentPresenter → fallback ToString() → "Xceed.wpf.Tool…".
                    // On crée un ItemTemplate explicite pour corriger l'affichage.
                    if (!string.IsNullOrEmpty(cb.DisplayMemberPath) && cb.ItemTemplate == null)
                    {
                        var dp = cb.DisplayMemberPath;
                        cb.DisplayMemberPath = null;   // WPF interdit DisplayMemberPath + ItemTemplate simultanément
                        var factory = new FrameworkElementFactory(typeof(TextBlock));
                        factory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(dp));
                        cb.ItemTemplate = new DataTemplate { VisualTree = factory };
                    }
                    // Reparcourir l'arbre interne après application du template
                    cb.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        (System.Action)(() => WalkAndApply(cb)));
                    break;

                // ── ComboBoxItem (dropdown réalisé) ───────────────────────────
                case ComboBoxItem cbi:
                    cbi.Background = _dkControl;
                    cbi.Foreground = _dkFg;
                    break;

                // ── ListBox ───────────────────────────────────────────────────
                case ListBox lb:
                    lb.Background  = _dkWindow;
                    lb.Foreground  = _dkFg;
                    lb.BorderBrush = _dkBorder;
                    lb.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        (System.Action)(() => WalkAndApply(lb)));
                    break;

                // ── ListBoxItem ───────────────────────────────────────────────
                case ListBoxItem lbi:
                    lbi.Background = _dkWindow;
                    lbi.Foreground = _dkFg;
                    break;

                // ── TextBlock ─────────────────────────────────────────────────
                case TextBlock tbl:
                    if (IsVeryDark(tbl.Foreground) || IsDefaultForeground(tbl))
                        tbl.Foreground = _dkFg;
                    break;

                // ── Border ────────────────────────────────────────────────────
                case Border border:
                    if (IsLightBrush(border.Background))
                        border.Background = IsHeaderBg(border) ? _dkCatHdr : _dkWindow;
                    if (IsLightBrush(border.BorderBrush) && border.BorderThickness != default)
                        border.BorderBrush = _dkBorder;
                    break;

                // ── Panel (Grid, StackPanel…) ─────────────────────────────────
                case Panel panel:
                    if (IsLightBrush(panel.Background))
                        panel.Background = _dkWindow;
                    break;
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static SolidColorBrush Freeze(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        private void SetRes(string key, Color color)
            => Resources[key] = new SolidColorBrush(color);

        /// <summary>Retourne true si le brush est une couleur claire (fond clair).</summary>
        private static bool IsLightBrush(Brush brush)
        {
            if (brush is SolidColorBrush scb && scb.Color.A > 10)
            {
                double lum = 0.2126 * scb.Color.R / 255.0
                           + 0.7152 * scb.Color.G / 255.0
                           + 0.0722 * scb.Color.B / 255.0;
                return lum > 0.45;
            }
            return false;
        }

        /// <summary>Retourne true si le brush est très sombre (quasi noir).</summary>
        private static bool IsVeryDark(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                double lum = 0.2126 * scb.Color.R / 255.0
                           + 0.7152 * scb.Color.G / 255.0
                           + 0.0722 * scb.Color.B / 255.0;
                return lum < 0.05;
            }
            return false;
        }

        /// <summary>Détecte si un TextBlock utilise le Foreground hérité (pas de valeur locale).</summary>
        private static bool IsDefaultForeground(TextBlock tb)
            => tb.ReadLocalValue(TextBlock.ForegroundProperty) == DependencyProperty.UnsetValue;

        /// <summary>
        /// Heuristique : un Border est un en-tête de catégorie s'il est
        /// large, assez haut et a un fond coloré.
        /// </summary>
        private static bool IsHeaderBg(Border border)
        {
            if (border.ActualHeight >= 20 && border.ActualHeight <= 35
                && border.Background is SolidColorBrush scb)
            {
                // Fonds bleutés ou gris moyens → probable en-tête Xceed
                double r = scb.Color.R / 255.0, g = scb.Color.G / 255.0, b = scb.Color.B / 255.0;
                return b > r + 0.05 || (r > 0.3 && g > 0.3 && b > 0.3 && r < 0.85);
            }
            return false;
        }
    }
}
