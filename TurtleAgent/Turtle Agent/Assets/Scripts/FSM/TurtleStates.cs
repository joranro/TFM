public enum TurtleState
{
    Idle,           // Estado inicial, esperando input
    Moving,         // Moviéndose hacia adelante
    RotatingLeft,   // Girando a la izquierda
    RotatingRight,  // Girando a la derecha
    Navigating,     // Navegando autónomamente hacia el objetivo
    ReachedGoal,    // Ha llegado al objetivo
    Colliding       // Está colisionando con una pared
} 