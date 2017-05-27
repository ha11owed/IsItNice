using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ClientLogic.ViewModel
{
    /// <summary>
    /// Base class for ViewModels
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private SynchronizationContext synchronizationContext = SynchronizationContext.Current;

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="entity">An entity with a propert with the same name as the last parameter and with both a getter and a setter</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetEntityProperty<T>(object entity, T value, [CallerMemberName] String propertyName = null)
        {
            Type type = entity.GetType();
            var entityProperty = type.GetRuntimeProperty(propertyName);
            var oldValue = entityProperty.GetValue(entity);

            if (object.Equals(oldValue, value)) return false;

            entityProperty.SetValue(entity, value);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected T GetEntityProperty<T>(object entity, [CallerMemberName] String propertyName = null)
        {
            Type type = entity.GetType();
            var entityProperty = type.GetRuntimeProperty(propertyName);
            var value = entityProperty.GetValue(entityProperty);
            return (T)value;
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Execute an action in the creator context (should be GUI)
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteInCreatorContext(Action<BaseViewModel> action)
        {
            synchronizationContext.Post((x) =>
            {
                action(x as BaseViewModel);
            }, this);
        }

        internal virtual void OnEntityChanged()
        {
            var properties = GetType().GetRuntimeProperties();
            foreach (var p in properties)
            {
                OnPropertyChanged(p.Name);
            }
        }
    }
}
