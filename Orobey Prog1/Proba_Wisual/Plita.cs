using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Proba_Wisual
{
    public enum Status {

    None = 0,
    //изображения нет

    Plita = 1,
    //плита

    Stringer = 2,
    //стунгер

    Treygolnik = 3,
    //треугольник
    }

    public class Plita
    {
        public Plita(Status status) { Status = status; }
        public string Name { get
            {
                if (Status == 1) { return "Plita"; }
                else if (Status == 3) { return "Stringer"; }
                else { return "Treygolnik"; }
            }
        }

         
        private Rebra[] rebras;
        public Rebra[] Rebras
        {
            get => rebras ?? (rebras = new Rebra[0]);
            set => Rebras = value;
            



        }

     public Status Status { get; set; }= Status.None;
     public int Rebra {  get; set; }
     public int Long { get; set; }
     public int Hight { get; set; }
     public int Wight { get; set; }
     public int DistanceR { get; set; }
     public int DistanceG { get; set; }
     public int IndentionsStart { get; set; }
     public int IndentionsEnd { get; set; }
     public int DissolutionStart { get; set; }
     public int DissolutionEnd { get; set; }
     
      

    }
}
