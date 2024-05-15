using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace ForRobot.Model
{
    public class Detal : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Коллекция рёбер
        /// </summary>
        private ObservableCollection<Rebro> _rebraDetal;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        #endregion

        #region Public variables

        //[JsonIgnore]
        //public string Name
        //{
        //    get
        //    {
        //        if ((int)DetalType == 1) { return "Plita"; }
        //        else if ((int)DetalType == 3) { return "Stringer"; }
        //        else { return "Treygolnik"; }
        //    }
        //}

        [JsonIgnore]
        public ObservableCollection<Rebro> RebraDetal
        {
            get => _rebraDetal ?? (_rebraDetal = FillCollection());
            set => _rebraDetal = value;
        }

        #region Virtual

        [JsonIgnore]
        public virtual string Json { get => JsonSerializer.Serialize<Detal>(this, options); }
               
        //[JsonIgnore]
        ///// <summary>
        ///// Тип детали
        ///// </summary>
        //public virtual DetalType DetalType { get; private set; }

        [JsonIgnore]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int SumReber { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal Long { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal Hight { get; set; }

        ///// <summary>
        ///// Ширина
        ///// </summary>
        //public virtual decimal Wight { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Расстояние до осевой линии первого ребра сверху
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public virtual decimal DistanceBetween { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Расстояние торца ребра в начале
        /// </summary>
        public virtual decimal DistanceToStart { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Расстояние торца ребра в конце
        /// </summary>
        public virtual decimal DistanceToEnd { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Роспуск вначале
        /// </summary>
        public virtual decimal DissolutionStart { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Роспуск вконце
        /// </summary>
        public virtual decimal DissolutionEnd { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal ThicknessPlita { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal ThicknessRebro { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public virtual decimal SeamsOverlap { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public virtual decimal TechOffsetSeamStart { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public virtual decimal TechOffsetSeamEnd { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public virtual int WildingSpead { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Номер сварачной программы
        /// </summary>
        public virtual int ProgramNom { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Общее изображение детали
        /// </summary>
        public virtual BitmapImage GenericImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Изображение рёбер
        /// </summary>
        public virtual BitmapImage RebraImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Продольная привязка
        /// </summary>
        public virtual Privyazka LongitudinalPrivyazka { get; set; } = Privyazka.FromLeftToRight;

        [JsonIgnore]
        /// <summary>
        /// Поперечная привязка
        /// </summary>
        public virtual Privyazka TransversePrivyazka { get; set; } = Privyazka.FromLeftToRight;

        #endregion

        #endregion

        #region Event

        public virtual event EventHandler Change;

        //public virtual event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructors

        public Detal() { }

        public Detal(DetalType type)
        {
            switch (type)
            {
                case DetalType.Plita:
                    this.SumReber = 5;
                    this.Long = 120000;
                    this.Hight = 200;
                    this.DistanceToFirst = 830;
                    this.DistanceBetween = 600;
                    this.DistanceToStart = 250;
                    this.DistanceToEnd = 250;
                    this.DissolutionStart = 200;
                    this.DissolutionEnd = 200;
                    this.ThicknessPlita = 16;
                    this.ThicknessRebro = 16;
                    this.SearchOffsetStart = 60;
                    this.SearchOffsetEnd = 60;
                    this.SeamsOverlap = 5;
                    this.TechOffsetSeamStart = 3;
                    this.TechOffsetSeamEnd = 2;
                    this.WildingSpead = 38;
                    this.ProgramNom = 1;
                    break;

                case DetalType.WithBevels:
                    this.SumReber = 2;
                    this.Long = 11166;
                    this.Hight = 200;
                    this.DistanceToFirst = 300;
                    this.DistanceBetween = 600;
                    this.DistanceToStart = 801;
                    this.DistanceToEnd = 201;
                    this.DissolutionStart = 0;
                    this.DissolutionEnd = 201;
                    this.ThicknessPlita = 16;
                    this.ThicknessRebro = 16;
                    this.SearchOffsetStart = 100;
                    this.SearchOffsetEnd = 60;
                    this.SeamsOverlap = 5;
                    this.TechOffsetSeamStart = 3;
                    this.TechOffsetSeamEnd = 2;
                    this.WildingSpead = 31;
                    this.ProgramNom = 1;
                    break;
            }
        }
        
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
