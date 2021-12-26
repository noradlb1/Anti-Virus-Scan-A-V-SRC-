#region Notes
// Based on the silverlight viewmodel example by Colin E. on ScottLogic.co.uk
// http://www.scottlogic.co.uk/blog/colin/2011/02/a-circular-progressbar-style-using-an-attached-viewmodel
// Modified for wpf and turned into a control library..
#endregion

#region Directives
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CircularProgressBar.Helpers;
using System.Windows.Data;
using System.ComponentModel;
using System;
using System.Windows.Media;
using System.Collections.Generic;
#endregion

namespace CircularProgressBar
{
    public class CircularProgressBar : ProgressBar
    {
        #region Property Name Constants
        /// <summary>
        /// Property names as string constants
        /// </summary>
        private const string ThemePropName = "Theme";
        private const string ForegroundPropName = "Foreground";
        private const string AccentPropName = "Accent";
        private const string BarColorPropName = "BarColor";
        private const string PercentPropName = "Percent";
        private const string DiameterPropName = "Diameter";
        private const string RadiusPropName = "Radius";
        private const string InnerRadiusPropName = "InnerRadius";
        private const string CentreXPropName = "CentreX";
        private const string CentreYPropName = "CentreY";
        private const string AnglePropName = "Angle";
        private const string HoleSizeFactorPropName = "HoleSizeFactor";
        private const string DisplayPropName = "Display";
        private const string ValuePropName = "Value";
        #endregion

        #region Theme Declarations
        /// <summary>
        /// Operating System Themes
        /// </summary>
        public static ThemePath CircularProgressBarStyle = new ThemePath(Themes.CircularProgressBarStyle.ToString(), "/CircularProgressBar;component/Themes/CircularProgressBar.Theme.xaml");
        public static ThemePath GlassyProgressBarStyle = new ThemePath(Themes.GlassyProgressBarStyle.ToString(), "/CircularProgressBar;component/Themes/GlassyProgressBar.Theme.xaml");
        public static ThemePath PieProgressBarStyle = new ThemePath(Themes.PieProgressBarStyle.ToString(), "/CircularProgressBar;component/Themes/PieProgressBar.Theme.xaml");
        #endregion

        #region Fields
        private Dictionary<string, ResourceDictionary> _rdThemeDictionaries;
        #endregion

