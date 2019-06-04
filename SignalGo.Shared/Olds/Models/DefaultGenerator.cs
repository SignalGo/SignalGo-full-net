namespace SignalGo.Shared.Models
{

    public class DefaultGenerator<T>
    {
        public static T GetDefault()
        {
            return default(T);
        }
    }

}
