using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proba_Wisual
{
    internal class MainViewsModel : BaseViewModel

    {
        public string FirstName { get; set; }

        public List<Plita> DetalList = new List<Plita> {
        new Plita (Status.Plita),
        new Plita (Status.Stringer),
        new Plita (Status.Treygolnik)};


        private ObservableCollection<Plita> _detalCollection;
        public ObservableCollection<Plita> DetalCollection { get => _detalCollection ?? (_detalCollection = new ObservableCollection<Plita>(DetalList)); }


   
    }
}
