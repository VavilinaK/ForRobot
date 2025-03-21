using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;

using HelixToolkit.Wpf;


namespace ForRobot.Model.File3D
{
    public sealed class VisualViewModel : BaseClass
    {
        #region Private variables

        private bool _isVisible = true;

        private DependencyObject _element;

        private static readonly Material TransparentMaterial = new DiffuseMaterial(Brushes.Transparent);

        #endregion Private variables

        #region Public variables

        public IEnumerable<VisualViewModel> Children
        {
            get
            {
                if (this._element is ModelVisual3D mv)
                {
                    if (mv.Content != null)
                    {
                        yield return new VisualViewModel(mv.Content);
                    }

                    foreach (var mc in mv.Children)
                    {
                        yield return new VisualViewModel(mc);
                    }
                }

                if (this._element is Model3DGroup mg)
                {
                    foreach (var mc in mg.Children)
                    {
                        yield return new VisualViewModel(mc);
                    }
                }

                if (this._element is GeometryModel3D gm)
                {
                    yield return new VisualViewModel(gm.Geometry);
                }

                foreach (DependencyObject c in LogicalTreeHelper.GetChildren(this._element))
                {
                    yield return new VisualViewModel(c);
                }
            }
        }

        public bool IsVisible { get => this.IsVisible; set => Set(ref this._isVisible, value); }

        public string Name { get => this._element.GetName(); }

        public string TypeName { get => this._element.GetType().Name; }

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

        #endregion

        public VisualViewModel(DependencyObject e)
        {
            this._element = e;
        }

        private void UpdateVisibility()
        {
            //switch (SceneObject)
            //{
            //    case GeometryModel3D geometryModel:
            //        if (_originalMaterial == null)
            //            _originalMaterial = geometryModel.Material;

            //        geometryModel.Material = _isVisible ? _originalMaterial : TransparentMaterial;
            //        break;

            //    case GroupModel3D group:
            //        foreach (var child in Children)
            //            child.IsVisible = _isVisible;
            //        break;
            //}
        }

        public override string ToString() => this._element.GetType().ToString();
    }
}
