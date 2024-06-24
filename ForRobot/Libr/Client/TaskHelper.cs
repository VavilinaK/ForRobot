using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ForRobot.Libr.Client
{
    public static class TaskHelper
    {
        // returns true if task
        public static bool TryAwaitTask(object task, out object result)
        {
            result = null;

            if (task is null)
            {
                return false;
            }

            // ValueTask<T>
            var returnType = task.GetType();
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var asTaskMethod = task.GetType().GetMethod("AsTask");
                task = asTaskMethod.Invoke(task, null);
            }

            if (task is ValueTask valueTask)
                task = valueTask.AsTask();

            // Task or Task<T>
            if (task is Task t)
            {
                if (TryGetTaskResult(t, out var taskResult))
                    result = taskResult;

                return true;
            }

            return false;
        }

        private static bool TryGetTaskResult(Task task, out object result)
        {
            result = null;

            var voidTaskType = typeof(Task<>).MakeGenericType(Type.GetType("System.Threading.Tasks.VoidTaskResult"));
            if (voidTaskType.IsInstanceOfType(task))
            {
                task.GetAwaiter().GetResult();
                return false;
            }

            var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (property is null)
            {
                return false;
            }

            result = property.GetValue(task);
            return true;
        }
    }
}