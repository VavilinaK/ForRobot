using System;
using System.Windows.Media.Imaging;
using System.Configuration;

using Newtonsoft.Json;

using ForRobot.Libr;

namespace ForRobot.Model.Detals
{
    public class Detal : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Экземпляр детали из app.config
        /// </summary>
        private ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection Config { get; set; }

        #endregion

        #region Public variables

        #region Virtual
        
        public virtual string Json { get; }
        
        public virtual string JsonForSave { get; }
        
        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual string DetalType { get; }

        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int SumReber { get; set; }
        
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal Long { get; set; }
        
        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal Hight { get; set; }
        
        /// <summary>
        /// Ширина настила
        /// </summary>
        public virtual decimal Wight { get; set; }
        
        /// <summary>
        /// Расстояние до осевой линии первого ребра сверху
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; }
        
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public virtual decimal DistanceBetween { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра слева
        /// </summary>
        public virtual decimal IdentToLeft { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра справа
        /// </summary>
        public virtual decimal IdentToRight { get; set; }
        
        /// <summary>
        /// Роспуск в начале
        /// </summary>
        public virtual decimal DissolutionStart { get; set; }
        
        /// <summary>
        /// Роспуск в конце
        /// </summary>
        public virtual decimal DissolutionEnd { get; set; }
        
        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal ThicknessPlita { get; set; }
        
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal ThicknessRebro { get; set; }
        
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; }
        
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; }
        
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public virtual decimal SeamsOverlap { get; set; }
        
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public virtual decimal TechOffsetSeamStart { get; set; }
        
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public virtual decimal TechOffsetSeamEnd { get; set; }
        
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public virtual int WildingSpead { get; set; }
        
        /// <summary>
        /// Номер сварочной программы
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
                case ForRobot.Model.Detals.DetalType.Plita:
                    this.Config = ConfigurationManager.GetSection("plita") as ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection;
                    this.Long = Config.Long;
                    this.Wight = Config.Wight;
                    this.Hight = Config.Hight;
                    this.DistanceToFirst = Config.DistanceToFirst;
                    this.DistanceBetween = Config.DistanceBetween;
                    this.IdentToLeft = Config.DistanceToStart;
                    this.IdentToRight = Config.DistanceToEnd;
                    this.DissolutionStart = Config.DissolutionStart;
                    this.DissolutionEnd = Config.DissolutionEnd;
                    this.ThicknessPlita = Config.ThicknessPlita;
                    this.ThicknessRebro = Config.ThicknessRebro;
                    this.SearchOffsetStart = Config.SearchOffsetStart;
                    this.SearchOffsetEnd = Config.SearchOffsetEnd;
                    this.SeamsOverlap = Config.SeamsOverlap;
                    this.TechOffsetSeamStart = Config.TechOffsetSeamStart;
                    this.TechOffsetSeamEnd = Config.TechOffsetSeamEnd;
                    this.WildingSpead = Config.WildingSpead;
                    this.ProgramNom = Config.ProgramNom;
                    this.SumReber = Config.SumReber;
                    break;
            }
        }

        #endregion

        #region Private functions

        #endregion

        #region Public functions

        //public virtual async Task OnChange(Func<object, EventArgs, Task> func)
        //{
        //    Func<object, EventArgs, Task> handler = func;

        //    if (handler == null)
        //        return;

        //    Delegate[] invocationList = handler.GetInvocationList();
        //    Task[] handlerTasks = new Task[invocationList.Length];

        //    for (int i = 0; i < invocationList.Length; i++)
        //    {
        //        handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
        //    }

        //    await Task.WhenAll(handlerTasks);
        //}

        #endregion
    }
}
