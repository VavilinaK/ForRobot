using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Models.File3D;
using ForRobot.Models.Detals;
using ForRobot.Libr.UndoRedo;
using ForRobot.Libr.Strategies.ModelingStrategies;

namespace ForRobot.Libr.Services
{
    public class ChangeService : BaseClass, IDisposable
    {
        private File3D _pendingUpdate;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _updateLock = new object();
        private readonly int _debounceDelayMs = 150;
        private readonly ForRobot.Libr.Services.ModelingService _modelingService;
        
        public ChangeService()
        {
            double scaleFactor = (double)ForRobot.Models.Settings.Settings.ScaleFactor;
            var strategies = new List<IDetalModelingStrategy>
            {
                new PlateModelingStrategy(scaleFactor)
            };
            this._modelingService = new Services.ModelingService(strategies, scaleFactor);
        }

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Models.Detals.Detal"/> оповещающий о изменению его свойств
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Properties(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            Models.File3D.File3D file3D = sender as Models.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.CurrentDetal));

            if (file3D.CurrentDetal is ForRobot.Models.Detals.Plita)
            {
                Plita plita = file3D.CurrentDetal as ForRobot.Models.Detals.Plita;
                RaisePropertyChanged(nameof(plita.SelectedWeldingSchema));
                RaisePropertyChanged(nameof(plita.WeldingSchema));
            }

            if (e.OldValue != null)
            {
                var command = new PropertyChangeCommand<Detal>(file3D,
                                                               nameof(File3D.CurrentDetal),
                                                               e.OldValue,
                                                               e.NewValue,
                                                               $"Изменение детали: {e.OldValue?.GetType().Name} -> {e.NewValue?.GetType().Name}");
                file3D.AddUndoCommand(command);
            }
        }

        /// <summary>
        /// Делегат изменения <see cref="ForRobot.Models.Detals.Detal"/>, изменяющий CurrentModel в классе <see cref="File3D"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDetalChanged_Modeling(object sender, Libr.ValueChangedEventArgs<Detal> e)
        {
            File3D file = sender as File3D;
            this._pendingUpdate = file;
            lock (this._updateLock)
            {
                this._cancellationTokenSource?.Cancel();
                this._cancellationTokenSource?.Dispose();
                this._cancellationTokenSource = new CancellationTokenSource();
            }
            _ = DebouncedUpdateAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Делегат изменения CurrentModel в <see cref="ForRobot.Models.File3D.File3D"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleModelChanged(object sender, EventArgs e)
        {
            Models.File3D.File3D file3D = sender as Models.File3D.File3D;
            RaisePropertyChanged(nameof(file3D.CurrentModel));
        }

        /// <summary>
        /// Делегат изменения свойств <see cref="ForRobot.Models.File3D.File3D"/>, изменяющий CurrentModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleFileChange(object sender, EventArgs e)
        {
            Models.File3D.File3D file3D = sender as Models.File3D.File3D;
            RaisePropertyChanged(nameof(file3D));
        }

        #region Async functions

        private async Task DebouncedUpdateAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(_debounceDelayMs, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            File3D file = this._pendingUpdate;
            if (file?.CurrentDetal == null) return;
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var model = _modelingService.Get3DScene(file.CurrentDetal);
                            file.CurrentModel.Children.Clear();
                            file.CurrentModel.Children.Add(model);
                        }
                        catch (Exception ex)
                        {
                            App.Current.Logger.Error(ex, "Ошибка создания модели: " + ex.Message);
                        }
                    }
                }));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, "Ошибка отложенного обновления модели:\n" + ex.Message);
            }
        }
        
        #endregion Async functions

        #region Implementations of IDisposable

        ~ChangeService() => Dispose();

        public void Dispose()
        {
            lock (this._updateLock)
            {
                this._cancellationTokenSource?.Cancel();
                this._cancellationTokenSource?.Dispose();
            }
        }

        #endregion
    }
}
