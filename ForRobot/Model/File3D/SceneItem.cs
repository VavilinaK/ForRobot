using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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

        private DependencyObject _element;
        //private object _element;
        //private MeshGeometry3D _element;

        private Material _originalMaterial;

        #endregion Private variables

        #region Public variables

        //public string Name { get => this._element.GetName(); }

        public DependencyObject SceneObject
        {
            get => this._element;
            set => Set(ref this._element, value, false);
        }
        //public object SceneObject { get => this._element; set => Set(ref this._element, value); }
        //public MeshGeometry3D SceneObject { get => this._element; set => Set(ref this._element, value); }
        public static readonly Material TransparentMaterial = new DiffuseMaterial(Brushes.Transparent);

        public string Name { get; set; }
        public string TypeName { get => this.SceneObject.GetType().Name; }

        public bool IsVisible
        {
            get => this._isVisible;
            set
            {
                Set(ref this._isVisible, value, false);

                if(this._isVisible)
                    this.VisibleEvent?.Invoke(this, null);
                else
                    this.HiddenEvent?.Invoke(this, null);

                //this.UpdateVisibility(this._isVisible);
            }
        }

        //public Brush Brush
        //{
        //    get
        //    {
        //        var elementType = this._element.GetType();

        //        if (elementType == typeof(ModelVisual3D))
        //            return Brushes.Orange;

        //        if (elementType == typeof(GeometryModel3D))
        //            return Brushes.Green;

        //        if (elementType == typeof(Model3DGroup))
        //            return Brushes.Blue;

        //        if (elementType == typeof(Visual3D))
        //            return Brushes.Gray;

        //        if (elementType == typeof(Model3D))
        //            return Brushes.Black;

        //        return null;
        //    }
        //}

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

        public event EventHandler VisibleEvent;
        public event EventHandler HiddenEvent;

        //public IEnumerable<MeshGeometry3D> Meshes
        //{
        //    get
        //    {
        //        ObservableCollection<MeshGeometry3D> meshes = new ObservableCollection<MeshGeometry3D>();
        //        switch (this.SceneObject)
        //        {
        //            case Model3DGroup subGroup:
        //                foreach (var a in ExtractMeshes(subGroup))
        //                {
        //                    meshes.Add(a);
        //                }
        //                break;

        //            case GeometryModel3D geomModel:
        //                if (geomModel.Geometry is MeshGeometry3D mesh)
        //                    meshes.Add(mesh);
        //                break;

        //            default:
        //                break;
        //        }
        //        return meshes;
        //    }
        //}

        #endregion

        public SceneItem() { }

        public SceneItem(DependencyObject visual3D)
        {
            this.SceneObject = visual3D;

            if (string.IsNullOrEmpty(visual3D.GetName()))
                this.Name = this.TypeName;
            else
                this.Name = visual3D.GetName();

            this.AddChildren(this.SceneObject);
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

        private void AddChildren(object element)
        {
            switch (element)
            {
                case System.Windows.Media.Media3D.ModelVisual3D modelVisual3D:
                    foreach (var item in modelVisual3D.Children)
                    {
                        this.Children.Add(new SceneItem(item));
                    }
                    break;

                case Model3DGroup model3DGroup:
                    foreach (var item in model3DGroup.Children)
                    {
                        this.Children.Add(new SceneItem(item));
                    }
                    break;

                case GeometryModel3D geometryModel3D:
                    break;

                default:
                    foreach (DependencyObject item in LogicalTreeHelper.GetChildren(this.SceneObject))
                    {
                        this.Children.Add(new SceneItem(item));
                    }
                    break;
            }
        }

        //public static void AddChildren(SceneItem parent, MeshGeometry3D element)
        //{
        //    var item = new SceneItem()
        //    {
        //        Name = element.GetType().Name,
        //        SceneObject = element
        //    };

        //    parent.Children.Add(item);

        //    if (element is Model3DGroup group)
        //        foreach (var child in group.Children)
        //            AddChildren(item, child);
        //}

        public override string ToString() => this._element.GetType().ToString();

        //public static IEnumerable<MeshGeometry3D> ExtractMeshes(Model3DGroup group)
        //{
        //    foreach (var model in group.Children)
        //    {
        //        if (model is Model3DGroup subGroup)
        //        {
        //            foreach (var mesh in ExtractMeshes(subGroup))
        //                yield return mesh;
        //        }
        //        else if (model is GeometryModel3D geomModel)
        //        {
        //            if (geomModel.Geometry is MeshGeometry3D mesh)
        //                yield return mesh;
        //        }
        //    }
        //}
    }
}
