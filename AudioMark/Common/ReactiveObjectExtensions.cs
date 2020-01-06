using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AudioMark.Common
{
    public static class ReactiveObjectExtensions
    {
        public static void RaiseAndSetIfPropertyChanged<T>(this ReactiveObject reactiveObject, Expression<Func<T>> selector, T value, string propertyName = null)
        {
            var constExpression = Expression.Constant(value);
            var assignExpression = Expression.Assign(selector.Body, constExpression);
            var lambda = Expression.Lambda(assignExpression);

            lambda.Compile().DynamicInvoke();

            reactiveObject.RaisePropertyChanged(propertyName ?? selector.Body.GetMemberInfo().Name);
        }
    }
}
