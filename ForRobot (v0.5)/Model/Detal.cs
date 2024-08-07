using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Text.Json.Serialization;

//using ForRobot.Libr;

namespace ForRobot.Model
{
    public class Detal : BaseClass
    {
        #region Private variables

        private int _sumReber = 4;

        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        private ObservableCollection<Rebro> _rebraDetal;

        #endregion

        #region Public variables

        public string Name
        {
            get
            {
                if ((int)DetalType == 1) { return "Plita"; }
                else if ((int)DetalType == 3) { return "Stringer"; }
                else { return "Treygolnik"; }
            }
        }

        public ObservableCollection<Rebro> RebraDetal
        {
            get => _rebraDetal ?? (_rebraDetal = FillCollection());
            set => _rebraDetal = value;
        }

        #region Virtual

        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual DetalType DetalType { get; set; }

        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int SumReber
        {
            get => _sumReber;
            set
            {
                _sumReber = value;
            }
        }

        /// <summary>
        /// Длина
        /// </summary>
        public virtual decimal Long { get; set; }

        /// <summary>
        /// Высота
        /// </summary>
        public virtual decimal Hight { get; set; }

        /// <summary>
        /// Ширена
        /// </summary>
        public virtual decimal Wight { get; set; }

        /// <summary>
        /// Отступ в начале
        /// </summary>
        public virtual decimal IndentionStart { get; set; }

        /// <summary>
        /// Отступ в конце
        /// </summary>
        public virtual decimal IndentionEnd { get; set; }

        /// <summary>
        /// Роспуск в начале
        /// </summary>
        public virtual decimal DissolutionStart { get; set; }

        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public virtual decimal DissolutionEnd { get; set; }

        /// <summary>
        /// Расстояние до первого ребра
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; }

        /// <summary>
        /// Расстояние между ребрами
        /// </summary>
        public virtual decimal DistanceBetween { get; set; }

        /// <summary>
        /// Толщина плиты
        /// </summary>
        public virtual decimal ThicknessPlita { get; set; }

        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal ThicknessRebro { get; set; }

        /// <summary>
        /// Смещение поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; }

        /// <summary>
        /// Смещение поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; }

        /// <summary>
        /// Общее изображение детали
        /// </summary>
        public virtual BitmapImage GenericImage { get; set; }

        /// <summary>
        /// Изображение рёбер
        /// </summary>
        public virtual BitmapImage RebraImage { get; set; }

        /// <summary>
        /// Продольная привязка
        /// </summary>
        public virtual Privyazka LongitudinalPrivyazka { get; set; } = Privyazka.FromLeftToRight;

        /// <summary>
        /// Поперечная привязка
        /// </summary>
        public virtual Privyazka TransversePrivyazka { get; set; } = Privyazka.FromLeftToRight;

        #endregion

        #endregion

        #region Event

        //public virtual event EventHandler Change;
        
        public virtual event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructor

        public Detal() { }

        #endregion

        #region Private functions

        private ObservableCollection<Rebro> FillCollection()
        {
            Rebro rebro;
            List<Rebro> rebros = new List<Rebro>();
            ObservableCollection<Rebro> collection;
            for (int i = 0; i < SumReber; i++)
            {
                rebro = new Rebro(ThicknessRebro, DissolutionStart, DissolutionEnd);
                rebros.Add(rebro);
            }
            collection = new ObservableCollection<Rebro>(rebros);
            return collection;
        }

        #endregion

        #region Public functions

        public virtual async Task OnChange(Func<object, EventArgs, Task> func)
        {
            Func<object, EventArgs, Task> handler = func;

            if (handler == null)
                return;

            Delegate[] invocationList = handler.GetInvocationList();
            Task[] handlerTasks = new Task[invocationList.Length];

            for (int i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
            }

            await Task.WhenAll(handlerTasks);
        }

        #endregion
    }
}
