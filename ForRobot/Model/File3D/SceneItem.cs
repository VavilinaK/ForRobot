using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using HelixToolkit.Wpf;
//using HelixToolkit.Wpf;

namespace ForRobot.Model.File3D
{
    public sealed class SceneItem : BaseClass
    {
        #region Private variables

        private bool _isVisible = true;

        //private DependencyObject _element;

        private object _element;

        private Material _originalMaterial;
        private static readonly Material TransparentMaterial = new DiffuseMaterial(Brushes.Transparent);

        #endregion Private variables

        #region Public variables

        //public string Name { get => this._element.GetName(); }

        public object SceneObject { get => this._element; set => Set(ref this._element, value); }
        public string Name { get; set; }
        public string TypeName { get => this._element.GetType().Name; }

        //public bool IsVisible
        //{
        //    get => this.IsVisible;
        //    set
        //    {
        //        //if(this._isVisible != value)
        //        //    this.UpdateVisibility(this._isVisible);

        //        Set(ref this._isVisible, value);
        //    }
        //}

        public Brush Brush
        {
            get
            {
                var elementType = this._element.GetType();

                if (elementType == typeof(ModelVisual3D))
                    return Brushes.Orange;

                if (elementType == typeof(GeometryModel3D))
                    return Brushes.Green;

                if (elementType == typeof(Model3DGroup))
                    return Brushes.Blue;

                if (elementType == typeof(Visual3D))
                    return Brushes.Gray;

                if (elementType == typeof(Model3D))
                    return Brushes.Black;

                return null;
            }
        }

        public SceneItem this[int index] { get => this.Children[index]; set => this.Children[index] = value; }
        
        public ObservableCollection<SceneItem> Children
        {
            get;
            //get
            //{
            //    if (this._element is ModelVisual3D mv)
            //    {
            //        if (mv.Content != null)
            //        {
            //            yield return new SceneItem(mv.Content);
            //        }

            //        foreach (var mc in mv.Children)
            //        {
            //            yield return new SceneItem(mc);
            //        }
            //    }

            //    if (this._element is Model3DGroup mg)
            //    {
            //        foreach (var mc in mg.Children)
            //        {
            //            yield return new SceneItem(mc);
            //        }
            //    }

            //    if (this._element is GeometryModel3D gm)
            //    {
            //        yield return new SceneItem(gm.Geometry);
            //    }

            //    //foreach (DependencyObject c in LogicalTreeHelper.GetChildren(this._element))
            //    //{
            //    //    yield return new SceneItem(c);
            //    //}
            //}
        } = new ObservableCollection<SceneItem>();

        #endregion

        public SceneItem()
        {
            //this._element = e;
        }

        private void UpdateVisibility(bool isVisible)
        {
            switch (SceneObject)
            {
                //case Visual3D visual3D:
                //    if (_originalMaterial == null)
                //        _originalMaterial = visual3D;
                //    break;

                case GeometryModel3D geometryModel:
                    if (_originalMaterial == null)
                        _originalMaterial = geometryModel.Material;

                    geometryModel.Material = isVisible ? _originalMaterial : TransparentMaterial;
                    break;

                case Model3DGroup group:
                    //foreach (var child in Children)
                    //    child.IsVisible = isVisible;
                    break;
            }
        }

        public static void AddChildren(SceneItem parent, object element)
        {
            var item = new SceneItem()
            {
                Name = element.GetType().Name,
                SceneObject = element
            };

            parent.Children.Add(item);

            if (element is Model3DGroup group)
                foreach (var child in group.Children)
                    AddChildren(item, child);
        }

        //public override string ToString() => this._element.GetType().ToString();
    }
}
