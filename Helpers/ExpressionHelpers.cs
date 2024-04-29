using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Spring.Helpers
{
    /// <summary>
    /// This class was made to force the lamda Func to compile first then return 
    /// the propwerty after the compilation [Advanced way to handle any generic funcs]
    /// </summary>
    public static class ExpressionHelpers
    {
        #region Helpers Methods
        /// <summary>
        /// Compiles the Expression and getting the reutn value of the function.
        /// </summary>
        /// <typeparam name="T">DataType</typeparam>
        /// <param name="lmda">whole func used to be compiled</param>
        /// <returns></returns>
        public static T GetCompliledValue<T>(this Expression<Func<T>> lmda)
        {
            //invoke to getter
            return lmda.Compile().Invoke();
        }
        /// <summary>
        /// Set value to property particular property with particular type.
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="lmda">whole fun to be compiled</param>
        /// <param name="val">value to set</param>
        public static void SetCompliledValue<T>(this Expression<Func<T>> lmda, T val)
        {
            //convert (void)=>d.x to d.x
            var bodyOfLmda = (lmda as LambdaExpression).Body;
            //get x member = property
            var memberInsideLmbda = bodyOfLmda as MemberExpression;
            //invoke property
            var target = Expression.Lambda(memberInsideLmbda.Expression).Compile().DynamicInvoke();
            //set new value by knowing x info member with the invoked target with new val
            var propertyInfo = (PropertyInfo)memberInsideLmbda.Member;
            //setter
            propertyInfo.SetValue(target, val);
        }
        #endregion
    }
}
