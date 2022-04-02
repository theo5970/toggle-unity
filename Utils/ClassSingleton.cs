namespace Toggle.Utils
{
    public class ClassSingleton<T> where T : ClassSingleton<T>, new()
    {
        protected ClassSingleton()
        {
        }

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.Init();
                }

                return _instance;
            }
        }

        protected virtual void Init()
        {
        
        }
    
    
    }
}