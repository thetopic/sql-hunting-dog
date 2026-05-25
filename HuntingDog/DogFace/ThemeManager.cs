using System;

namespace HuntingDog.DogFace
{
    /// <summary>
    /// Pont statique entre la détection de thème SSMS (HuntingDog2022)
    /// et l'interface WPF (HuntingDog).
    /// HuntingDog2022 appelle <see cref="Apply"/> lorsqu'il détecte
    /// le thème courant ou un changement de thème.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>True si SSMS tourne en mode sombre.</summary>
        public static bool IsDark { get; private set; }

        /// <summary>
        /// Déclenché sur le thread UI lors d'un changement de thème.
        /// Le paramètre bool est true pour le mode sombre.
        /// </summary>
        public static event Action<bool> ThemeChanged;

        /// <summary>
        /// Appelé par HuntingDog2022 lors de la détection initiale
        /// ou d'un changement de thème VS/SSMS.
        /// </summary>
        public static void Apply(bool isDark)
        {
            IsDark = isDark;
            ThemeChanged?.Invoke(isDark);
        }
    }
}
