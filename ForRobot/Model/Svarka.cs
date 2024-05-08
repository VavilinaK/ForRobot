using System;
//using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ForRobot.Model
{
    public class Svarka : BaseClass
    {
        #region Private variables

        private int _weldingSpead = 38;
        private int _programNom = 1;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        #endregion

        #region Public variables

        [JsonIgnore]
        public string Json { get => JsonSerializer.Serialize<Svarka>(this, options); }

        [JsonPropertyName("velocity")]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public virtual int WildingSpead
        {
            get => _weldingSpead;
            set
            {
                _weldingSpead = value;
                if (!EventArgs.Equals(this.Change, null))
                    this.Change.Invoke(this, null);
            }
        }

        [JsonPropertyName("job")]
        /// <summary>
        /// Номер сварачной программы
        /// </summary>
        public virtual int ProgramNom
        {
            get => _programNom;
            set
            {
                _programNom = value;
                if (!EventArgs.Equals(this.Change, null))
                    this.Change.Invoke(this, null);
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// Событие изменения свойства класса
        /// </summary>
        public virtual event EventHandler Change;

        //public virtual event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructor

        public Svarka() { }

        #endregion

        #region Public function

        //public async Task OnChange()
        //{
        //    Func<object, EventArgs, Task> handler = Change;

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
