namespace SignalGo.Shared.Http
{
    public static class ActionResultExtension
    {
        public static ActionResult ToActionResult(this object data)
        {
            if (data == null)
                return null;
            if ((data as ActionResult) != null)
                return (ActionResult)data;
            return new ActionResult(data);
        }
    }

    public class ActionResult
    {
        public object Data { get; private set; }

        public ActionResult(object data)
        {
            Data = data;
        }

        public static implicit operator ActionResult(string text)
        {
            return new ActionResult(text);
        }
    }
}
