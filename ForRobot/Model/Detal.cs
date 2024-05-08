using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

        [JsonIgnore]
        public string Name
        {
            get
            {
                if ((int)DetalType == 1) { return "Plita"; }
                else if ((int)DetalType == 3) { return "Stringer"; }
                else { return "Treygolnik"; }
            }
        }

        [JsonIgnore]
        public ObservableCollection<Rebro> RebraDetal
        {
            get => _rebraDetal ?? (_rebraDetal = FillCollection());
            set => _rebraDetal = value;
        }

        #region Virtual

        [JsonIgnore]
        public virtual string Json { get => JsonSerializer.Serialize<Detal>(this, options); }

        [JsonIgnore]
        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual DetalType DetalType { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int SumReber { get; set; } = 5;

        [JsonIgnore]
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal Long { get; set; } = 120000;

        [JsonIgnore]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal Hight { get; set; } = 200;

        ///// <summary>
        ///// Ширина
        ///// </summary>
        //public virtual decimal Wight { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Расстояние до осевой линии первого ребра сверху
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; } = 300;

        [JsonIgnore]
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public virtual decimal DistanceBetween { get; set; } = 600;

        [JsonIgnore]
        /// <summary>
        /// Расстояние торца ребра в начале
        /// </summary>
        public virtual decimal DistanceToStart { get; set; } = 250;

        [JsonIgnore]
        /// <summary>
        /// Расстояние торца ребра в конце
        /// </summary>
        public virtual decimal DistanceToEnd { get; set; } = 250;

        [JsonIgnore]
        /// <summary>
        /// Роспуск вначале
        /// </summary>
        public virtual decimal DissolutionStart { get; set; } = 200;

        [JsonIgnore]
        /// <summary>
        /// Роспуск вконце
        /// </summary>
        public virtual decimal DissolutionEnd { get; set; } = 200;

        [JsonIgnore]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal ThicknessPlita { get; set; } = 16;

        [JsonIgnore]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal ThicknessRebro { get; set; } = 16;

        [JsonIgnore]
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; } = 60;

        [JsonIgnore]
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; } = 60;

        [JsonIgnore]
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public virtual decimal SeamsOverlap { get; set; } = 5;

        [JsonIgnore]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public virtual decimal TechOffsetSeamStart { get; set; } = 2;

        [JsonIgnore]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public virtual decimal TechOffsetSeamEnd { get; set; } = 3;

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
