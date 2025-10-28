using UnityEngine;

namespace AudioSystem {
    /// <summary>
    /// Extension methods for GameObject to make component management easier.
    /// These helpers reduce boilerplate code when working with components.
    /// </summary>
    public static class GameObjectExtensions {
        /// <summary>
        /// Gets an existing component or adds it if it doesn't exist.
        /// Saves you from writing "GetComponent, check if null, AddComponent" every time.
        /// Very useful when you need a component but aren't sure if it's already there.
        /// </summary>
        /// <typeparam name="T">The type of component to get or add (must be a Component).</typeparam>
        /// <param name="gameObject">The GameObject to get/add the component on.</param>
        /// <returns>The existing component if found, or a newly added component.</returns>
        /// <example>
        /// // Instead of this:
        /// AudioSource source = gameObject.GetComponent&lt;AudioSource&gt;();
        /// if (source == null) source = gameObject.AddComponent&lt;AudioSource&gt;();
        /// 
        /// // Just do this:
        /// AudioSource source = gameObject.GetOrAdd&lt;AudioSource&gt;();
        /// </example>
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component {
            T component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();

            return component;
        }
    }
}