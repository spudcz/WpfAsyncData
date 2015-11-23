using System;

namespace WpfAsyncDataSample.Model
{
    public class DataResult<TValue>
    {
        public TValue Result { get; set; }

        public Exception Exception { get; set; }
    }
}