        #region Constructors
        static CircularProgressBar()
        {
            // Angle
            FrameworkPropertyMetadata angleMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnAngleChanged),
                AffectsRender = true
            };
            AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(CircularProgressBar), angleMetaData);

            // Accent
            FrameworkPropertyMetadata accentMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Brushes.LightGray,
                PropertyChangedCallback = new PropertyChangedCallback(OnAccentChanged),
                AffectsRender = true
            };
            AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(CircularProgressBar), accentMetaData);

            // BarColor
            FrameworkPropertyMetadata barColorMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Brushes.Black,
                PropertyChangedCallback = new PropertyChangedCallback(OnBarColorChanged),
                AffectsRender = true
            };
            BarColorProperty = DependencyProperty.Register("BarColor", typeof(Brush), typeof(CircularProgressBar), barColorMetaData);

            // CentreX
            FrameworkPropertyMetadata centreXMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnCentreXChanged),
                AffectsRender = true
            };
            CentreXProperty = DependencyProperty.Register("CentreX", typeof(double), typeof(CircularProgressBar), centreXMetaData);

            // CentreY
            FrameworkPropertyMetadata centreYMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnCentreYChanged),
                AffectsRender = true
            };
            CentreYProperty = DependencyProperty.Register("CentreY", typeof(double), typeof(CircularProgressBar), centreYMetaData);

            // Diameter
            FrameworkPropertyMetadata diameterMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnDiameterChanged),
                AffectsRender = true
            };
            DiameterProperty = DependencyProperty.Register("Diameter", typeof(double), typeof(CircularProgressBar), diameterMetaData);

            // Display
            FrameworkPropertyMetadata displayMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = "0%",
                PropertyChangedCallback = new PropertyChangedCallback(OnDisplayChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceDisplayChange),
                AffectsRender = true
            };
            DisplayProperty = DependencyProperty.Register("Display", typeof(string), typeof(CircularProgressBar), displayMetaData);

            // Foreground
            FrameworkPropertyMetadata foregroundMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Brushes.Black,
                PropertyChangedCallback = new PropertyChangedCallback(OnForegroundChanged),
                AffectsRender = true
            };
            ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(CircularProgressBar), foregroundMetaData);

            // HoleSizeFactor
            FrameworkPropertyMetadata holeSizeFactorMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.75,
                PropertyChangedCallback = new PropertyChangedCallback(OnHoleSizeFactorChanged),
                AffectsRender = true
            };
            HoleSizeFactorProperty = DependencyProperty.Register("HoleSizeFactor", typeof(double), typeof(CircularProgressBar), holeSizeFactorMetaData);

            // InnerRadius
            FrameworkPropertyMetadata innerRadiusMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnInnerRadiusChanged),
                AffectsRender = true
            };
            InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(CircularProgressBar), innerRadiusMetaData);

            // Percent
            FrameworkPropertyMetadata percentMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnPercentChanged),
                AffectsRender = true
            };
            PercentProperty = DependencyProperty.Register("Percent", typeof(double), typeof(CircularProgressBar), percentMetaData);

            // Radius
            FrameworkPropertyMetadata radiusMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnRadiusChanged),
                AffectsRender = true
            };
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(CircularProgressBar), radiusMetaData);

            // SetTheme
            FrameworkPropertyMetadata themeMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = Themes.CircularProgressBarStyle.ToString(),
                CoerceValueCallback = new CoerceValueCallback(CoerceThemeChange),
                PropertyChangedCallback = new PropertyChangedCallback(OnThemeChanged),
                AffectsRender = true,
                AffectsMeasure = true
            };
            ThemeProperty = DependencyProperty.Register("Theme", typeof(string), typeof(CircularProgressBar), themeMetaData);

            // Value
            FrameworkPropertyMetadata valueMetaData = new FrameworkPropertyMetadata
            {
                DefaultValue = 0.0,
                PropertyChangedCallback = new PropertyChangedCallback(OnValueChanged),
                CoerceValueCallback = new CoerceValueCallback(CoerceValueChange),
                AffectsRender = true
            };
            ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(CircularProgressBar), valueMetaData);

            // Default
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CircularProgressBar), new FrameworkPropertyMetadata(typeof(CircularProgressBar)));
        }

        public CircularProgressBar()
        {
            // register inbuilt themes
            RegisterAttachedThemes();
        }
        #endregion

        #region Properties
        #region Accent
        /// <summary>
        /// Gets/Sets the ProgressBar ForeColor.
        /// </summary>
        public static readonly DependencyProperty AccentProperty;

        public Brush Accent
        {
            get { return (Brush)this.GetValue(AccentProperty); }
            set { this.SetValue(AccentProperty, value); }
        }

        private static void OnAccentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            Brush brush = (Brush)e.NewValue;

            if (brush == null)
            {
                cp.Accent = Brushes.LightGray;
            }
            cp.OnPropertyChanged(new PropertyChangedEventArgs(AccentPropName));
        }
        #endregion

        #region Angle
        /// <summary>
        /// Gets/Sets the ProgressBar Angle.
        /// </summary>
        public static readonly DependencyProperty AngleProperty;

        public double Angle
        {
            get { return (double)this.GetValue(AngleProperty); }
            set { this.SetValue(AngleProperty, value); }
        }

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            //cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(AnglePropName));
        }
        #endregion

        #region BarColor
        /// <summary>
        /// Gets/Sets the ProgressBar ForeColor.
        /// </summary>
        public static readonly DependencyProperty BarColorProperty;

        public Brush BarColor
        {
            get { return (Brush)this.GetValue(BarColorProperty); }
            set { this.SetValue(BarColorProperty, value); }
        }

        private static void OnBarColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            Brush brush = (Brush)e.NewValue;

            if (brush == null)
            {
                cp.Accent = Brushes.Black;
            }
            cp.OnPropertyChanged(new PropertyChangedEventArgs(BarColorPropName));
        }
        #endregion

        #region CentreX
        /// <summary>
        /// Gets/Sets the ProgressBar CentreX.
        /// </summary>
        public static readonly DependencyProperty CentreXProperty;

        public double CentreX
        {
            get { return (double)this.GetValue(CentreXProperty); }
            set { this.SetValue(CentreXProperty, value); }
        }

        private static void OnCentreXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            // cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(CentreXPropName));
        }
        #endregion

        #region CentreY
        /// <summary>
        /// Gets/Sets the ProgressBar CentreY.
        /// </summary>
        public static readonly DependencyProperty CentreYProperty;

        public double CentreY
        {
            get { return (double)this.GetValue(CentreYProperty); }
            set { this.SetValue(CentreYProperty, value); }
        }

        private static void OnCentreYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            // cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(CentreYPropName));
        }
        #endregion

        #region Diameter
        /// <summary>
        /// Gets/Sets the ProgressBar Diameter.
        /// </summary>
        public static readonly DependencyProperty DiameterProperty;

        public double Diameter
        {
            get { return (double)this.GetValue(DiameterProperty); }
            set { this.SetValue(DiameterProperty, value); }
        }

        private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            //  cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(DiameterPropName));
        }
        #endregion

        #region Display
        /// <summary>
        /// Gets/Sets the ProgressBar Display string.
        /// </summary>
        public static readonly DependencyProperty DisplayProperty;

        public string Display
        {
            get { return (string)this.GetValue(DisplayProperty); }
            set { this.SetValue(DisplayProperty, value); }
        }

        private static void OnDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            cp.OnPropertyChanged(new PropertyChangedEventArgs(DisplayPropName));
        }

        private static object CoerceDisplayChange(DependencyObject d, object o)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            //o = (cp.Percent * 100) + "%";
            return o;
        }
        #endregion

        #region Foreground
        /// <summary>
        /// Gets/Sets the ProgressBar ForeColor.
        /// </summary>
        public new static readonly DependencyProperty ForegroundProperty;

        public new Brush Foreground
        {
            get { return (Brush)this.GetValue(ForegroundProperty); }
            set { this.SetValue(ForegroundProperty, value); }
        }

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            Brush brush = (Brush)e.NewValue;

            if (brush == null)
            {
                cp.Foreground = Brushes.Black;
            }
            cp.OnPropertyChanged(new PropertyChangedEventArgs(ForegroundPropName));
        }
        #endregion

        #region HoleSizeFactor
        /// <summary>
        /// Gets/Sets the ProgressBar HoleSizeFactor.
        /// </summary>
        public static readonly DependencyProperty HoleSizeFactorProperty;

        public double HoleSizeFactor
        {
            get { return (double)this.GetValue(HoleSizeFactorProperty); }
            set { this.SetValue(HoleSizeFactorProperty, value); }
        }

        private static void OnHoleSizeFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(HoleSizeFactorPropName));
        }
        #endregion

        #region InnerRadius
        /// <summary>
        /// Gets/Sets the ProgressBar InnerRadius.
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty;

        public double InnerRadius
        {
            get { return (double)this.GetValue(InnerRadiusProperty); }
            set { this.SetValue(InnerRadiusProperty, value); }
        }

        private static void OnInnerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            // cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(InnerRadiusPropName));
        }
        #endregion

        #region Percent
        /// <summary>
        /// Gets/Sets the ProgressBar Percent.
        /// </summary>
        public static readonly DependencyProperty PercentProperty;

        public double Percent
        {
            get { return (double)this.GetValue(PercentProperty); }
            set { this.SetValue(PercentProperty, value); }
        }

        private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            //  cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(PercentPropName));
        }
        #endregion

        #region Radius
        /// <summary>
        /// Gets/Sets the ProgressBar Radius.
        /// </summary>
        public static readonly DependencyProperty RadiusProperty;

        public double Radius
        {
            get { return (double)this.GetValue(RadiusProperty); }
            set { this.SetValue(RadiusProperty, value); }
        }

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            // cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(RadiusPropName));
        }
        #endregion

        #region Theme
        /// <summary>
        /// Get/Sets the ProgressBar theme
        /// </summary>
        public static DependencyProperty ThemeProperty;

        public string Theme
        {
            get { return (string)GetValue(ThemeProperty); }
            set { SetValue(ThemeProperty, value); }
        }

        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar vc = d as CircularProgressBar;

            // test args
            if (vc == null || e == null)
            {
                throw new ArgumentNullException("Invalid Theme property");
            }

            // current theme
            string curThemeName = e.OldValue as string;
            string curRegisteredThemeName = vc.GetRegistrationName(curThemeName, vc.GetType());

            if (vc._rdThemeDictionaries.ContainsKey(curRegisteredThemeName))
            {
                // remove current theme
                ResourceDictionary curThemeDictionary = vc._rdThemeDictionaries[curRegisteredThemeName];
                vc.Resources.MergedDictionaries.Remove(curThemeDictionary);
            }

            // new theme name
            string newThemeName = e.NewValue as string;
            string newRegisteredThemeName = vc.GetRegistrationName(newThemeName, vc.GetType());

            // set defaults for style
            switch (newThemeName)
            {
                case "CircularProgressBarStyle":
                    vc.HoleSizeFactor = 0.75;
                    vc.Accent = Brushes.LightGray;
                    vc.BarColor = Brushes.Black;
                    break;
                case "GlassyProgressBarStyle":
                    vc.HoleSizeFactor = 0.0;
                    vc.Accent = Brushes.DarkGreen;
                    vc.BarColor = Brushes.Green;
                    break;
                case "PieProgressBarStyle":
                    vc.HoleSizeFactor = 0.0;
                    vc.Accent = Brushes.LightGray;
                    vc.BarColor = Brushes.LightBlue;
                    break;
            }
            // add the resource
            if (!vc._rdThemeDictionaries.ContainsKey(newRegisteredThemeName))
            {
                throw new ArgumentNullException("Invalid Theme property");
            }
            else
            {
                // add the dictionary
                ResourceDictionary newThemeDictionary = vc._rdThemeDictionaries[newRegisteredThemeName];
                vc.Resources.MergedDictionaries.Add(newThemeDictionary);
            }
            vc.OnPropertyChanged(new PropertyChangedEventArgs(ThemePropName));
        }

        private static object CoerceThemeChange(DependencyObject d, object o)
        {
            return o;
        }
        #endregion

        #region Value
        /// <summary>
        /// Gets/Sets the ProgressBar Value.
        /// </summary>
        public new static readonly DependencyProperty ValueProperty;

        public new double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CircularProgressBar cp = d as CircularProgressBar;
            cp.ComputeViewModelProperties(cp);
            cp.OnPropertyChanged(new PropertyChangedEventArgs(ValuePropName));
        }

        private static object CoerceValueChange(DependencyObject d, object o)
        {
            CircularProgressBar cp = d as CircularProgressBar;

            if ((double)o > cp.Maximum)
            {
                o = cp.Maximum;
            }
            else if ((double)o < cp.Minimum)
            {
                o = cp.Minimum;
            }
            
            return o;
        }
        #endregion
        #endregion

        #region Methods
        #region Theming
        /// <summary>
        /// Load the default theme
        /// </summary>
        private void LoadDefaultTheme()
        {
            string registrationName = GetRegistrationName(Themes.CircularProgressBarStyle.ToString(), typeof(CircularProgressBar));
            this.Resources.MergedDictionaries.Add(_rdThemeDictionaries[registrationName]);
        }

        /// <summary>
        /// Instance theme dictionary and add themes
        /// </summary>
        private void RegisterAttachedThemes()
        {
            _rdThemeDictionaries = new Dictionary<string, ResourceDictionary>();
            RegisterTheme(CircularProgressBarStyle, typeof(CircularProgressBar));
            RegisterTheme(GlassyProgressBarStyle, typeof(CircularProgressBar));
            RegisterTheme(PieProgressBarStyle, typeof(CircularProgressBar));
        }

        /// <summary>
        /// Register a theme with internal dictionary
        /// </summary>
        public void RegisterTheme(ThemePath theme, Type ownerType)
        {
            // test args
            if ((theme.Name == null) || (theme.DictionaryPath == null))
            {
                throw new ArgumentNullException("Theme name/path is null");
            }
            if (ownerType == null)
            {
                throw new ArgumentNullException("Invalid ownerType");
            }

            // get registration name vhCalendar.Calendar;CustomTheme
            string registrationName = GetRegistrationName(theme.Name, ownerType);

            try
            {
                if (!_rdThemeDictionaries.ContainsKey(registrationName))
                {
                    // create the Uri
                    Uri themeUri = new Uri(theme.DictionaryPath, UriKind.Relative);
                    // register the new theme
                    _rdThemeDictionaries[registrationName] = Application.LoadComponent(themeUri) as ResourceDictionary;
                }
            }
            catch { }
        }

        /// <summary>
        /// Get themes formal registration name
        /// </summary>
        private string GetRegistrationName(string themeName, Type ownerType)
        {
            return string.Format("{0};{1}", ownerType.ToString(), themeName);
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event
        /// </summary>
        /// <param name="e">The arguments to pass</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ComputeViewModelProperties(this);
        }
        #endregion

        #region Calculations
        private void ComputeViewModelProperties(CircularProgressBar cp)
        {
            if (cp != null)
            {
                cp.Angle = (cp.Value - cp.Minimum) * 360 / (cp.Maximum - cp.Minimum);
                cp.CentreX = cp.Width / 2;
                cp.CentreY = cp.Height / 2;
                cp.Radius = Math.Min(CentreX, CentreY);
                cp.Diameter = Radius * 2;
                cp.InnerRadius = Radius * HoleSizeFactor;
                cp.Percent = Angle / 360;
                cp.Display = (uint)(cp.Percent * 100) + "%";
            }
        }
        #endregion
        #endregion
    }

    public static class UIHelper
    {
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null)
            {
                return null;
            }
            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null)
                {
                    return parent;
                }
                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }
}
