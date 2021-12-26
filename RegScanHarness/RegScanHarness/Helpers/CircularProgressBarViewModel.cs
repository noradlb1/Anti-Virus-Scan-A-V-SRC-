using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RegScanHarness.Helpers.LinkToVisualTree;
using System.Windows.Data;
using System.ComponentModel;
using System;
using System.Windows.Media;

namespace RegScanHarness.Helpers.CircularProgressBar
{
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
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

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
  /// <summary>
  /// An attached view model that adapts a ProgressBar control to provide properties
  /// that assist in the creation of a circular template
  /// </summary>
  public class CircularProgressBarViewModel : FrameworkElement, INotifyPropertyChanged
  {
    #region Attach attached property

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach", typeof(object), typeof(CircularProgressBarViewModel),
            new PropertyMetadata(null, new PropertyChangedCallback(OnAttachChanged)));

    public static CircularProgressBarViewModel GetAttach(DependencyObject d)
    {
      return (CircularProgressBarViewModel)d.GetValue(AttachProperty);
    }

    public static void SetAttach(DependencyObject d, CircularProgressBarViewModel value)
    {
      d.SetValue(AttachProperty, value);
    }

    /// <summary>
    /// Change handler for the Attach property
    /// </summary>
    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // set the view model as the DataContext for the rest of the template
      FrameworkElement targetElement = d as FrameworkElement;
      CircularProgressBarViewModel viewModel = e.NewValue as CircularProgressBarViewModel;
      targetElement.DataContext = viewModel;

      // handle the loaded event
      targetElement.Loaded += new RoutedEventHandler(Element_Loaded);
    }

    /// <summary>
    /// Handle the Loaded event of the element to which this view model is attached
    /// in order to enable the attached
    /// view model to bind to properties of the parent element
    /// </summary>
    static void Element_Loaded(object sender, RoutedEventArgs e)
    {
      FrameworkElement targetElement = sender as FrameworkElement;
      CircularProgressBarViewModel attachedModel = GetAttach(targetElement);
      var progressBar = UIHelper.TryFindParent<ProgressBar>(targetElement) as ProgressBar;
      // find the ProgressBar and associated it with the view model
     // var progressBar = targetElement.Ancestors<ProgressBar>().Single() as ProgressBar;
      attachedModel.SetProgressBar(progressBar);
    }

    #endregion


    #region fields

    private double _angle;

    private double _centreX;

    private double _centreY;

    private double _radius;

    private double _innerRadius;

    private double _diameter;

    private double _percent;

    private double _holeSizeFactor = 0.0;

    protected ProgressBar _progressBar;

    #endregion

    #region properties
  
    public double Percent
    {
      get { return _percent; }
      set { _percent = value; OnPropertyChanged("Percent"); }
    }

    public double Diameter
    {
      get { return _diameter; }
      set { _diameter = value; OnPropertyChanged("Diameter");  }
    }

    public double Radius
    {
      get { return _radius; }
      set { _radius = value; OnPropertyChanged("Radius"); }
    }

    public double InnerRadius
    {
      get { return _innerRadius; }
      set { _innerRadius = value; OnPropertyChanged("InnerRadius"); }
    }

    public double CentreX
    {
      get { return _centreX; }
      set { _centreX = value; OnPropertyChanged("CentreX"); }
    }

    public double CentreY
    {
      get { return _centreY; }
      set { _centreY = value; OnPropertyChanged("CentreY"); }
    }

    public double Angle
    {
      get { return _angle; }
      set { _angle = value; OnPropertyChanged("Angle"); }
    }

    public double HoleSizeFactor
    {
      get { return _holeSizeFactor; }
      set { _holeSizeFactor = value; ComputeViewModelProperties();  }
    }

    #endregion


    /// <summary>
    /// Re-computes the various properties that the elements in the template bind to.
    /// </summary>
    protected virtual void ComputeViewModelProperties()
    {
      if (_progressBar == null)
        return;

      Angle = (_progressBar.Value - _progressBar.Minimum) * 360 / (_progressBar.Maximum - _progressBar.Minimum);
      CentreX = _progressBar.ActualWidth / 2;
      CentreY = _progressBar.ActualHeight / 2;
      Radius = Math.Min(CentreX, CentreY);
      Diameter = Radius * 2;
      InnerRadius = Radius * HoleSizeFactor;
      Percent = Angle / 360;
    }

    /// <summary>
    /// Add handlers for the updates on various properties of the ProgressBar
    /// </summary>
    private void SetProgressBar(ProgressBar progressBar)
    {
      _progressBar = progressBar;
      _progressBar.SizeChanged += (s, e) => ComputeViewModelProperties();
      RegisterForNotification("Value", progressBar, (d,e) => ComputeViewModelProperties());
      RegisterForNotification("Maximum", progressBar, (d, e) => ComputeViewModelProperties());
      RegisterForNotification("Minimum", progressBar, (d, e) => ComputeViewModelProperties());

      ComputeViewModelProperties();
    }
    

    /// Add a handler for a DP change
    /// see: http://amazedsaint.blogspot.com/2009/12/silverlight-listening-to-dependency.html
    private void RegisterForNotification(string propertyName, FrameworkElement element, PropertyChangedCallback callback)
    {

      //Bind to a dependency property  
      Binding b = new Binding(propertyName) { Source = element };
      var prop = System.Windows.DependencyProperty.RegisterAttached(
          "ListenAttached" + propertyName,
          typeof(object),
          typeof(UserControl),
          new PropertyMetadata(callback));

      element.SetBinding(prop, b);
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string property)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(property));
      }
    }

    #endregion
  }
}
