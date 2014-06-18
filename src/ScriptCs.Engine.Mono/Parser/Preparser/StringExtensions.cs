namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    public static class StringExtensions
    {
        public static string PurgeExcept(this string self, string keep)
        {
            var result = new string(' ', self.Length);

            var tmp = self;
            int index = 0;
            while((index = tmp.IndexOf(keep)) != -1)
            {
                result = result.Remove(index, keep.Length);
                result = result.Insert(index, keep);

                tmp = tmp.Remove(index, keep.Length);
                tmp = tmp.Insert(index, new string(' ', keep.Length));
            }

            return result;
        }
    }
}

