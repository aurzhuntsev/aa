using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AudioMark.Common;
using ReactiveUI;

namespace AudioMark.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private static Dictionary<Type, Type> _defaultViewModelsCache = new Dictionary<Type, Type>();

        static ViewModelBase()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ViewModelBase).IsAssignableFrom(type) && !type.IsAbstract &&
                    type.CustomAttributes.Any(attr => attr.AttributeType == typeof(DefaultViewModelAttribute)))
                .ToList();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute(typeof(DefaultViewModelAttribute)) as DefaultViewModelAttribute;                
                if (!type.GetConstructors().Any(ctor => ctor.GetParameters().Count() == 1 && ctor.GetParameters().Any(p => p.ParameterType == attr.ModelType)))
                {
                    throw new InvalidOperationException($"Missing constructor with a parameter of type ${attr.ModelType}");
                }

                if (_defaultViewModelsCache.ContainsKey(attr.ModelType))
                {
                    throw new InvalidOperationException($"Ambiguous default view model for type {attr.ModelType}");
                }

                _defaultViewModelsCache.Add( attr.ModelType, type);
            }
        }

        public static ViewModelBase DefaultForModel<T>(T model)
        {
            if (!_defaultViewModelsCache.ContainsKey(model.GetType()))
            {
                throw new KeyNotFoundException($"A default view model is not registered for type {model.GetType()}");
            }

            return (ViewModelBase)Activator.CreateInstance(_defaultViewModelsCache[model.GetType()], new object[] { model });
        }
    }
}
